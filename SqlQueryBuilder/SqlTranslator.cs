using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public class SqlTranslator : ISqlTranslator
    {
        public bool HasError { get; private set; } = false;
        private readonly Dictionary<string, Type> Tables = new Dictionary<string, Type>();

        public SqlTranslator()
        {
        }

        public bool AddTable<T>(string tableAlias)
        {
            if (Tables.ContainsKey(tableAlias))
            {
                HasError = true;
                return false;
            }
                
            Tables.Add(tableAlias, typeof(T));
            return true;
        }

        public string Translate<T>(string col, string tableAlias)
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

        public IEnumerable<string> Translate<T>(Expression<Func<T, object>> lambda, string tableName)
        {
            return NameOf(lambda).Select(x => Translate<T>(x, tableName));
        }

        public string GetFirstTranslation<T>(Expression<Func<T, object>> lambda, string tableName)
        {
            return NameOf(lambda).Select(x => Translate<T>(x, tableName)).FirstOrDefault();
        }

        private IEnumerable<string> NameOf(Expression expression)
        {
            if (expression is LambdaExpression lambda)
                return NameOf(lambda.Body);
            else if (expression is UnaryExpression unaryExpression)
                return NameOf((MemberExpression)unaryExpression.Operand);
            else if (expression is NewExpression newExpression)
                return ((NewExpression)expression).Members.Select(x => x.Name);
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
