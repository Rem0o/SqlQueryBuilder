using System;
using System.Linq;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public class WhereBuilderFactory: IWhereBuilderFactory, IWhereBuilder
    {
        private readonly Func<ICompare> _compareFactory;

        private Func<ISqlTranslator, string> _whereExpression { get; set; }

        public WhereBuilderFactory(Func<ICompare> compareFactory)
        {
            this._compareFactory = compareFactory;
        }

        public IWhereBuilder Compare(Func<ICompare, ICompareBuilder> compareBuilderFactory)
        {
            _whereExpression = t =>
            {
                if (compareBuilderFactory(_compareFactory()).TryBuild(t, out string comparison))
                    return comparison;
                return string.Empty;
            };

            return this;
        }

        public IWhereBuilder Or(params Func<IWhereBuilderFactory, IWhereBuilder>[] conditions) => JoinConditions("OR", conditions);

        public IWhereBuilder And(params Func<IWhereBuilderFactory, IWhereBuilder>[] conditions) => JoinConditions("AND", conditions);

        private IWhereBuilder JoinConditions(string compare, params Func<IWhereBuilderFactory, IWhereBuilder>[] clauses)
        {
            _whereExpression = t =>
            {
                var res = clauses.Select(condition =>
                {
                    var success = condition(new WhereBuilderFactory(_compareFactory)).TryBuild(t, out string whereClause);
                    return new { success, whereClause };
                });

                if (res.Any(r => !r.success))
                    return "";
                else
                    return $"({string.Join($" {compare} ", res.Select(x => x.whereClause))})";
            };
            
            return this;
        }         

        public bool TryBuild(ISqlTranslator translator, out string whereClause)
        {
            whereClause = string.Empty;
            if (translator.HasError)
                return false;

            whereClause = _whereExpression(translator);
            if (string.IsNullOrEmpty(whereClause))
                return false;

            return true;
        }
    }
}
