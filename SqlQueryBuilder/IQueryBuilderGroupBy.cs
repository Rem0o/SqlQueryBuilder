using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderGroupBy<T>: IQueryBuilderOrderBy<T>
    {
        IQueryBuilderGroupBy<T> GroupBy<U>(Expression<Func<U, object>> lambda, string tableNameAs = null);
        IQueryBuilderGroupBy<T> GroupBy(Expression<Func<T, object>> lambda, string tableNameAs = null);
    }
}