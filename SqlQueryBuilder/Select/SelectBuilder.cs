using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder.Select
{
    public abstract class SelectBuilderBase : ISelectBuilder
    {
        protected abstract string CreateClause(ISqlTranslator translator);

        public bool TryBuild(ISqlTranslator translator, out string selectClause)
        {
            selectClause = "";
            if (translator.HasError)
                return false;

            selectClause = CreateClause(translator);
            if (string.IsNullOrEmpty(selectClause))
                return false;

            return true;
        }
    }

    public abstract class SelectBuilder<T> : SelectBuilderBase
    {
        private Func<ISqlTranslator, string> _selectExpression;

        public ISelectBuilder Select<A>(Expression<Func<A, T>> exp, string tableAlias = null)
        {
            _selectExpression = t => t.GetFirstTranslation(typeof(A), exp, tableAlias);
            return this;
        }

        protected override string CreateClause(ISqlTranslator translator) => CreateClause(_selectExpression(translator));

        protected abstract string CreateClause(string selector);
    }

    public abstract class SelectBuilder<T, U> : SelectBuilderBase
    {
        private Func<ISqlTranslator, string> _firstSelectExpression;
        private Func<ISqlTranslator, string> _secondSelectExpression;

        public ISelectBuilder Select<A, B>(Expression<Func<A, T>> exp1, Expression<Func<B, U>> exp2, string tableAlias1 = null, string tableAlias2 = null)
        {
            _firstSelectExpression = t => t.GetFirstTranslation(typeof(A), exp1, tableAlias1);
            _secondSelectExpression = t => t.GetFirstTranslation(typeof(B), exp2, tableAlias2);
            return this;
        }

        protected override string CreateClause(ISqlTranslator translator) => 
            CreateClause(_firstSelectExpression(translator), _secondSelectExpression(translator));

        protected abstract string CreateClause(string selector1, string selector2);
    }

    public abstract class SelectBuilder<T, U, V> : SelectBuilderBase
    {
        private Func<ISqlTranslator, string> _firstSelectExpression;
        private Func<ISqlTranslator, string> _secondSelectExpression;
        private Func<ISqlTranslator, string> _thirdSelectExpression;

        public ISelectBuilder Select<A, B, C>(Expression<Func<A, T>> exp1, Expression<Func<B, U>> exp2, Expression<Func<C, V>> exp3,
            string tableAlias1 = null, string tableAlias2 = null, string tableAlias3 = null)
        {
            _firstSelectExpression = t => t.GetFirstTranslation(typeof(A), exp1, tableAlias1);
            _secondSelectExpression = t => t.GetFirstTranslation(typeof(A), exp2, tableAlias2);
            _thirdSelectExpression = t => t.GetFirstTranslation(typeof(A), exp2, tableAlias3); ;
            return this;
        }

        protected override string CreateClause(ISqlTranslator translator) => 
            CreateClause(_firstSelectExpression(translator), _secondSelectExpression(translator), _thirdSelectExpression(translator));

        protected abstract string CreateClause(string selector1, string selector2, string selector3);
    }
}
