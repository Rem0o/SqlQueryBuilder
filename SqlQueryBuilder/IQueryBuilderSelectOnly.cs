using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderSelectOnly
    {
        IQueryBuilderSelectOrWhere Top(int i);
        IQueryBuilderSelectOrWhere Select<T>(Expression<Func<T, object>> lambda, string tableAlias = null);
        IQueryBuilderSelectOrWhere SelectAll<T>(string tableAlias = null);
        IQueryBuilderSelectOrWhere SelectAs(ISelectBuilder selectBuilder, string alias);
    }
}
