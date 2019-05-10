using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder.Select
{
    public interface IQueryBuilderOrderBy: IBuildQuery
    {
        IQueryBuilderOrderBy OrderBy<T>(Expression<Func<T, object>> lambda, bool desc = false, string tableNameAs = null);
        IQueryBuilderOrderBy SkipTake(int skip, int take);
    }
}
