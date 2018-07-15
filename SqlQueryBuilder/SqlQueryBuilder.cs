using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public class SqlQueryBuilder : IQueryBuilderFrom, IQueryBuilderJoinOrSelect, IQueryBuilderSelect
    {
        private KeyValuePair<string, Type> TableFrom { get; set; }
        private SqlTranslator Translator = new SqlTranslator();
        private int TopClause = 0;
        private List<string> SelectClauses = new List<string>();
        private List<string> SelectAggregateClauses = new List<string>();
        private List<string> WhereClauses = new List<string>();
        private List<string> JoinClauses = new List<string>();
        private List<string> OrderByClauses = new List<string>();
        private List<string> GroupByClauses = new List<string>();

        private SqlQueryBuilder SkipIfError(Action action)
        {
            if (!Translator.HasError)
                action();

            return this;
        }

        public IQueryBuilderJoinOrSelect From<T>(string tableAlias = null)
        {
            var type = typeof(T);
            var tableAliasKey = tableAlias ?? type.Name;
            TableFrom = new KeyValuePair<string, Type>(tableAliasKey, type);

            Translator.AddTable<T>(TableFrom.Key);
            return this;
        }

        public IQueryBuilderJoinOrSelect Join<T, U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string table1Alias = null,
            string table2Alias = null, string joinType = null) => SkipIfError(() =>
        {
            var joinTable2Type = typeof(U);
            var joinTable2Name = string.IsNullOrEmpty(table2Alias) ? joinTable2Type.Name : table2Alias;

            if (Translator.AddTable<U>(joinTable2Name) == false)
                return;

            var joinTypeStr = (string.IsNullOrEmpty(joinType) ? string.Empty : $"{joinType} ") + "JOIN";
            var joinTableStr = $"[{joinTable2Type.Name}]" + (!string.IsNullOrEmpty(table2Alias) ? $" AS [{table2Alias}]" : string.Empty);
            var joinOnStr = $"{Translator.GetFirstTranslation<T>(key1, table1Alias)} = {Translator.GetFirstTranslation<U>(key2, joinTable2Name)}";
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

        public IQueryBuilderSelect Top(int i) => SkipIfError(() =>
        {
            TopClause = i;
        });
        
        public IQueryBuilderSelect SelectAll<T>(string tableAlias = null) =>
            SkipIfError(() =>
                SelectClauses.Add(Translator.Translate<T>("*", tableAlias))
            );

        public IQueryBuilderSelect Select<T>(Expression<Func<T, object>> lambda, string tableAlias = null) =>
            SkipIfError(() =>
                SelectClauses.AddRange(Translator.Translate<T>(lambda, tableAlias))
            );

        public IQueryBuilderSelect SelectAggregateAs<T>(string aggregationFunc, Expression<Func<T, object>> lambda, string propertyAs, string tableAlias = null) =>
            SkipIfError(() =>
                SelectAggregateClauses.Add($"{aggregationFunc}({Translator.GetFirstTranslation<T>(lambda, tableAlias)}) AS [{propertyAs}]")
            );

        private IQueryBuilderWhere Where(string value1, string compare, string value2) =>
            SkipIfError(() =>
                WhereClauses.Add($"({value1} {compare} {value2})")
            );

        public IQueryBuilderWhere Where<T>(Expression<Func<T, object>> lambda, string compare, string value, string tableAlias = null)
        {
            return Where(Translator.GetFirstTranslation<T>(lambda, tableAlias), compare, value);
        }
            
        public IQueryBuilderWhere Where<T, U>(Expression<Func<T, object>> lambda1, string compare, Expression<Func<U, object>> lambda2,
            string table1Alias = null, string table2Alias = null)
        {
            return Where(Translator.GetFirstTranslation<T>(lambda1, table1Alias), compare, Translator.GetFirstTranslation<U>(lambda2, table2Alias));
        }
            
        public IQueryBuilderWhere Where(Func<IWhereBuilderFactory, IWhereBuilder> createBuilder) =>
            SkipIfError(() =>
            {
                var factory = new WhereBuilderFactory(Translator);
                var success = createBuilder(factory).TryBuild(out var whereClause);
                if (success == true)
                    WhereClauses.Add(whereClause);
            });

        public IQueryBuilderGroupBy GroupBy<T>(Expression<Func<T, object>> lambda, string tableAlias = null) =>
            SkipIfError(() =>
                GroupByClauses.Add(Translator.GetFirstTranslation<T>(lambda, tableAlias))
            );

        public IQueryBuilderOrderBy OrderBy<T>(Expression<Func<T, object>> lambda, bool desc = false, string tableAlias = null) =>
            SkipIfError(() =>
                OrderByClauses.Add($"{Translator.GetFirstTranslation<T>(lambda, tableAlias)}{(desc ? " DESC" : string.Empty)}")
            );

        public bool TryBuild(out string query)
        {
            query = string.Empty;

            if (Validate() == false)
                return false;

            var tableName = TableFrom.Value.Name;
            var tableAlias = TableFrom.Key;

            const string separator = ", ";
            var selectString = string.Join(separator, SelectAggregateClauses.Concat(SelectClauses));

            query = $"SELECT {(TopClause > 0 ? $"{TopClause} ": "")}{selectString} FROM [{tableName}] {(tableAlias != tableName ? $"AS [{tableAlias}] " : string.Empty)}"
                + (JoinClauses.Count > 0 ? string.Join(" ", JoinClauses) + " " : string.Empty)
                + (WhereClauses.Count > 0 ? $"WHERE {string.Join(" AND ", WhereClauses)} " : string.Empty)
                + (GroupByClauses.Count > 0 ? $"GROUP BY {string.Join(separator, GroupByClauses)} " : string.Empty)
                + (OrderByClauses.Count > 0 ? $"ORDER BY {string.Join(separator, OrderByClauses)} " : string.Empty);

            return true;
        }

        private bool Validate()
        {
            if (Translator.HasError)
                return false;

            if (SelectClauses.Count == 0)
                return false;

            if (TopClause < 0)
                return false;

            var missingGroupBy = SelectAggregateClauses.Count > 0 && SelectClauses
                .Any(select => GroupByClauses.Any(group => select.Contains(group)) == false);

            if (missingGroupBy)
                return false;

            return true;
        }
    }
}
