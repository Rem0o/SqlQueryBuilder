using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public class WhereFactory: SqlStatementFactory, IWhereBuilderFactory, IWhereBuilder
    {
        private string _whereClause { get; set; }

        public WhereFactory(Dictionary<string, Type> tables) : base(tables)
        {
        }

        public IWhereBuilder Compare<T>(Expression<Func<T, object>> lambda, string compare, string value, string tableAlias = null)
        {
            _whereClause = $"(({GetFirstSQL<T>(tableAlias, lambda)}) {compare} ({value}))";
            return this;
        }
            
        public IWhereBuilder Compare<T, U>(Expression<Func<T, object>> lambda1, string compare, Expression<Func<U, object>> lambda2,
            string table1Alias = null, string table2Alias = null) =>
            Compare(lambda1, compare, GetFirstSQL<T>(table2Alias, lambda2), table1Alias);

        public IWhereBuilder Or(params Func<IWhereBuilderFactory, IWhereBuilder>[] conditions) => JoinConditions("OR", conditions);

        public IWhereBuilder And(params Func<IWhereBuilderFactory, IWhereBuilder>[] conditions) => JoinConditions("AND", conditions);

        private IWhereBuilder JoinConditions(string compare, params Func<IWhereBuilderFactory, IWhereBuilder>[] clauses)
        {
            var clauseResults = clauses.Select(x => {
                var f = new WhereFactory(Tables);
                var builder = x(f);
                var success = builder.TryBuild(out string whereClause);
                return new { success, whereClause };
            });

            if (clauseResults.Any(x => x.success == false))
            {
                HasError = true;
                _whereClause = "";
                return this;
            }

            _whereClause = $"({string.Join($" {compare} ", clauseResults.Select(x => x.whereClause))})";
            return this;
        }         

        public bool TryBuild(out string whereClause)
        {
            whereClause = "";
            if (HasError)
                return false;

            whereClause = _whereClause;
            return true;
        }
    }
}
