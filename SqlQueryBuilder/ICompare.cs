using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface ICompare
    {
        ICompareWith Compare(ISelectBuilder selectBuilder);
        ICompareWith Compare(string val);
        ICompareWith Compare<U>(Expression<Func<U, object>> lambda, string tableAlias = null);
    }

    public interface ICompareWith
    {
        ICompareBuilder With(string op, ISelectBuilder selectBuilder);
        ICompareBuilder With(string op, string val);
        ICompareBuilder With<U>(string op, Expression<Func<U, object>> lambda, string tableAlias = null);
    }
}