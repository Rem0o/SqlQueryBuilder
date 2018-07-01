using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderSelectOnly<T>
    {
        IQueryBuilderSelect<T> Select(Expression<Func<T, object>> lambda, string tableAlias = null);
        IQueryBuilderSelect<T> Select<U>(Expression<Func<U, object>> lambda, string tableAlias = null);
        //ISelect<T> SelectAs<U>(string propertyAs, Expression<Func<T, U>> lambda, string tableAlias = null);
        //ISelect<T> SelectAs<U, V>(string propertyAs, Expression<Func<U, V>> lambda, string tableAlias = null);
        IQueryBuilderSelect<T> SelectAll(string tableAlias = null);
        IQueryBuilderSelect<T> SelectAll<U>(string tableAlias = null);
        IQueryBuilderSelect<T> SelectAggregateAs(string aggregationFunc, Expression<Func<T, object>> lambda, string propertyAs,  string tableAlias = null);
        IQueryBuilderSelect<T> SelectAggregateAs<U>(string aggregationFunc, Expression<Func<U, object>> lambda, string propertyAs, string tableAlias = null);
    }
}
