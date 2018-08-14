using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderWhere: IQueryBuilderGroupBy
    {
        IQueryBuilderWhere Where(Func<ICompare, ICompareBuilder> compareFactory);
        IQueryBuilderWhere WhereFactory(Func<IWhereBuilderFactory, IWhereBuilder> createBuilder);
    }
}
