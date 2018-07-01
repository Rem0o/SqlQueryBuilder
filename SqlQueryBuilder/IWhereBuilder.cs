using System;
using System.Collections.Generic;

namespace SqlQueryBuilder
{
    public interface IWhereBuilder
    {
        bool TryBuild(out string whereClause);
    }
}