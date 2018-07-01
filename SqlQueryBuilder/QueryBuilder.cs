using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public class QueryBuilder<T> : IQueryBuilderFrom<T>, IQueryBuilderSelect<T>
    {
        private bool HasError { get; set; } = false;
        private string From { get; set; }
        private Dictionary<string, Type> Tables = new Dictionary<string, Type>();
        private List<string> SelectClauses = new List<string>();
        private List<string> WhereClauses = new List<string>();
        private List<string> JoinClauses = new List<string>();
        private List<string> OrderByClauses = new List<string>();
        private List<string> GroupByClauses = new List<string>();

        private QueryBuilder(string tableAlias)
        {
            From = tableAlias;
            Tables.Add(tableAlias, typeof(T));
        }

        private QueryBuilder<T> SkipIfError(Action action)
        {
            if (!HasError)
                action();

            return this;
        }

        public static IQueryBuilderFrom<T> Start(string tableAlias = null)
            => new QueryBuilder<T>(string.IsNullOrEmpty(tableAlias) ? typeof(T).Name : tableAlias);

        public IQueryBuilderFrom<T> Join<U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2,
            string tableAlias = null, string joinType = null) => SkipIfError(() =>
        {
            var joinTableType = typeof(U);
            var joinTableName = tableAlias ?? joinTableType.Name;

            Tables.Add(joinTableName, joinTableType);

            var joinClause = string.IsNullOrEmpty(joinType) ? "" : $"{joinType} " + "JOIN "
                + $"[{joinTableType.Name}]" + tableAlias != null ? $" AS [{tableAlias}]" : ""
                + $"ON {GetFirstSQL<T>(From, key1)} = {GetFirstSQL<U>(joinTableName, key2)}";

            JoinClauses.Add(joinClause);
        });

        public IQueryBuilderFrom<T> LeftJoin<U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string tableAlias = null)
            => Join(key1, key2, tableAlias, "LEFT");

        public IQueryBuilderSelect<T> SelectAll<U>(string tableAlias = null) =>
            SkipIfError(() =>
                SelectClauses.Add(GetSQL<U>(tableAlias, "*"))
            );

        public IQueryBuilderSelect<T> SelectAll(string tableAlias = null) => SelectAll<T>(tableAlias);

        public IQueryBuilderSelect<T> Select<U>(Expression<Func<U, object>> lambda, string tableAlias = null) =>
            SkipIfError(() =>
                SelectClauses.AddRange(GetSQL<U>(tableAlias, lambda)));

        public IQueryBuilderSelect<T> Select(Expression<Func<T, object>> lambda, string tableAlias = null) =>
            Select<T>(lambda, tableAlias);

        /*
        public ISelect<T> SelectAs<U, V>(string propertyAs, Expression<Func<U, V>> lambda, string tableAlias = null) =>
            SkipIfError(() =>
                SelectClauses.Add($"{GetFirstSQL<U>(tableAlias, lambda)} as {propertyAs}"));

        public ISelect<T> SelectAs<U>(string propertyAs, Expression<Func<T, U>> lambda, string tableAlias = null) =>
            SelectAs<T, U>(propertyAs, lambda, tableAlias);
        */

        public IQueryBuilderSelect<T> SelectAggregateAs<U>(string aggregationFunc, Expression<Func<U, object>> lambda, string propertyAs, string tableAlias = null) =>
            SkipIfError(() =>
                SelectClauses.Add($"{aggregationFunc}( {GetFirstSQL<U>(tableAlias, lambda)} ) as {propertyAs}")
            );

        public IQueryBuilderSelect<T> SelectAggregateAs(string aggregationFunc, Expression<Func<T, object>> lambda, string propertyAs, string tableAlias = null) =>
            SelectAggregateAs<T>(aggregationFunc, lambda, propertyAs, tableAlias);

        public IQueryBuilderWhere<T> Where<U>(Expression<Func<U, object>> lambda, string compare, string value, string tableAlias = null) =>
            SkipIfError(() =>
                WhereClauses.Add($"{GetFirstSQL<U>(tableAlias, lambda)} {compare} {value}")
            );

        public IQueryBuilderWhere<T> Where(Expression<Func<T, object>> lambda, string compare, string value, string tableAlias = null)
            => Where<T>(lambda, compare, value, tableAlias);

        public IQueryBuilderGroupBy<T> GroupBy<U>(Expression<Func<U, object>> lambda, string tableAlias = null) =>
            SkipIfError(() =>
                GroupByClauses.Add(GetFirstSQL<U>(tableAlias, lambda))
            );

        public IQueryBuilderGroupBy<T> GroupBy(Expression<Func<T, object>> lambda, string tableAlias = null) =>
            GroupBy<T>(lambda, tableAlias);

        public IQueryBuilderOrderBy<T> OrderBy<U>(Expression<Func<U, object>> lambda, bool desc = false, string tableAlias = null) =>
            SkipIfError(() =>
                OrderByClauses.Add($"{GetSQL<U>(tableAlias, lambda)}{(desc ? " DESC" : "")}")
            );

        public IQueryBuilderOrderBy<T> OrderBy(Expression<Func<T, object>> lambda, bool desc = false, string tableAlias = null) =>
            OrderBy<T>(lambda, desc, tableAlias);

        public bool TryBuild(out string query)
        {
            query = "";

            if (HasError)
                return false;

            var tableName = typeof(T).Name;
            var selectString = string.Join(",", SelectClauses);

            query = $"SELECT {selectString} FROM [{typeof(T).Name}] {(From != tableName ? $" AS [{From}] " : "")}"
                + (JoinClauses.Count > 0 ? string.Join(" ", JoinClauses) + " " : "")
                + (WhereClauses.Count > 0 ? $"WHERE {string.Join(" AND ", WhereClauses)} " : "")
                + (GroupByClauses.Count > 0 ? $"GROUP BY {string.Join(",", GroupByClauses)} " : "")
                + (OrderByClauses.Count > 0 ? $"ORDER BY {string.Join(",", OrderByClauses)} " : "");

            return true;
        }

        private string GetSQL<U>(string tableAlias, string col)
        {
            KeyValuePair<string, Type> kv = Tables.FirstOrDefault(x =>
            {
                if (String.IsNullOrEmpty(tableAlias))
                    return x.Value.Name == typeof(U).Name;
                else
                    return x.Key == tableAlias;
            });

            if (kv.Key != null)
                return $"[{kv.Key}].{(col == "*" ? "*": $"[{col}]" )}";
            else
            {
                HasError = true;
                return "";
            }
        }

        private IEnumerable<string> GetSQL<U>(string tableName, Expression expression) =>
            NameOf(expression).Select(x => GetSQL<U>(tableName, x));

        private string GetFirstSQL<U>(string tableName, Expression expression) =>
            NameOf(expression).Select(x => GetSQL<U>(tableName, x)).FirstOrDefault();

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
