using SqlQueryBuilder.Where;
using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder.Update
{
    public interface IQueryBuilderSet<T>
    {
        IQueryBuilderWhereOrBuild Set(Expression<Func<T, object>> lambda, string value, string tableAlias = null);
    }
}
