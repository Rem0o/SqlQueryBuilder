using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderOrderBy: IBuildQuery
    {
        IQueryBuilderOrderBy OrderBy<T>(Expression<Func<T, object>> lambda, bool desc = false, string tableNameAs = null);
    }
}
