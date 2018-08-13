using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderSelectOnly
    {
        IQueryBuilderSelect Top(int i);
        IQueryBuilderSelect Select<T>(Expression<Func<T, object>> lambda, string tableAlias = null);
        IQueryBuilderSelect SelectAll<T>(string tableAlias = null);
        IQueryBuilderSelect SelectAs(Func<ISqlTranslator, ISelectBuilder> selectBuilderFactory, string alias);
    }
}
