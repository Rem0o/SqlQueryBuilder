﻿using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder.Select
{
    public interface IQueryBuilderOrderBy : IBuildQuery
    {
        IQueryBuilderOrderBy OrderBy<T>(Expression<Func<T, object>> lambda, bool desc = false, string tableNameAs = null);
        IQueryBuilderOrderBy Skip(int skip);
        IQueryBuilderOrderBy Take(int take);
    }
}
