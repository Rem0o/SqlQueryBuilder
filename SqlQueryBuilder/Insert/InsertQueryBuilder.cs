using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SqlQueryBuilder.Insert
{
    internal class InsertQueryBuilder : IQueryBuilderInsertInto, IQueryBuilderValues, IBuildQuery
    {
        private readonly ISqlTranslator _translator;
        private KeyValuePair<string, Type> TableInto;
        private readonly List<string> Columns = new List<string>();
        private readonly List<string> ValueList = new List<string>();

        private InsertQueryBuilder SkipIfError(Action action)
        {
            if (!_translator.HasError)
                action();

            return this;
        }

        public InsertQueryBuilder(ISqlTranslator translator)
        {
            _translator = translator;
        }

        public IQueryBuilderValues InsertInto<T>(Expression<Func<T, object>> lambda)
        {
            var type = typeof(T);
            var tableAliasKey = type.Name;
            TableInto = new KeyValuePair<string, Type>(tableAliasKey, type);
            _translator.AddTable(type, TableInto.Key);

            Columns.AddRange(_translator.Translate(typeof(T), lambda, null));
            return this;
        }

        public IBuildQuery Values(params string[] values)
        {
            ValueList.AddRange(values);
            return this;
        }

        public bool TryBuild(out string query)
        {
            query = string.Empty;

            if (ValueList.Count != Columns.Count)
                return false;

            query = $"INSERT INTO [{TableInto.Key}] ({string.Join(", ", Columns.Select(c => $"{c}"))}) "
                + $"VALUES ({string.Join(", ", ValueList)})";

            return true;
        }
    }
}