using System;

namespace SqlQueryBuilder
{
    public enum DateDiffType
    {
        DAY,
        MONTH,
        YEAR
    }

    public class DateDiff : SelectBuilder<DateTime>
    {
        private readonly DateDiffType type;
        private readonly DateTime compareTo;

        public DateDiff(DateDiffType type, DateTime compareTo, ISqlTranslator translator) : base(translator)
        {
            this.type = type;
            this.compareTo = compareTo;
        }

        protected override string CreateClause(string column)
        {
            return $"datediff({type.ToString()}, {column}, '{compareTo.ToString("yyyy-MM-dd")}')";
        }
    }

    public class DateDiff2 : SelectBuilder<DateTime, DateTime>
    {
        private readonly DateDiffType type;

        public DateDiff2(DateDiffType type, ISqlTranslator translator) : base(translator)
        {
            this.type = type;
        }

        protected override string CreateClause(string column1, string column2)
        {
            return $"datediff({type.ToString()}, '{column1}', {column2})";
        }
    }

    public class Aggregate : SelectBuilder<object>
    {
        private readonly string aggregateFunction;

        public Aggregate(string aggregateFunction, ISqlTranslator translator) : base(translator)
        {
            this.aggregateFunction = aggregateFunction;
        }

        protected override string CreateClause(string column)
        {
            return $"{aggregateFunction}({column})";
        }
    }
}
