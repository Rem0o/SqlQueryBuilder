using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface ISqlTranslator
    {
        bool HasError { get; }
        bool AddTable<T>(string tableAlias);
        string GetFirstTranslation<T, U>(Expression<Func<T, U>> lambda, string tableName);
        IEnumerable<string> Translate<T, U>(Expression<Func<T, U>> lambda, string tableName);
        string Translate<T>(string col, string tableAlias);
    }
}