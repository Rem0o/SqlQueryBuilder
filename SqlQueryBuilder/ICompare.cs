using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface ICompare
    {
        ICompareWith Compare(Func<ISqlTranslator, ISelectBuilder> selectBuilderFactory);
        ICompareWith Compare(string val);
        ICompareWith Compare<U>(Expression<Func<U, object>> lambda, string tableAlias = null);
    }

    public interface ICompareWith
    {
        string With(string op, Func<ISqlTranslator, ISelectBuilder> selectBuilderFactory);
        string With(string op, string val);
        string With<U>(string op, Expression<Func<U, object>> lambda, string tableAlias = null);
    }
}