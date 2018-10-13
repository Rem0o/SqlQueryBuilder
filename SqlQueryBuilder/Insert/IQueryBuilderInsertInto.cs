using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder.Insert
{
    public interface IQueryBuilderInsertInto
    {
        IQueryBuilderValues InsertInto<T>(Expression<Func<T, object>> lambda);
    }
}