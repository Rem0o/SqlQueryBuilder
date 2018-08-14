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

            var mydatediff = new DateDiff(DateDiffType.YEAR, DateTime.Now.ToString("yyyy-MM-dd"));

            Assert.True(mydatediff
                .Select<CarMaker>(maker => maker.FoundationDate)
                .TryBuild(translator, out string clause));

            Assert.True($"datediff(YEAR, [CarMaker].[FoundationDate], '{compareTo.ToString("yyyy-MM-dd")}')" == clause);
        }
     
        private static SqlTranslator GetTranslator()
        {
            var translator = new SqlTranslator();
            translator.AddTable(typeof(CarMaker));
            return translator;
        }
    }
}
