using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderJoinOrSelect<T>: IQueryBuilderSelectOnly<T>
    {
        IQueryBuilderJoinOrSelect<T> LeftJoin<U, V>(Expression<Func<U, object>> key1, Expression<Func<V, object>> key2, string table1Alias = null, string table2Alias = null);
        IQueryBuilderJoinOrSelect<T> Join<U, V>(Expression<Func<U, object>> key1, Expression<Func<V, object>> key2, string table1Alias = null, string table2Alias = null, string joinType = null);
    }
}
