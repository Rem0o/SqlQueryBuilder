using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderWhere: IQueryBuilderGroupBy
    {
        IQueryBuilderWhere Where(Func<ICompare, string> compareFactory);
        IQueryBuilderWhere WhereFactory(Func<ISqlTranslator, IWhereBuilder> build);
    }
}
