using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderWhere<T>: IQueryBuilderGroupBy<T>
    {
        IQueryBuilderWhere<T> Where<U>(Expression<Func<U, object>> lambda, string compare, string value, string tableAlias = null);
        IQueryBuilderWhere<T> Where<U, V>(Expression<Func<U, object>> lambda1, string compare, Expression<Func<V, object>> p2, string table1Alias = null, string table2Alias = null);
    }
}
