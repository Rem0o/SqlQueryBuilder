using System;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderWhere: IQueryBuilderGroupBy
    {
        IQueryBuilderWhere Where(Func<ICompare, ICompareBuilder> compareFactory);
        IQueryBuilderWhere WhereFactory(Func<IWhereBuilderFactory, IWhereBuilder> createBuilder);
    }

    public interface IQueryBuilderWhereOrBuild : IBuildQuery
    {
        IQueryBuilderWhereOrBuild Where(Func<ICompare, ICompareBuilder> compareFactory);
        IQueryBuilderWhereOrBuild WhereFactory(Func<IWhereBuilderFactory, IWhereBuilder> createBuilder);
    }
}
