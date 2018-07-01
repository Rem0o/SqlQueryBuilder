using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{ 
    public interface IQueryBuilderFrom
    {
        IQueryBuilderJoinOrSelect From<T>(string tableAlias = null);
    }
}