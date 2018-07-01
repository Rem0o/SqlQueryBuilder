using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderSelectOnly<T>
    {
        IQueryBuilderSelect<T> Select<U>(Expression<Func<U, object>> lambda, string tableAlias = null);
        //ISelect<T> SelectAs<U, V>(string propertyAs, Expression<Func<U, V>> lambda, string tableAlias = null);
        IQueryBuilderSelect<T> SelectAll<U>(string tableAlias = null);
        IQueryBuilderSelect<T> SelectAggregateAs<U>(string aggregationFunc, Expression<Func<U, object>> lambda, string propertyAs, string tableAlias = null);
    }
}
