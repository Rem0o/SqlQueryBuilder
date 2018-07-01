using SqlQueryBuilder.Test.POCO;
using System;
using Xunit;

namespace SqlQueryBuilder.Test
{
    public class BuildingQueries
    {
        private IQueryBuilderFrom GetBuilder() => new SqlQueryBuilder();

        [Fact]
        public void SelectFrom_POCO_ValidQuery()
        {
            var isValid = GetBuilder().From<Car>()
                .Join<Car, Maker>(car => car.MakerId, maker => maker.Id)
                .SelectAll<Car>()
                .Where<Maker>(maker => maker.Name, Compare.EQ, "@brand")
                .TryBuild(out var query);

            Assert.True(isValid, "The query should be valid");

            // what a Micro-ORM/ORM would do with parameters
            var parsedQuery = query.Replace("@brand", "'TOYOTA'");

            var expectedQuery = $"SELECT [Car].* FROM [Car] JOIN [Maker] ON [Car].[MakerId] = [Maker].[Id] WHERE [Maker].[Name] = 'TOYOTA'";
            Assert.True(CompareQueries(expectedQuery, parsedQuery));
        }

        [Theory]
        [InlineData("Automobile", "CarMaker")]
        [InlineData("Voiture", "Manufacturier")]
        public void SelectFrom_POCOWithAlias_ValidQuery(string carTableAlias, string makerTableAlias)
        {
            var isValid = GetBuilder().From<Car>(carTableAlias)
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
            Assert.True(CompareQueries(expectedQuery, parsedQuery));
        }

        [Fact]
        public void SelectFrom_POCOWithAlias_InferredAliasValidQuery()
        {
            const string CAR_ALIAS = "SomeCar";
            const string MAKER_ALIAS = "SomeMaker";

            var isValid = GetBuilder().From<Car>(CAR_ALIAS)
                    .LeftJoin<Car, Maker>(car => car.MakerId, maker => maker.Id, table2Alias: MAKER_ALIAS)
                    // inferred alias for "Car" because "Car" is only referred once
                    .SelectAll<Car>()
                    // inferred alias for "Maker" because "Maker" is only referred once
                    .Where<Maker>(maker => maker.Name, Compare.LIKE, "@brand") 
                    .TryBuild(out var query);

            var parsedQuery = query.Replace("@brand", "'Nissan'");

            Assert.True(isValid);

            var expectedQuery = $"SELECT [{CAR_ALIAS}].* FROM [Car] AS [{CAR_ALIAS}] "
                + $"LEFT JOIN [Maker] AS [{MAKER_ALIAS}] ON [{CAR_ALIAS}].[MakerId] = [{MAKER_ALIAS}].[Id] "
                + $"WHERE [{MAKER_ALIAS}].[Name] LIKE 'Nissan'";

            Assert.True(CompareQueries(expectedQuery, parsedQuery));
        }

        [Fact]
        public void SelectAggregate_Average_IsValid()
        {
            var isValid = GetBuilder().From<Car>()
                .Join<Car, Maker>(car => car.MakerId, maker => maker.Id)
                .Select<Car>(car => car.ModelYear)
                .Select<Maker>(maker => maker.Name)
                .SelectAggregateAs<Car>(AggregateFunctions.AVG, car => car.Mileage, "AverageMileage")
                .GroupBy<Car>(car => car.ModelYear)
                .GroupBy<Maker>(maker => maker.Name)
                .TryBuild(out var query);

            var expectedQuery = $"SELECT AVG([Car].[Mileage]) AS [AverageMileage], [Car].[ModelYear], [Maker].[Name] FROM [Car] "
                + $"JOIN [Maker] ON [Car].[MakerId] = [Maker].[Id] "
                + $"GROUP BY [Car].[ModelYear], [Maker].[Name]";

            Assert.True(isValid, "An aggregation query where all select items are in the group by clause should be valid");
            Assert.True(CompareQueries(expectedQuery, query));
        }

        [Fact]
        public void SelectAggregate_AverageWithFailCondition_InvalidQuery()
        {
            var isValid = GetBuilder().From<Car>()
                .Join<Car, Maker>(car => car.MakerId, maker => maker.Id)
                .SelectAggregateAs<Car>(AggregateFunctions.AVG, car => car.Mileage, "AverageMileage")
                .Select<Car>(car => car.ModelYear)
                .Select<Car>(car => car.MakerId) // FAIL CONDITION: not in any group by statement
                .Select<Maker>(maker => maker.Name)
                .GroupBy<Car>(car => car.ModelYear)
                .GroupBy<Maker>(maker => maker.Name)
                .TryBuild(out var query);

            Assert.False(isValid, "An aggregation query with a select item not in the group by clause should not be valid");
        }

        [Fact]
        public void JoinSamePOCO_WithoutAlias_InvalidQuery()
        {
            var isValid = GetBuilder().From<Car>()
                    .LeftJoin<Car, Car>(car1 => car1.Id, car2 => car2.Id)
                    .SelectAll<Car>()
                    .TryBuild(out var query);

            Assert.False(isValid);
        }

        [Fact]
        public void JoinSamePOCO_WithAlias_ValidQuery()
        {
            const string CAR1 = "CAR1";
            const string CAR2 = "CAR2";

            var isValid = GetBuilder().From<Car>(CAR1)
                    .LeftJoin<Car, Car>(car1 => car1.ModelYear, car2 => car2.ModelYear, CAR1, CAR2)
                    .SelectAll<Car>(CAR1)
                    .Where<Car, Car>(car1 => car1.Id, Compare.NEQ, car2 => car2.Id, CAR1, CAR2)
                    .TryBuild(out var query);

            Assert.True(isValid);
        }

        private bool CompareQueries(string first, string second)
        {
            string prep(string s) => s.Trim().ToUpperInvariant().Replace(" ", string.Empty).Replace(Environment.NewLine, string.Empty);
            return prep(first) == prep(second);
        }
    }
}
