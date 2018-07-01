using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderJoinOrSelect: IQueryBuilderSelectOnly
    {
        IQueryBuilderJoinOrSelect LeftJoin<T, U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string table1Alias = null, string table2Alias = null);
        IQueryBuilderJoinOrSelect Join<T, U>(Expression<Func<T, object>> key1, Expression<Func<U, object>> key2, string table1Alias = null, string table2Alias = null, string joinType = null);
    }
}
