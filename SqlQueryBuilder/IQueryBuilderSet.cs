using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderSet<T>
    {
        IQueryBuilderWhereOrBuild Set(Expression<Func<T, object>> lambda, string value, string tableAlias = null);
    }
}
