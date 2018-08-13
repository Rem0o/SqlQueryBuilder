using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IWhereBuilderFactory
    {
        IWhereBuilder Compare(Func<ICompare, string> compareFactory);
        IWhereBuilder Or(params Func<IWhereBuilderFactory, IWhereBuilder>[] conditions);
        IWhereBuilder And(params Func<IWhereBuilderFactory, IWhereBuilder>[] conditions);
    }
}
