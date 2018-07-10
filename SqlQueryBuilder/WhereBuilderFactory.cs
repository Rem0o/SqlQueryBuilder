using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public class WhereBuilderFactory: IWhereBuilderFactory, IWhereBuilder
    {
        private readonly SqlTranslator translator;

        private string _whereClause { get; set; }

        public WhereBuilderFactory(SqlTranslator translator)
        {
            this.translator = translator;
        }

        public IWhereBuilder Compare<T>(Expression<Func<T, object>> lambda, string compare, string value, string tableAlias = null)
        {
            _whereClause = $"(({translator.GetFirstTranslation<T>(lambda, tableAlias)}) {compare} ({value}))";
            return this;
        }

        public IWhereBuilder Compare<T, U>(Expression<Func<T, object>> lambda1, string compare, Expression<Func<U, object>> lambda2,
            string table1Alias = null, string table2Alias = null)
        {
            return Compare(lambda1, compare, translator.GetFirstTranslation<U>(lambda2, table2Alias), table1Alias);
        }

        public IWhereBuilder Or(params Func<IWhereBuilderFactory, IWhereBuilder>[] conditions) => JoinConditions("OR", conditions);

        public IWhereBuilder And(params Func<IWhereBuilderFactory, IWhereBuilder>[] conditions) => JoinConditions("AND", conditions);

        private IWhereBuilder JoinConditions(string compare, params Func<IWhereBuilderFactory, IWhereBuilder>[] clauses)
        {
            var clauseResults = clauses.Select(condition => {
                var success = condition(new WhereBuilderFactory(translator)).TryBuild(out string whereClause);
                return new { success, whereClause };
            });

            if (clauseResults.Any(x => x.success == false))
            {
                _whereClause = "";
                return this;
            }

            _whereClause = $"({string.Join($" {compare} ", clauseResults.Select(x => x.whereClause))})";
            return this;
        }         

        public bool TryBuild(out string whereClause)
        {
            whereClause = "";
            if (translator.HasError)
                return false;

            whereClause = _whereClause;
            return true;
        }
    }
}
