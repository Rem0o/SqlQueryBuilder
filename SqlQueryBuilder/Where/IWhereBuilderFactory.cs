using System;

namespace SqlQueryBuilder.Where
{
    public interface IWhereBuilderFactory
    {
        IWhereBuilder Compare(Func<ICompare, ICompareBuilder> compareFactory);
        IWhereBuilder Or(params Func<IWhereBuilderFactory, IWhereBuilder>[] conditions);
        IWhereBuilder And(params Func<IWhereBuilderFactory, IWhereBuilder>[] conditions);
    }
}
