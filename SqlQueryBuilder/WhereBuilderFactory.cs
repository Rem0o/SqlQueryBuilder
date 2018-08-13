using System;
using System.Linq;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public class WhereBuilderFactory: IWhereBuilderFactory, IWhereBuilder
    {
        private readonly ISqlTranslator translator;

        private string _whereClause { get; set; }

        public WhereBuilderFactory(ISqlTranslator translator)
        {
            this.translator = translator;
        }

        public IWhereBuilder Compare(Func<ICompare, string> compareFactory)
        {
            _whereClause = compareFactory(new Comparator(translator));
            return this;
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
