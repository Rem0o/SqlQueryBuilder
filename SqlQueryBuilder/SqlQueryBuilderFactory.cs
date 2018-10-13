using System;

namespace SqlQueryBuilder
{
    public class SqlQueryBuilderFactory : IQueryBuilderFactory
    {
        private readonly ISqlTranslator _translator;
        private readonly Func<IWhereBuilderFactory> _createWhereBuilderFactory;
        private readonly Func<ICompare> _compareFactory;

        public SqlQueryBuilderFactory(ISqlTranslator translator, Func<IWhereBuilderFactory> createWhereBuilderFactory, Func<ICompare> compareFactory)
        {
            this._translator = translator;
            this._createWhereBuilderFactory = createWhereBuilderFactory;
            this._compareFactory = compareFactory;
        }

        public IQueryBuilderSelectFrom GetSelect()
        {
            return new SelectQueryBuilder(_translator, _createWhereBuilderFactory, _compareFactory);
        }

        public IQueryBuilderUpdateFrom GetUpdate()
        {
            return new UpdateQueryBuilder(_translator, _createWhereBuilderFactory, _compareFactory);
        }
    }
}
