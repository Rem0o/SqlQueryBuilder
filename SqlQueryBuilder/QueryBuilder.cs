using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public class QueryBuilder<T> : IQueryBuilderFrom<T>, IQueryBuilderSelect<T>
    {
        private bool Error { get; set; } = false;
        private string From { get; set; }
        private Dictionary<string, Type> Tables = new Dictionary<string, Type>();
        private List<string> SelectClauses = new List<string>();
        private List<string> WhereClauses = new List<string>();
        private List<string> JoinClauses = new List<string>();
        private List<string> OrderByClauses = new List<string>();
        private List<string> GroupByClauses = new List<string>();

        private QueryBuilder(string tableAs)
        {
            From = tableAs;
            Tables.Add(tableAs, typeof(T));
        }

        private QueryBuilder<T> SkipIfError(Action action)
        {
            if (!Error)
                action();

            return this;
        }

        public static IQueryBuilderFrom<T> Start(string tableAs = null)
            => new QueryBuilder<T>(String.IsNullOrEmpty(tableAs) ? typeof(T).Name : tableAs);

        public IQueryBuilderFrom<T> Join<U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2,
            string tableAs = null, string joinType = "") => SkipIfError(() =>
        {
            var joinTableType = typeof(U);
            var joinTableName = tableAs ?? joinTableType.Name;

            Tables.Add(joinTableName, joinTableType);

            var joinClause = $"{(String.IsNullOrEmpty(joinType) ? "": $"{joinType} ")}JOIN [{joinTableType.Name}] {(tableAs != null ? $" AS [{tableAs}]" : "")}"
                + $"ON {GetFirstSQL<T>(From, key1)} = {GetFirstSQL<U>(joinTableName, key2)}";

            JoinClauses.Add(joinClause);
        });

        public IQueryBuilderFrom<T> LeftJoin<U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string tableAs = null)
            => Join(key1, key2, tableAs, "LEFT");

        public IQueryBuilderSelect<T> SelectAll<U>(string tableAs = null) =>
            SkipIfError(() =>
                SelectClauses.Add(GetSQL<U>(tableAs, "*"))
            );

        public IQueryBuilderSelect<T> SelectAll(string tableAs = null) => SelectAll<T>(tableAs);

        public IQueryBuilderSelect<T> Select<U>(Expression<Func<U, object>> lambda, string tableAs = null) =>
            SkipIfError(() =>
                SelectClauses.AddRange(GetSQL<U>(tableAs, lambda)));

        public IQueryBuilderSelect<T> Select(Expression<Func<T, object>> lambda, string tableAs = null) =>
            Select<T>(lambda, tableAs);

        /*
        public ISelect<T> SelectAs<U, V>(string propertyAs, Expression<Func<U, V>> lambda, string tableAs = null) =>
            SkipIfError(() =>
                SelectClauses.Add($"{GetFirstSQL<U>(tableAs, lambda)} as {propertyAs}"));

        public ISelect<T> SelectAs<U>(string propertyAs, Expression<Func<T, U>> lambda, string tableAs = null) =>
            SelectAs<T, U>(propertyAs, lambda, tableAs);
        */

        public IQueryBuilderSelect<T> SelectAggregateAs<U>(string aggregationFunc, Expression<Func<U, object>> lambda, string propertyAs, string tableAs = null) =>
            SkipIfError(() =>
                SelectClauses.Add($"{aggregationFunc}( {GetFirstSQL<U>(tableAs, lambda)} ) as {propertyAs}")
            );

        public IQueryBuilderSelect<T> SelectAggregateAs(string aggregationFunc, Expression<Func<T, object>> lambda, string propertyAs, string tableAs = null) =>
            SelectAggregateAs<T>(aggregationFunc, lambda, propertyAs, tableAs);

        public IQueryBuilderWhere<T> Where<U>(Expression<Func<U, object>> lambda, string compare, string value, string tableAs = null) =>
            SkipIfError(() =>
                WhereClauses.Add($"{GetFirstSQL<U>(tableAs, lambda)} {compare} {value}")
            );

        public IQueryBuilderWhere<T> Where(Expression<Func<T, object>> lambda, string compare, string value, string tableAs = null)
            => Where<T>(lambda, compare, value, tableAs);

        public IQueryBuilderGroupBy<T> GroupBy<U>(Expression<Func<U, object>> lambda, string tableAs = null) =>
            SkipIfError(() =>
                GroupByClauses.Add(GetFirstSQL<U>(tableAs, lambda))
            );

        public IQueryBuilderGroupBy<T> GroupBy(Expression<Func<T, object>> lambda, string tableAs = null) =>
            GroupBy<T>(lambda, tableAs);

        public IQueryBuilderOrderBy<T> OrderBy<U>(Expression<Func<U, object>> lambda, bool desc = false, string tableAs = null) =>
            SkipIfError(() =>
                OrderByClauses.Add($"{GetSQL<U>(tableAs, lambda)}{(desc ? " DESC" : "")}")
            );

        public IQueryBuilderOrderBy<T> OrderBy(Expression<Func<T, object>> lambda, bool desc = false, string tableAs = null) =>
            OrderBy<T>(lambda, desc, tableAs);

        public bool TryBuild(out string query)
        {
            query = "";

            if (Error)
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

        private string GetSQL<U>(string tableAs, string col)
        {
            KeyValuePair<string, Type> kv = Tables.FirstOrDefault(x =>
            {
                if (String.IsNullOrEmpty(tableAs))
                    return x.Value.Name == typeof(U).Name;
                else
                    return x.Key == tableAs;
            });

            if (kv.Key != null)
                return $"[{kv.Key}].{(col == "*" ? "*": $"[{col}]" )}";
            else
            {
                Error = true;
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
                return new string[] { };
        }
    }
}
