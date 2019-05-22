using SqlQueryBuilder.Select;
using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder.Where
{
    public class Comparator : ICompare, ICompareWith, ICompareBuilder
    {
        private delegate bool TranslateDelegate(ISqlTranslator translator, out string str);

        private TranslateDelegate _first;
        private TranslateDelegate _second;
        private string _operator;

        public ICompareWith Compare(string val)
        {
            bool translateFunction (ISqlTranslator translator, out string str)
            {
                str = val;
                return !translator.HasError;
            }

            _first = translateFunction;
            return this;
        }

        public ICompareWith Compare<U>(Expression<Func<U, object>> lambda, string tableAlias = null)
        {
            bool translateFunction(ISqlTranslator translator, out string str)
            {
                str = translator.GetFirstTranslation(typeof(U), lambda, tableAlias);
                return !translator.HasError;
            }

            _first = translateFunction;
            return this;
        }

        public ICompareWith Compare(ISelectBuilder selectBuilder)
        {
            bool translateFunction(ISqlTranslator translator, out string str)
            {
                return selectBuilder.TryBuild(translator, out str) && !translator.HasError;
            }

            _first = translateFunction;
            return this;
        }

        public ICompareBuilder With(string op, string val)
        {
            _operator = op;
            bool translateFunction(ISqlTranslator translator, out string str)
            {
                str = val;
                return !translator.HasError;
            }

            _second = translateFunction;
            return this;
        }

        public ICompareBuilder With<U>(string op, Expression<Func<U, object>> lambda, string tableAlias = null)
        {
            _operator = op;
            bool translateFunction(ISqlTranslator translator, out string str)
            {
                str = translator.GetFirstTranslation(typeof(U), lambda, tableAlias);
                return !translator.HasError;
            }

            _second = translateFunction;
            return this;
        }

        public ICompareBuilder With(string op, ISelectBuilder selectBuilder)
        {
            _operator = op;
            bool translateFunction(ISqlTranslator translator, out string str)
            {
                return selectBuilder.TryBuild(translator, out str) && !translator.HasError;
            }

            _second = translateFunction;
            return this;
        }

        private string WrapWithParentheses(string str) => $"({str})";

        public bool TryBuild(ISqlTranslator translator, out string comparison)
        {
            comparison = string.Empty;

            if (
                !_first(translator, out string str1)  || 
                !_second(translator, out string str2) || 
                string.IsNullOrEmpty(_operator))
            {
                return false;
            }

            comparison = WrapWithParentheses($"{WrapWithParentheses(str1)} {_operator} {WrapWithParentheses(str2)}");
            return true;
        }
    }
}
