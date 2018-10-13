using System;

namespace SqlQueryBuilder.Where
{
    public interface IQueryBuilderWhereOrBuild : IBuildQuery
    {
        IQueryBuilderWhereOrBuild Where(Func<ICompare, ICompareBuilder> compareFactory);
        IQueryBuilderWhereOrBuild WhereFactory(Func<IWhereBuilderFactory, IWhereBuilder> createBuilder);
    }
}
