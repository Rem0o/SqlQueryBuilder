using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderWhere<T>: IQueryBuilderGroupBy<T>
    {
        IQueryBuilderWhere<T> Where(Expression<Func<T, object>> lambda, string compare, string value, string tableNameAs = null);
        IQueryBuilderWhere<T> Where<U>(Expression<Func<U, object>> lambda, string compare, string value, string tableNameAs = null);     
    }
}
