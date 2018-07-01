using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public class SqlQueryBuilder : IQueryBuilderFrom, IQueryBuilderJoinOrSelect, IQueryBuilderSelect
    {
        private bool HasError { get; set; } = false;
        private Tuple<string, Type> TableFrom { get; set; }
        private Dictionary<string, Type> Tables = new Dictionary<string, Type>();
        private List<string> SelectClauses = new List<string>();
        private List<string> SelectAggregateClauses = new List<string>();
        private List<string> WhereClauses = new List<string>();
        private List<string> JoinClauses = new List<string>();
        private List<string> OrderByClauses = new List<string>();
        private List<string> GroupByClauses = new List<string>();

        private SqlQueryBuilder SkipIfError(Action action)
        {
            if (!HasError)
                action();

            return this;
        }

        public IQueryBuilderJoinOrSelect From<T>(string tableAlias = null)
        {
            var type = typeof(T);
            var tableAliasKey = tableAlias ?? type.Name;
            TableFrom = new Tuple<string, Type>(tableAliasKey, type);
            Tables.Add(tableAliasKey, type);
            return this;
        }

        public IQueryBuilderJoinOrSelect Join<T, U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string table1Alias = null,
            string table2Alias = null, string joinType = null) => SkipIfError(() =>
        {
            var first = GetFirstSQL<T>(table1Alias, key1);
            var joinTable2Type = typeof(U);
            var joinTable2Name = string.IsNullOrEmpty(table2Alias) ? joinTable2Type.Name : table2Alias;

            if (Tables.ContainsKey(joinTable2Name))
            {
                HasError = true;
                return;
            }

            Tables.Add(joinTable2Name, joinTable2Type);

            var joinTypeStr = (string.IsNullOrEmpty(joinType) ? string.Empty : $"{joinType} ") + "JOIN";
            var joinTableStr = $"[{joinTable2Type.Name}]" + (!string.IsNullOrEmpty(table2Alias) ? $" AS [{table2Alias}]" : string.Empty);
            var joinOnStr = $"{first} = {GetFirstSQL<T>(joinTable2Name, key2)}";

            var joinClause = $"{joinTypeStr} {joinTableStr} ON {joinOnStr}";

            JoinClauses.Add(joinClause);
        });

        public IQueryBuilderJoinOrSelect LeftJoin<T, U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string table1Alias = null,
            string table2Alias = null)
            => Join(key1, key2, table1Alias, table2Alias, "LEFT");

        public IQueryBuilderSelect SelectAll<T>(string tableAlias = null) =>
            SkipIfError(() =>
                SelectClauses.Add(GetSQL<T>(tableAlias, "*"))
            );

        public IQueryBuilderSelect Select<T>(Expression<Func<T, object>> lambda, string tableAlias = null) =>
            SkipIfError(() =>
                SelectClauses.AddRange(GetSQL<T>(tableAlias, lambda))
            );

        public IQueryBuilderSelect SelectAggregateAs<T>(string aggregationFunc, Expression<Func<T, object>> lambda, string propertyAs, string tableAlias = null) =>
            SkipIfError(() =>
                SelectAggregateClauses.Add($"{aggregationFunc}({GetFirstSQL<T>(tableAlias, lambda)}) AS [{propertyAs}]")
            );

        private IQueryBuilderWhere Where(string value1, string compare, string value2) =>
            SkipIfError(() =>
                WhereClauses.Add($"{value1} {compare} {value2}")
            );

        public IQueryBuilderWhere Where<T>(Expression<Func<T, object>> lambda, string compare, string value, string tableAlias = null) =>
            Where(GetFirstSQL<T>(tableAlias, lambda), compare, value);

        public IQueryBuilderWhere Where<T, U>(Expression<Func<T, object>> lambda1, string compare, Expression<Func<U, object>> lambda2,
            string table1Alias = null, string table2Alias = null) =>
            Where(GetFirstSQL<T>(table1Alias, lambda1), compare, GetFirstSQL<U>(table2Alias, lambda2));

        public IQueryBuilderGroupBy GroupBy<T>(Expression<Func<T, object>> lambda, string tableAlias = null) =>
            SkipIfError(() =>
                GroupByClauses.Add(GetFirstSQL<T>(tableAlias, lambda))
            );

        public IQueryBuilderOrderBy OrderBy<T>(Expression<Func<T, object>> lambda, bool desc = false, string tableAlias = null) =>
            SkipIfError(() =>
                OrderByClauses.Add($"{GetSQL<T>(tableAlias, lambda)}{(desc ? " DESC" : string.Empty)}")
            );

        public bool TryBuild(out string query)
        {
            query = string.Empty;

            if (Validate() == false)
                return false;

            var tableName = TableFrom.Item2.Name;
            var tableAlias = TableFrom.Item1;

            const string separator = ", ";
            var selectString = string.Join(separator, SelectAggregateClauses.Concat(SelectClauses));

            query = $"SELECT {selectString} FROM [{tableName}] {(tableAlias != tableName ? $"AS [{TableFrom.Item1}] " : string.Empty)}"
                + (JoinClauses.Count > 0 ? string.Join(" ", JoinClauses) + " " : string.Empty)
                + (WhereClauses.Count > 0 ? $"WHERE {string.Join(" AND ", WhereClauses)} " : string.Empty)
                + (GroupByClauses.Count > 0 ? $"GROUP BY {string.Join(separator, GroupByClauses)} " : string.Empty)
                + (OrderByClauses.Count > 0 ? $"ORDER BY {string.Join(separator, OrderByClauses)} " : string.Empty);

            return true;
        }

        private bool Validate()
        {
            if (HasError)
                return false;

            var missingGroupBy = SelectAggregateClauses.Count > 0 && SelectClauses
                .Any(select => GroupByClauses.Any(group => select.Contains(group)) == false);

            if (missingGroupBy)
                return false;

            return true;
        }

        private string GetSQL<T>(string tableAlias, string col)
        {
            KeyValuePair<string, Type> kv = Tables.FirstOrDefault(x =>
            {
                if (string.IsNullOrEmpty(tableAlias))
                    return x.Value.Name == typeof(T).Name;
                else
                    return x.Key == tableAlias;
            });

            if (kv.Key != null)
                return $"[{kv.Key}].{(col == "*" ? "*" : $"[{col}]")}";
            else
            {
                HasError = true;
                return string.Empty;
            }
        }

        private IEnumerable<string> GetSQL<T>(string tableName, Expression expression) =>
            NameOf(expression).Select(x => GetSQL<T>(tableName, x));

        private string GetFirstSQL<T>(string tableName, Expression expression) =>
            NameOf(expression).Select(x => GetSQL<T>(tableName, x)).FirstOrDefault();

        private IEnumerable<string> NameOf(Expression expression)
        {
            if (expression is LambdaExpression lambda)
                return NameOf(lambda.Body);
            else if (expression is NewExpression newExpression)
                return ((NewExpression)expression).Members.Select(x => x.Name);
            else if (expression is UnaryExpression unaryExpression)
                return new[] { ((MemberExpression)unaryExpression.Operand).Member.Name };
            else if (expression is MemberExpression memberExpression)
                return new[] { memberExpression.Member.Name };
            else
            {
                HasError = true;
                return new string[] { };
            }
        }
    }
}
