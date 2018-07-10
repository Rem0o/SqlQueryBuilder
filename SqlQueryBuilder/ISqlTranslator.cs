using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface ISqlTranslator
    {
        string GetFirstTranslation<T>(Expression<Func<T, object>> lambda, string tableName);
        IEnumerable<string> Translate<T>(Expression<Func<T, object>> lambda, string tableName);
        string Translate<T>(string col, string tableAlias);
    }
}