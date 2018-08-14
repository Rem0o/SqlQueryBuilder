using System;
using System.Linq;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public class Comparator : ICompare, ICompareWith, ICompareBuilder
    {
        private Func<ISqlTranslator, string> _first;
        private Func<ISqlTranslator, string> _second;
        private string _operator;

        public ICompareWith Compare(string val)
        {
            _first = _ => val;
            return this;
        }

        public ICompareWith Compare<U>(Expression<Func<U, object>> lambda, string tableAlias = null)
        {
            _first = t => t.GetFirstTranslation(typeof(U), lambda, tableAlias);
            return this;
        }

        public ICompareWith Compare(ISelectBuilder selectBuilder)
        {
            _first = t => {
                selectBuilder.TryBuild(t, out string str);
                return str;
            };

            return this;
        }

        public ICompareBuilder With(string op, string val)
        {
            _operator = op;
            _second = _ => val;
            return this;
        }

        public ICompareBuilder With<U>(string op, Expression<Func<U, object>> lambda, string tableAlias = null)
        {
            _operator = op;
            _second = t => t.GetFirstTranslation(typeof(U), lambda, tableAlias);
            return this;
        }

        public ICompareBuilder With(string op, ISelectBuilder selectBuilder)
        {
            _operator = op;
            _second = t => {
                selectBuilder.TryBuild(t, out string str);
                return str;
            };

            return this;
        }

        private string WrapWithParentheses(string str) => $"({str})";

        public bool TryBuild(ISqlTranslator translator, out string comparison)
        {
            comparison = "";
            string val1 = _first(translator);
            string val2 = _second(translator);

            if (new[] { val1, val2, _operator }.Any(string.IsNullOrEmpty))
                return false;

            comparison = WrapWithParentheses($"{WrapWithParentheses(val1)} {_operator} {WrapWithParentheses(val2)}");
            return true;
        }
    }
}
