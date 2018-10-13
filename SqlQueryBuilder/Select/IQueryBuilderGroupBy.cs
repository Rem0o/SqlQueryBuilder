using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder.Select
{ 
    public interface IQueryBuilderGroupBy: IQueryBuilderOrderBy
    {
        IQueryBuilderGroupBy GroupBy<T>(Expression<Func<T, object>> lambda, string tableAlias = null);
    }
}