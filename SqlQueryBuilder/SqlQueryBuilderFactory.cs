using SqlQueryBuilder.Delete;
using SqlQueryBuilder.Insert;
using SqlQueryBuilder.Select;
using SqlQueryBuilder.Update;
using SqlQueryBuilder.Where;
using System;

namespace SqlQueryBuilder
{
    public class SqlQueryBuilderFactory : IQueryBuilderFactory
    {
        private readonly Func<ISqlTranslator> _translatorFactory;
        private readonly Func<IWhereBuilderFactory> _createWhereBuilderFactory;
        private readonly Func<ICompare> _compareFactory;

        public SqlQueryBuilderFactory(Func<ISqlTranslator> translatorFactory, Func<IWhereBuilderFactory> createWhereBuilderFactory, Func<ICompare> compareFactory)
        {
            this._translatorFactory = translatorFactory;
            this._createWhereBuilderFactory = createWhereBuilderFactory;
            this._compareFactory = compareFactory;
        }

        public IQueryBuilderSelectFrom GetSelect()
        {
            return new SelectQueryBuilder(_translatorFactory(), _createWhereBuilderFactory, _compareFactory);
        }

        public IQueryBuilderUpdateFrom GetUpdate()
        {
            return new UpdateQueryBuilder(_translatorFactory(), _createWhereBuilderFactory, _compareFactory);
        }

        public IQueryBuilderInsertInto GetInsert()
        {
            return new InsertQueryBuilder(_translatorFactory());
        }

        public IQueryBuilderDeleteFrom GetDelete()
        {
            return new DeleteQueryBuilder(_translatorFactory(), _createWhereBuilderFactory, _compareFactory);
        }
    }
}
