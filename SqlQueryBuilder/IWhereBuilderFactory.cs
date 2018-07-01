using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public interface IWhereBuilderFactory
    {
        IWhereBuilder Compare<T>(Expression<Func<T, object>> lambda, string compare, string value, string tableAlias = null);
        IWhereBuilder Compare<T, U>(Expression<Func<T, object>> lambda1, string compare, Expression<Func<U, object>> lambda2, string table1Alias = null, string table2Alias = null);
        IWhereBuilder Or(params Func<IWhereBuilderFactory, IWhereBuilder>[] conditions);
        IWhereBuilder And(params Func<IWhereBuilderFactory, IWhereBuilder>[] conditions);
    }
}
