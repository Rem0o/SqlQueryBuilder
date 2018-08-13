using System;
using System.Linq.Expressions;

namespace SqlQueryBuilder
{
    public abstract class SelectBuilderBase : ISelectBuilder
    {
        protected ISqlTranslator _translator;
        protected abstract string CreateClause();

        public SelectBuilderBase(ISqlTranslator translator)
        {
            _translator = translator;
        }

        public bool TryBuild(out string selectClause)
        {
            selectClause = "";
            if (this._translator.HasError)
                return false;

            selectClause = CreateClause();
            return true;
        }
    }

    public abstract class SelectBuilder<T> : SelectBuilderBase
    {
        private string _selectExpression;

        public SelectBuilder(ISqlTranslator translator) : base(translator) { }

        public ISelectBuilder Select<A>(Expression<Func<A, T>> exp, string tableAlias = null)
        {
            _selectExpression = _translator.GetFirstTranslation(exp, tableAlias);
            return this;
        }

        protected override string CreateClause() => CreateClause(_selectExpression);

        protected abstract string CreateClause(string selector);
    }

    public abstract class SelectBuilder<T, U> : SelectBuilderBase
    {
        private string _firstSelectExpression;
        private string _secondSelectExpression;

        public SelectBuilder(ISqlTranslator translator) : base(translator) { }

        public ISelectBuilder Select<A, B>(Expression<Func<A, T>> exp1, Expression<Func<B, U>> exp2, string tableAlias1 = null, string tableAlias2 = null)
        {
            _firstSelectExpression = _translator.GetFirstTranslation(exp1, tableAlias1);
            _secondSelectExpression = _translator.GetFirstTranslation(exp2, tableAlias2);
            return this;
        }

        protected override string CreateClause() => CreateClause(_firstSelectExpression, _secondSelectExpression);

        protected abstract string CreateClause(string selector1, string selector2);
    }
}
