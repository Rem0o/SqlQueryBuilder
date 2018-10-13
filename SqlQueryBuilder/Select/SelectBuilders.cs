using System;

namespace SqlQueryBuilder.Select
{
    public enum DateDiffType
    {
        SECOND,
        MINUTE,
        HOUR,
        DAY,
        MONTH,
        YEAR
    }

    public class DateDiff : SelectBuilder<DateTime>
    {
        private readonly DateDiffType type;
        private readonly string compareTo;

        public DateDiff(DateDiffType type, string compareTo)
        {
            this.type = type;
            this.compareTo = compareTo;
        }

        protected override string CreateClause(string column)
        {
            return $"datediff({type.ToString()}, {column}, '{compareTo}')";
        }
    }

    public class DateDiff2 : SelectBuilder<DateTime, DateTime>
    {
        private readonly DateDiffType type;

        public DateDiff2(DateDiffType type)
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

        public Aggregate(string aggregateFunction)
        {
            this.aggregateFunction = aggregateFunction;
        }

        protected override string CreateClause(string column)
        {
            return $"{aggregateFunction}({column})";
        }
    }
}
