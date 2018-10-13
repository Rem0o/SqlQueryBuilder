using SqlQueryBuilder.Where;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SqlQueryBuilder.Delete
{
    internal class DeleteQueryBuilder : IQueryBuilderDeleteFrom, IQueryBuilderJoinOrWhere
    {
        private ISqlTranslator _translator;
        private Func<IWhereBuilderFactory> _createWhereBuilderFactory;
        private Func<ICompare> _compareFactory;
        private KeyValuePair<string, Type> TableFrom;
        private List<string> JoinClauses = new List<string>();
        private List<string> WhereClauses = new List<string>();

        private DeleteQueryBuilder SkipIfError(Action action)
        {
            if (!_translator.HasError)
                action();

            return this;
        }

        public DeleteQueryBuilder(ISqlTranslator translator, Func<IWhereBuilderFactory> createWhereBuilderFactory, Func<ICompare> compareFactory)
        {
            _translator = translator;
            _createWhereBuilderFactory = createWhereBuilderFactory;
            _compareFactory = compareFactory;
        }

        public IQueryBuilderJoinOrWhere DeleteFrom<T>(string tableAlias = null)
        {
            var type = typeof(T);
            var tableAliasKey = type.Name;
            TableFrom = new KeyValuePair<string, Type>(tableAliasKey, type);
            _translator.AddTable(type, TableFrom.Key);

            return this;
        }

        public IQueryBuilderJoinOrWhere Join<T, U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string table1Alias = null, string table2Alias = null, string joinType = null)
            => SkipIfError(() =>
            {
                var table1Type = typeof(T);
                var joinTable2Type = typeof(U);
                var joinTable2Name = string.IsNullOrEmpty(table2Alias) ? joinTable2Type.Name : table2Alias;

                if (_translator.AddTable(joinTable2Type, joinTable2Name) == false)
                    return;

                var joinTypeStr = (string.IsNullOrEmpty(joinType) ? string.Empty : $"{joinType}") + "JOIN";
                var joinTableStr = $"[{joinTable2Type.Name}]" + (!string.IsNullOrEmpty(table2Alias) ? $" AS [{table2Alias}]" : string.Empty);
                var joinOnStr = $"{_translator.GetFirstTranslation(table1Type, key1, table1Alias)} = {_translator.GetFirstTranslation(joinTable2Type, key2, joinTable2Name)}";
                var joinClause = $"{joinTypeStr} {joinTableStr} ON {joinOnStr}";

                JoinClauses.Add(joinClause);
            });

        public IQueryBuilderJoinOrWhere FullOuterJoin<T, U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string table1Alias = null, string table2Alias = null)
        {
            return Join(key1, key2, table1Alias, table2Alias, "FULL OUTER");
        }

        public IQueryBuilderJoinOrWhere LeftJoin<T, U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string table1Alias = null, string table2Alias = null)
        {
            return Join(key1, key2, table1Alias, table2Alias, "LEFT");
        }

        public IQueryBuilderJoinOrWhere RightJoin<T, U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string table1Alias = null, string table2Alias = null)
        {
            return Join(key1, key2, table1Alias, table2Alias, "RIGHT");
        }

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

            if (!Validate())
                return false;

            var tableName = TableFrom.Value.Name;
            var tableAlias = TableFrom.Key;

            query = $"DELETE FROM [{tableName}] {(tableAlias != tableName ? $"[{tableAlias}]" : string.Empty)}"
                + (JoinClauses.Count > 0 ? string.Join(" ", JoinClauses) + " " : string.Empty)
                + (WhereClauses.Count > 0 ? $"WHERE {string.Join(" AND ", WhereClauses)} " : string.Empty);

            return true;
        }

        private bool Validate() => !_translator.HasError;
    }
}