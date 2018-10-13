using SqlQueryBuilder.Where;
using System;

namespace SqlQueryBuilder.Select
{
    public interface IQueryBuilderWhere: IQueryBuilderGroupBy
    {
        IQueryBuilderWhere Where(Func<ICompare, ICompareBuilder> compareFactory);
        IQueryBuilderWhere WhereFactory(Func<IWhereBuilderFactory, IWhereBuilder> createBuilder);
    }
}
