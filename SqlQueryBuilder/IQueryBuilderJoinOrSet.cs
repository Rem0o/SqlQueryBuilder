using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderJoinOrSet<T> : IQueryBuilderSet<T>
    {
        IQueryBuilderJoinOrSet<T> Join<U, V>(Expression<Func<U, object>> key1, Expression<Func<V, object>> key2, string table1Alias = null, string table2Alias = null, string joinType = null);
        IQueryBuilderJoinOrSet<T> LeftJoin<U, V>(Expression<Func<U, object>> key1, Expression<Func<V, object>> key2, string table1Alias = null, string table2Alias = null);
        IQueryBuilderJoinOrSet<T> RightJoin<U, V>(Expression<Func<U, object>> key1, Expression<Func<V, object>> key2, string table1Alias = null, string table2Alias = null);
        IQueryBuilderJoinOrSet<T> FullOuterJoin<U, V>(Expression<Func<U, object>> key1, Expression<Func<V, object>> key2, string table1Alias = null, string table2Alias = null);
    }
}
