using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderWhere: IQueryBuilderGroupBy
    {
        IQueryBuilderWhere Where<T>(Expression<Func<T, object>> lambda, string compare, string value, string tableAlias = null);
        IQueryBuilderWhere Where<T, U>(Expression<Func<T, object>> lambda1, string compare, Expression<Func<U, object>> p2, string table1Alias = null, string table2Alias = null);
    }
}
