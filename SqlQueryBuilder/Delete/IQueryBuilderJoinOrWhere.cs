using SqlQueryBuilder.Where;
using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder.Delete
{
    public interface IQueryBuilderJoinOrWhere: IQueryBuilderWhereOrBuild
    {
        IQueryBuilderJoinOrWhere Join<T, U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string table1Alias = null, string table2Alias = null, string joinType = null);
        IQueryBuilderJoinOrWhere LeftJoin<T, U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string table1Alias = null, string table2Alias = null);
        IQueryBuilderJoinOrWhere RightJoin<T, U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string table1Alias = null, string table2Alias = null);
        IQueryBuilderJoinOrWhere FullOuterJoin<T, U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string table1Alias = null, string table2Alias = null);
    }
}