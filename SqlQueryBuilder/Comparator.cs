using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public class Comparator : ICompare, ICompareWith
    {
        private readonly List<string> _list = new List<string>();
        private readonly ISqlTranslator translator;

        public Comparator (ISqlTranslator translator)
        {
            this.translator = translator;
        }

        public ICompareWith Compare(string val)
        {
            _list.Add(WrapWithParentheses(val));
            return this;
        }

        public ICompareWith Compare<U>(Expression<Func<U, object>> lambda, string tableAlias = null)
        {
            _list.Add(WrapWithParentheses(this.translator.GetFirstTranslation(lambda, null)));
            return this;
        }

        public ICompareWith Compare(Func<ISqlTranslator, ISelectBuilder> selectBuilderFactory)
        {
            selectBuilderFactory(this.translator).TryBuild(out string str);
            _list.Add(WrapWithParentheses(str));
            return this;
        }

        public string With(string op, string val)
        {
            _list.Add(op);
            Compare(val);
            return JoinList();
        }

        public string With<U>(string op, Expression<Func<U, object>> lambda, string tableAlias = null)
        {
            _list.Add(op);
            Compare(lambda, tableAlias);
            return JoinList();
        }

        public string With(string op, Func<ISqlTranslator, ISelectBuilder> selectBuilderFactory)
        {
            _list.Add(op);
            Compare(selectBuilderFactory);
            return JoinList();
        }

        private string WrapWithParentheses(string str) => $"({str})";
        private string JoinList() => WrapWithParentheses(string.Join(" ", _list));
    }
}
