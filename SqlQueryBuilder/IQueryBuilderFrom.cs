using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderFrom<T>: IQueryBuilderSelectOnly<T>
    {
        IQueryBuilderFrom<T> LeftJoin<U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string tableNameAs = null);
        IQueryBuilderFrom<T> Join<U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string tableNameAs = null, string joinType = null);
    }
}
