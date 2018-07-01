using SqlQueryBuilder.Test.POCO;
using System;
using Xunit;

namespace SqlQueryBuilder.Test
{
    public class BuildingQueries
    {
        [Fact]
        public void SelectFrom_POCO_ValidQuery()
        {
            var isValid = SqlQueryBuilder.StartFrom<Car>()
                .Join<Car, Maker>(car => car.MakerId, maker => maker.Id)
                .SelectAll<Car>()
                .Where<Maker>(maker => maker.Name, Compare.EQ, "@brand")
                .TryBuild(out var query);

            Assert.True(isValid, "The query should be valid");

            // what a Micro-ORM/ORM would do with parameters
            var parsedQuery = query.Replace("@brand", "'TOYOTA'");

            var expectedQuery = $"SELECT [Car].* FROM [Car] JOIN [Maker] ON [Car].[MakerId] = [Maker].[Id] WHERE [Maker].[Name] = 'TOYOTA'";
            Assert.True(CompareQueries(expectedQuery, parsedQuery), "Both queries should be identical");
        }

        [Theory]
        [InlineData("Automobile", "CarMaker")]
        [InlineData("Voiture", "Manufacturier")]
        public void SelectFrom_POCOWithAlias_ValidQuery(string carTableAlias, string makerTableAlias)
        {
            var isValid = SqlQueryBuilder.StartFrom<Car>(carTableAlias)
                .Join<Car, Maker>(car => car.MakerId, maker => maker.Id, table1Alias: carTableAlias, table2Alias: makerTableAlias)
                .SelectAll<Car>(carTableAlias)
                .Where<Maker>(maker => maker.Name, Compare.EQ, "@brand", tableAlias: makerTableAlias)
                .Where<Car>(car => car.ModelYear, Compare.GT, "@year", tableAlias: carTableAlias)
                .TryBuild(out var query);

            Assert.True(isValid, "The query should be valid");

            // what a Micro-ORM/ORM would do with parameters
            var parsedQuery = query.Replace("@brand", "'TOYOTA'").Replace("@year", "2005");

            var expectedQuery = $"SELECT [{carTableAlias}].* FROM [Car] AS [{carTableAlias}] "
                + $"JOIN [Maker] AS [{makerTableAlias}] ON [{carTableAlias}].[MakerId] = [{makerTableAlias}].[Id] "
                + $"WHERE [{makerTableAlias}].[Name] = 'TOYOTA' AND [{carTableAlias}].[ModelYear] > 2005";
            Assert.True(CompareQueries(expectedQuery, parsedQuery), "Both queries should be identical");
        }

        [Fact]
        public void JoinSamePOCO_WithoutAlias_Exception()
        {
            Assert.Throws<ArgumentException>(() => 
                SqlQueryBuilder.StartFrom<Car>()
                    .LeftJoin<Car, Car>(car1 => car1.Id, car2 => car2.Id)
                    .SelectAll<Car>()
                    .TryBuild(out var query));
        }

        private bool CompareQueries(string first, string second)
        {
            string prep(string s) => s.Trim().ToUpperInvariant();
            return prep(first) == prep(second);
        }
    }
}
