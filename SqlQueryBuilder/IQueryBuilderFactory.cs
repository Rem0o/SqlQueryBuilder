using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderFactory
    {
        IQueryBuilderSelectFrom GetSelect();
        IQueryBuilderUpdateFrom GetUpdate();
        IQueryBuilderInsertInto GetInsert();
    }

    public interface IQueryBuilderSelectFrom
    {
        IQueryBuilderJoinOrSelect From<T>(string tableAlias = null);
    }

    public interface IQueryBuilderUpdateFrom
    {
        IQueryBuilderJoinOrSet<T> From<T>(string tableAlias = null);
    }

    public interface IQueryBuilderInsertInto
    {
        IQueryBuilderValues InsertInto<T>(Expression<Func<T, object>> lambda);
    }
}