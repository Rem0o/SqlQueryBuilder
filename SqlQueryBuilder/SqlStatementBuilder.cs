using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public abstract class SqlStatementFactory
    {
        protected bool HasError { get; set; } = false;
        protected readonly Dictionary<string, Type> Tables = new Dictionary<string, Type>();

        protected SqlStatementFactory()
        {
        }

        protected SqlStatementFactory(Dictionary<string, Type> tables)
        {
            foreach (var kv in tables)
                Tables.Add(kv.Key, kv.Value);
        }

        protected string GetSQL<T>(string tableAlias, string col)
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

        protected IEnumerable<string> GetSQL<T>(string tableName, Expression expression)
        {
            return NameOf(expression).Select(x => GetSQL<T>(tableName, x));
        }
            
        protected string GetFirstSQL<T>(string tableName, Expression expression)
        {
            return NameOf(expression).Select(x => GetSQL<T>(tableName, x)).FirstOrDefault();
        }

        protected IEnumerable<string> NameOf(Expression expression)
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
