using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public static class SqlQueryBuilder
    {
        public static IQueryBuilderJoinOrSelect<T> StartFrom<T>(string tableAlias = null)
            => new SqlQueryBuilder<T>(string.IsNullOrEmpty(tableAlias) ? typeof(T).Name : tableAlias);
    }

    public class SqlQueryBuilder<T> : IQueryBuilderJoinOrSelect<T>, IQueryBuilderSelect<T>
    {
        private bool HasError { get; set; } = false;
        private string From { get; set; }
        private Dictionary<string, Type> Tables = new Dictionary<string, Type>();
        private List<string> SelectClauses = new List<string>();
        private List<string> WhereClauses = new List<string>();
        private List<string> JoinClauses = new List<string>();
        private List<string> OrderByClauses = new List<string>();
        private List<string> GroupByClauses = new List<string>();

        public SqlQueryBuilder(string tableAlias)
        {
            From = tableAlias;
            Tables.Add(tableAlias, typeof(T));
        }

        private SqlQueryBuilder<T> SkipIfError(Action action)
        {
            if (!HasError)
                action();

            return this;
        }

        public IQueryBuilderJoinOrSelect<T> Join<U, V>(Expression<Func<U, object>> key1, Expression<Func<V, object>> key2, string table1Alias = null,
            string table2Alias = null, string joinType = null) => SkipIfError(() =>
        {
            var first = GetFirstSQL<U>(table1Alias, key1);
            var joinTable2Type = typeof(V);
            var joinTable2Name = string.IsNullOrEmpty(table2Alias) ? joinTable2Type.Name : table2Alias;

            /*try
            {*/
            Tables.Add(joinTable2Name, joinTable2Type);
            /*}
            catch (ArgumentException)
            {
                HasError = true;
                return;
            }*/

            var joinTypeStr = (string.IsNullOrEmpty(joinType) ? "" : $"{joinType} ") + "JOIN";
            var joinTableStr = $"[{joinTable2Type.Name}]" + (!string.IsNullOrEmpty(table2Alias) ? $" AS [{table2Alias}]" : "");
            var joinOnStr = $"{first} = {GetFirstSQL<U>(joinTable2Name, key2)}";

            var joinClause = $"{joinTypeStr} {joinTableStr} ON {joinOnStr}";

            JoinClauses.Add(joinClause);
        });

        public IQueryBuilderJoinOrSelect<T> LeftJoin<U, V>(Expression<Func<U, object>> key1, Expression<Func<V, object>> key2, string table1Alias = null,
            string table2Alias = null)
            => Join(key1, key2, table1Alias, table2Alias, "LEFT");

        public IQueryBuilderSelect<T> SelectAll<U>(string tableAlias = null) =>
            SkipIfError(() =>
                SelectClauses.Add(GetSQL<U>(tableAlias, "*"))
            );

        public IQueryBuilderSelect<T> Select<U>(Expression<Func<U, object>> lambda, string tableAlias = null) =>
            SkipIfError(() =>
                SelectClauses.AddRange(GetSQL<U>(tableAlias, lambda))
            );

        /*
        public ISelect<T> SelectAs<U, V>(string propertyAs, Expression<Func<U, V>> lambda, string tableAlias = null) =>
            SkipIfError(() =>
                SelectClauses.Add($"{GetFirstSQL<U>(tableAlias, lambda)} as {propertyAs}"));
        */

        public IQueryBuilderSelect<T> SelectAggregateAs<U>(string aggregationFunc, Expression<Func<U, object>> lambda, string propertyAs, string tableAlias = null) =>
            SkipIfError(() =>
                SelectClauses.Add($"{aggregationFunc}( {GetFirstSQL<U>(tableAlias, lambda)} ) as {propertyAs}")
            );

        public IQueryBuilderWhere<T> Where<U>(Expression<Func<U, object>> lambda, string compare, string value, string tableAlias = null) =>
            SkipIfError(() =>
                WhereClauses.Add($"{GetFirstSQL<U>(tableAlias, lambda)} {compare} {value}")
            );

        public IQueryBuilderGroupBy<T> GroupBy<U>(Expression<Func<U, object>> lambda, string tableAlias = null) =>
            SkipIfError(() =>
                GroupByClauses.Add(GetFirstSQL<U>(tableAlias, lambda))
            );

        public IQueryBuilderOrderBy<T> OrderBy<U>(Expression<Func<U, object>> lambda, bool desc = false, string tableAlias = null) =>
            SkipIfError(() =>
                OrderByClauses.Add($"{GetSQL<U>(tableAlias, lambda)}{(desc ? " DESC" : "")}")
            );

        public bool TryBuild(out string query)
        {
            query = "";

            if (HasError)
                return false;

            var tableName = typeof(T).Name;
            var selectString = string.Join(",", SelectClauses);

            query = $"SELECT {selectString} FROM [{typeof(T).Name}] {(From != tableName ? $"AS [{From}] " : "")}"
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
                if (string.IsNullOrEmpty(tableAlias))
                    return x.Value.Name == typeof(U).Name;
                else
                    return x.Key == tableAlias;
            });

            if (kv.Key != null)
                return $"[{kv.Key}].{(col == "*" ? "*" : $"[{col}]")}";
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
