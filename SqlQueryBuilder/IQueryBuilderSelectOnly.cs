using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderSelectOnly
    {
        IQueryBuilderSelect Select<T>(Expression<Func<T, object>> lambda, string tableAlias = null);
        IQueryBuilderSelect SelectAll<T>(string tableAlias = null);
        IQueryBuilderSelect SelectAggregateAs<T>(string aggregationFunc, Expression<Func<T, object>> lambda, string propertyAs, string tableAlias = null);
    }
}
