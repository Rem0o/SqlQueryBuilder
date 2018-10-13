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

        public bool AddTable(Type type, string tableAlias = null)
        {
            var alias = tableAlias ?? type.Name;
            if (Tables.ContainsKey(alias))
            {
                HasError = true;
                return false;
            }
                
            Tables.Add(alias, type);
            return true;
        }

        public string Translate(Type type, string col, string tableAlias)
        {
            KeyValuePair<string, Type> kv = Tables.FirstOrDefault(x =>
            {
                if (string.IsNullOrEmpty(tableAlias))
                    return x.Value.Name == type.Name;
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

        public IEnumerable<string> Translate(Type type, Expression expression, string tableAlias)
        {
            return NameOf(expression).Select(x => Translate(type, x, tableAlias));
        }

        public string GetFirstTranslation(Type type, Expression expression, string tableAlias)
        {
            return NameOf(expression).Select(x => Translate(type, x, tableAlias)).FirstOrDefault();
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
