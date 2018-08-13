using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public class SqlQueryBuilder : IQueryBuilderFrom, IQueryBuilderJoinOrSelect, IQueryBuilderSelect
    {
        private KeyValuePair<string, Type> TableFrom { get; set; }
        private ISqlTranslator Translator = new SqlTranslator();
        private int TopClause = 0;
        private List<string> SelectClauses = new List<string>();
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
            var joinOnStr = $"{Translator.GetFirstTranslation(key1, table1Alias)} = {Translator.GetFirstTranslation(key2, joinTable2Name)}";
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
                SelectClauses.AddRange(Translator.Translate(lambda, tableAlias))
            );

        public IQueryBuilderSelect SelectAs(Func<ISqlTranslator, ISelectBuilder> selectBuilderFactory, string alias) =>
            SkipIfError(() =>
            {
                var success = selectBuilderFactory(Translator)
                    .TryBuild(out string selectClause);
                if (success)
                    SelectClauses.Add($"{selectClause} AS [{alias}]");
            });

        public IQueryBuilderWhere Where(Func<ICompare, string> compareFactory) =>
            SkipIfError(() =>
                WhereClauses.Add(compareFactory(new Comparator(Translator)))
            );
            
        public IQueryBuilderWhere WhereFactory(Func<ISqlTranslator, IWhereBuilder> createBuilder) =>
            SkipIfError(() =>
            {
                var success = createBuilder(Translator).TryBuild(out var whereClause);
                if (success)
                    WhereClauses.Add(whereClause); 
            });

        public IQueryBuilderGroupBy GroupBy<T>(Expression<Func<T, object>> lambda, string tableAlias = null) =>
            SkipIfError(() =>
                GroupByClauses.Add(Translator.GetFirstTranslation(lambda, tableAlias))
            );

        public IQueryBuilderOrderBy OrderBy<T>(Expression<Func<T, object>> lambda, bool desc = false, string tableAlias = null) =>
            SkipIfError(() =>
                OrderByClauses.Add($"{Translator.GetFirstTranslation(lambda, tableAlias)}{(desc ? " DESC" : string.Empty)}")
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
            if (Translator.HasError)
                return false;

            if (SelectClauses.Count == 0)
                return false;

            if (TopClause < 0)
                return false;

            return true;
        }
    }
}
