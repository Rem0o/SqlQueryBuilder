using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public class UpdateQueryBuilder : IQueryBuilderUpdateFrom
    {
        private readonly ISqlTranslator _translator;
        private readonly Func<IWhereBuilderFactory> _createWhereBuilderFactory;
        private readonly Func<ICompare> _compareFactory;

        public UpdateQueryBuilder(ISqlTranslator translator, Func<IWhereBuilderFactory> createWhereBuilderFactory, Func<ICompare> compareFactory)
        {
            _translator = translator;
            _createWhereBuilderFactory = createWhereBuilderFactory;
            _compareFactory = compareFactory;
        }

        public IQueryBuilderJoinOrSet<T> From<T>(string tableAlias = null)
        {
            return new UpdateQueryBuilder<T>(_translator, _createWhereBuilderFactory, _compareFactory, tableAlias);
        }
    }

    public class UpdateQueryBuilder<T>: IQueryBuilderJoinOrSet<T>, IQueryBuilderWhereOrBuild
    {
        private KeyValuePair<string, Type> TableFrom;
        private List<string> JoinClauses = new List<string>();
        private List<string> SetClauses = new List<string>();
        private List<string> WhereClauses = new List<string>();
        private readonly ISqlTranslator _translator;
        private readonly Func<IWhereBuilderFactory> _createWhereBuilderFactory;
        private readonly Func<ICompare> _compareFactory;

        private UpdateQueryBuilder<T> SkipIfError(Action action)
        {
            if (!_translator.HasError)
                action();

            return this;
        }

        public UpdateQueryBuilder(ISqlTranslator translator, Func<IWhereBuilderFactory> createWhereBuilderFactory, Func<ICompare> compareFactory, string tableAlias = null)
        {
            _translator = translator;
            _createWhereBuilderFactory = createWhereBuilderFactory;
            _compareFactory = compareFactory;

            var type = typeof(T);
            var tableAliasKey = tableAlias ?? type.Name;
            TableFrom = new KeyValuePair<string, Type>(tableAliasKey, type);
            _translator.AddTable(type, TableFrom.Key);
        }

        public IQueryBuilderJoinOrSet<T> Join<U, V>(Expression<Func<U, object>> key1, Expression<Func<V, object>> key2, string table1Alias = null,
          string table2Alias = null, string joinType = null) => SkipIfError(() =>
          {
              var table1Type = typeof(U);
              var joinTable2Type = typeof(V);
              var joinTable2Name = string.IsNullOrEmpty(table2Alias) ? joinTable2Type.Name : table2Alias;

              if (_translator.AddTable(joinTable2Type, joinTable2Name) == false)
                  return;

              var joinTypeStr = (string.IsNullOrEmpty(joinType) ? string.Empty : $"{joinType}") + "JOIN";
              var joinTableStr = $"[{joinTable2Type.Name}]" + (!string.IsNullOrEmpty(table2Alias) ? $" AS [{table2Alias}]" : string.Empty);
              var joinOnStr = $"{_translator.GetFirstTranslation(table1Type, key1, table1Alias)} = {_translator.GetFirstTranslation(joinTable2Type, key2, joinTable2Name)}";
              var joinClause = $"{joinTypeStr} {joinTableStr} ON {joinOnStr}";

              JoinClauses.Add(joinClause);
          });

        public IQueryBuilderJoinOrSet<T> LeftJoin<U, V>(Expression<Func<U, object>> key1, Expression<Func<V, object>> key2, string table1Alias = null,
            string table2Alias = null)
        {
            return Join(key1, key2, table1Alias, table2Alias, "LEFT");
        }

        public IQueryBuilderJoinOrSet<T> RightJoin<U, V>(Expression<Func<U, object>> key1, Expression<Func<V, object>> key2, string table1Alias = null,
            string table2Alias = null)
        {
            return Join(key1, key2, table1Alias, table2Alias, "RIGHT");
        }

        public IQueryBuilderJoinOrSet<T> FullOuterJoin<U, V>(Expression<Func<U, object>> key1, Expression<Func<V, object>> key2, string table1Alias = null,
            string table2Alias = null)
        {
            return Join(key1, key2, table1Alias, table2Alias, "FULL OUTER");
        }

        public IQueryBuilderWhereOrBuild Set(Expression<Func<T, object>> lambda, string value, string tableAlias = null) 
            => SkipIfError(() =>
            {
                SetClauses.Add($"{_translator.GetFirstTranslation(typeof(T), lambda, tableAlias)} = {value}");
            });

        public IQueryBuilderWhereOrBuild Where(Func<ICompare, ICompareBuilder> compareBuilderFactory) =>
            SkipIfError(() =>
            {
                var success = compareBuilderFactory(_compareFactory())
                    .TryBuild(_translator, out string comparison);

                if (success)
                    WhereClauses.Add(comparison);
            });

        public IQueryBuilderWhereOrBuild WhereFactory(Func<IWhereBuilderFactory, IWhereBuilder> whereBuilder) =>
            SkipIfError(() =>
            {
                var success = whereBuilder(_createWhereBuilderFactory()).TryBuild(_translator, out var whereClause);

                if (success)
                    WhereClauses.Add(whereClause);
            });

        public bool TryBuild(out string query)
        {
            query = string.Empty;

            if (Validate() == false)
                return false;

            var tableName = TableFrom.Value.Name;
            var tableAlias = TableFrom.Key;

            const string separator = ", ";
            var setString = string.Join(separator, SetClauses);

            query = $"UPDATE [{tableAlias}] SET {setString} FROM [{tableName}] [{tableAlias}] "
                + (JoinClauses.Count > 0 ? string.Join(" ", JoinClauses) + " " : string.Empty)
                + (WhereClauses.Count > 0 ? $"WHERE {string.Join(" AND ", WhereClauses)} " : string.Empty);

            query = query.Trim();

            return true;
        }

        private bool Validate()
        {
            if (_translator.HasError)
                return false;

            if (SetClauses.Count == 0)
                return false;

            return true;
        }
    }
}