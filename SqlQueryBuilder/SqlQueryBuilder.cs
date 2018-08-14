using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public class Builder : IQueryBuilderFrom, IQueryBuilderJoinOrSelect, IQueryBuilderSelectOrWhere
    {
        private readonly Func<IWhereBuilderFactory> _createWhereBuilderFactory;
        private readonly Func<ICompare> _compareFactory;
        private ISqlTranslator _translator = new SqlTranslator();

        private KeyValuePair<string, Type> TableFrom { get; set; }
        private int TopClause = 0;
        private List<string> SelectClauses = new List<string>();
        private List<string> WhereClauses = new List<string>();
        private List<string> JoinClauses = new List<string>();
        private List<string> OrderByClauses = new List<string>();
        private List<string> GroupByClauses = new List<string>();

        private Builder SkipIfError(Action action)
        {
            if (!_translator.HasError)
                action();

            return this;
        }

        public Builder(ISqlTranslator translator, Func<IWhereBuilderFactory> createWhereBuilderFactory, Func<ICompare> compareFactory)
        {
            this._translator = translator;
            this._createWhereBuilderFactory = createWhereBuilderFactory;
            this._compareFactory = compareFactory;
        }

        public IQueryBuilderJoinOrSelect From<T>(string tableAlias = null)
        {
            var type = typeof(T);
            var tableAliasKey = tableAlias ?? type.Name;
            TableFrom = new KeyValuePair<string, Type>(tableAliasKey, type);
            _translator.AddTable(type, TableFrom.Key);
            return this;
        }

        public IQueryBuilderJoinOrSelect Join<T, U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string table1Alias = null,
            string table2Alias = null, string joinType = null) => SkipIfError(() =>
        {
            var table1Type = typeof(T);
            var joinTable2Type = typeof(U);
            var joinTable2Name = string.IsNullOrEmpty(table2Alias) ? joinTable2Type.Name : table2Alias;

            if (_translator.AddTable(joinTable2Type, joinTable2Name) == false)
                return;

            var joinTypeStr = (string.IsNullOrEmpty(joinType) ? string.Empty : $"{joinType} ") + "JOIN";
            var joinTableStr = $"[{joinTable2Type.Name}]" + (!string.IsNullOrEmpty(table2Alias) ? $" AS [{table2Alias}]" : string.Empty);
            var joinOnStr = $"{_translator.GetFirstTranslation(table1Type, key1, table1Alias)} = {_translator.GetFirstTranslation(joinTable2Type, key2, joinTable2Name)}";
            var joinClause = $"{joinTypeStr} {joinTableStr} ON {joinOnStr}";

            JoinClauses.Add(joinClause);
        });

        public IQueryBuilderJoinOrSelect LeftJoin<T, U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string table1Alias = null,
            string table2Alias = null)
        {
            return Join(key1, key2, table1Alias, table2Alias, "LEFT");
        }

        public IQueryBuilderJoinOrSelect RightJoin<T, U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string table1Alias = null,
            string table2Alias = null)
        {
            return Join(key1, key2, table1Alias, table2Alias, "RIGHT");
        }

        public IQueryBuilderJoinOrSelect FullOuterJoin<T, U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string table1Alias = null,
            string table2Alias = null)
        {
            return Join(key1, key2, table1Alias, table2Alias, "FULL OUTER");
        }

        public IQueryBuilderSelectOrWhere Top(int i) => SkipIfError(() =>
        {
            TopClause = i;
        });
        
        public IQueryBuilderSelectOrWhere SelectAll<T>(string tableAlias = null) =>
            SkipIfError(() =>
                SelectClauses.Add(_translator.Translate(typeof(T), "*", tableAlias))
            );

        public IQueryBuilderSelectOrWhere Select<T>(Expression<Func<T, object>> lambda, string tableAlias = null) =>
            SkipIfError(() =>
                SelectClauses.AddRange(_translator.Translate(typeof(T), lambda, tableAlias))
            );

        public IQueryBuilderSelectOrWhere SelectAs(ISelectBuilder selectBuilder, string alias) =>
            SkipIfError(() =>
            {
                var success = selectBuilder
                    .TryBuild(_translator, out string selectClause);

                if (success)
                    SelectClauses.Add($"{selectClause} AS [{alias}]");
            });

        public IQueryBuilderWhere Where(Func<ICompare, ICompareBuilder> compareBuilderFactory) =>
            SkipIfError(() =>
            {
                var success = compareBuilderFactory(_compareFactory())
                    .TryBuild(_translator, out string comparison);

                if (success)
                    WhereClauses.Add(comparison);   
            });
            
        public IQueryBuilderWhere WhereFactory(Func<IWhereBuilderFactory, IWhereBuilder> whereBuilder) =>
            SkipIfError(() =>
            {
                var success = whereBuilder(_createWhereBuilderFactory()).TryBuild(_translator, out var whereClause);

                if (success)
                    WhereClauses.Add(whereClause); 
            });

        public IQueryBuilderGroupBy GroupBy<T>(Expression<Func<T, object>> lambda, string tableAlias = null) =>
            SkipIfError(() =>
                GroupByClauses.Add(_translator.GetFirstTranslation(typeof(T), lambda, tableAlias))
            );

        public IQueryBuilderOrderBy OrderBy<T>(Expression<Func<T, object>> lambda, bool desc = false, string tableAlias = null) =>
            SkipIfError(() =>
                OrderByClauses.Add($"{_translator.GetFirstTranslation(typeof(T), lambda, tableAlias)}{(desc ? " DESC" : string.Empty)}")
            );

        public bool TryBuild(out string query)
        {
            query = string.Empty;

            if (Validate() == false)
                return false;

            var tableName = TableFrom.Value.Name;
            var tableAlias = TableFrom.Key;

            const string separator = ", ";
            var selectString = string.Join(separator, SelectClauses);

            query = $"SELECT {(TopClause > 0 ? $"TOP {TopClause} ": "")}{selectString} FROM [{tableName}] {(tableAlias != tableName ? $"AS [{tableAlias}] " : string.Empty)}"
                + (JoinClauses.Count > 0 ? string.Join(" ", JoinClauses) + " " : string.Empty)
                + (WhereClauses.Count > 0 ? $"WHERE {string.Join(" AND ", WhereClauses)} " : string.Empty)
                + (GroupByClauses.Count > 0 ? $"GROUP BY {string.Join(separator, GroupByClauses)} " : string.Empty)
                + (OrderByClauses.Count > 0 ? $"ORDER BY {string.Join(separator, OrderByClauses)} " : string.Empty);

            query = query.Trim();

            return true;
        }

        private bool Validate()
        {
            if (_translator.HasError)
                return false;

            if (SelectClauses.Count == 0)
                return false;

            if (TopClause < 0)
                return false;

            return true;
        }
    }
}
