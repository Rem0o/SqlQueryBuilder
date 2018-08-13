using SqlQueryBuilder.Test.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SqlQueryBuilder.Test
{
    public class SelectBuilderTest
    {
        [Fact]
        public void DateDiff_Year_IsValid()
        {
            DateTime compareTo = DateTime.Now;
            var translator = GetTranslator();

            var mydatediff = new DateDiff(DateDiffType.YEAR, DateTime.Now, translator);

            Assert.True(mydatediff
                .Select<CarMaker>(maker => maker.FoundationDate)
                .TryBuild(out string clause));

            Assert.True($"datediff(YEAR, [CarMaker].[FoundationDate], '{compareTo.ToString("yyyy-MM-dd")}')" == clause);
        }
     
        private static SqlTranslator GetTranslator()
        {
            var translator = new SqlTranslator();
            translator.AddTable<CarMaker>("CarMaker");
            return translator;
        }
    }
}
