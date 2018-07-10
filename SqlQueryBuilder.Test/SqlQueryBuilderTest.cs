using SqlQueryBuilder.Test.POCO;
using System;
using System.Collections.Generic;
using Xunit;

namespace SqlQueryBuilder.Test
{
    public class SqlQueryBuilderTest
    {
        private IQueryBuilderFrom GetBuilder() => new SqlQueryBuilder();

        [Fact]
        public void SelectFrom_POCO_ValidQuery()
        {
            var isValid = GetBuilder().From<Car>()
                .Join<Car, CarMaker>(car => car.CarMakerId, maker => maker.Id)
                .SelectAll<Car>()
                .Where<CarMaker>(maker => maker.Name, Compare.EQ, "@brand")
                .OrderBy<CarMaker>(maker => maker.FoundationDate, desc: true)
                .TryBuild(out var query);

            Assert.True(isValid, "The query should be valid");

            var expectedQuery = $"SELECT [Car].* FROM [Car] JOIN [CarMaker] ON [Car].[CarMakerId] = [CarMaker].[Id] "
                + "WHERE ([CarMaker].[Name] = @brand) ORDER BY [CarMaker].[FoundationDate] DESC";
            Assert.True(CompareQueries(expectedQuery, query));
        }

        [Theory]
        [InlineData("Automobile", "CarMaker")]
        [InlineData("Voiture", "Manufacturier")]
        public void SelectFrom_POCOWithAlias_ValidQuery(string carTableAlias, string makerTableAlias)
        {
            var isValid = GetBuilder().From<Car>(carTableAlias)
                .Join<Car, CarMaker>(car => car.CarMakerId, maker => maker.Id, table1Alias: carTableAlias, table2Alias: makerTableAlias)
                .SelectAll<Car>(carTableAlias)
                .Where<CarMaker>(maker => maker.Name, Compare.EQ, "@brand", tableAlias: makerTableAlias)
                .Where<Car>(car => car.ModelYear, Compare.GT, "@year", tableAlias: carTableAlias)
                .TryBuild(out var query);

            Assert.True(isValid, "The query should be valid");

            var expectedQuery = $"SELECT [{carTableAlias}].* FROM [Car] AS [{carTableAlias}] "
                + $"JOIN [CarMaker] AS [{makerTableAlias}] ON [{carTableAlias}].[CarMakerId] = [{makerTableAlias}].[Id] "
                + $"WHERE ([{makerTableAlias}].[Name] = @brand) AND ([{carTableAlias}].[ModelYear] > @year)";
            Assert.True(CompareQueries(expectedQuery, query));
        }

        [Fact]
        public void SelectFrom_InferredAlias_ValidQuery()
        {
            const string CAR_ALIAS = "SomeCar";
            const string MAKER_ALIAS = "SomeMaker";

            var isValid = GetBuilder().From<Car>(CAR_ALIAS)
                    .LeftJoin<Car, CarMaker>(car => car.CarMakerId, maker => maker.Id, table2Alias: MAKER_ALIAS)
                    // inferred alias for "Car" because "Car" is only referred in a single FROM/JOIN
                    .SelectAll<Car>()
                    // inferred alias for "CarMaker" because "CarMaker" is only referred in a single FROM/JOIN
                    .Where<CarMaker>(maker => maker.Name, Compare.LIKE, "@brand") 
                    .TryBuild(out var query);

            Assert.True(isValid);

            var expectedQuery = $"SELECT [{CAR_ALIAS}].* FROM [Car] AS [{CAR_ALIAS}] "
                + $"LEFT JOIN [CarMaker] AS [{MAKER_ALIAS}] ON [{CAR_ALIAS}].[CarMakerId] = [{MAKER_ALIAS}].[Id] "
                + $"WHERE ([{MAKER_ALIAS}].[Name] LIKE @brand)";

            Assert.True(CompareQueries(expectedQuery, query));
        }

        [Fact]
        public void SelectFrom_AnonymousObject_ValidQuery()
        {
            var isValid = GetBuilder().From<Car>()
                .Select<Car>(car => new { car.Id, car.CarMakerId, car.Mileage, car.ModelYear })
                .TryBuild(out var query);

            Assert.True(isValid, "The query should be valid");

            var expectedQuery = $"SELECT [Car].[Id], [Car].[CarMakerId], [Car].[Mileage], [Car].[ModelYear] FROM [Car]";
            Assert.True(CompareQueries(expectedQuery, query));
        }

        [Fact]
        public void SelectAggregate_Average_IsValid()
        {
            var isValid = GetBuilder().From<Car>()
                .Join<Car, CarMaker>(car => car.CarMakerId, maker => maker.Id)
                .Select<Car>(car => car.ModelYear)
                .Select<CarMaker>(maker => maker.Name)
                .SelectAggregateAs<Car>(AggregateFunctions.AVG, car => car.Mileage, "AverageMileage")
                .GroupBy<Car>(car => car.ModelYear)
                .GroupBy<CarMaker>(maker => maker.Name)
                .TryBuild(out var query);

            var expectedQuery = $"SELECT AVG([Car].[Mileage]) AS [AverageMileage], [Car].[ModelYear], [CarMaker].[Name] FROM [Car] "
                + $"JOIN [CarMaker] ON [Car].[CarMakerId] = [CarMaker].[Id] "
                + $"GROUP BY [Car].[ModelYear], [CarMaker].[Name]";

            Assert.True(isValid);
            Assert.True(CompareQueries(expectedQuery, query));
        }

        [Fact]
        public void SelectAggregate_AverageWithFailCondition_InvalidQuery()
        {
            var isValid = GetBuilder().From<Car>()
                .Join<Car, CarMaker>(car => car.CarMakerId, maker => maker.Id)
                .SelectAggregateAs<Car>(AggregateFunctions.AVG, car => car.Mileage, "AverageMileage")
                .Select<Car>(car => car.CarMakerId) // FAIL CONDITION: not in any group by clause
                .Select<Car>(car => car.ModelYear)
                .Select<CarMaker>(maker => maker.Name)
                .GroupBy<Car>(car => car.ModelYear)
                .GroupBy<CarMaker>(maker => maker.Name)
                .TryBuild(out _);

            Assert.False(isValid, "An aggregation query with a select item not in any group by clauses should not be valid");
        }

        [Fact]
        public void JoinSamePOCO_WithoutAlias_InvalidQuery()
        {
            var isValid = GetBuilder().From<Car>()
                    .LeftJoin<Car, Car>(car1 => car1.Id, car2 => car2.Id)
                    .SelectAll<Car>() // which?
                    .TryBuild(out _);

            Assert.False(isValid, "A table join on the same table without an alias should invalidate the query");
        }

        [Fact]
        public void JoinSamePOCO_SameAlias_InvalidQuery()
        {
            const string ALIAS = "SomeAlias";
            var isValid = GetBuilder().From<Car>(ALIAS)
                    .LeftJoin<Car, Car>(car1 => car1.Id, car2 => car2.Id, ALIAS, ALIAS)
                    .SelectAll<Car>(ALIAS) // which?
                    .TryBuild(out _);

            Assert.False(isValid, "A table join with the same alias should invalidate the query");
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
                    .TryBuild(out _);

            Assert.True(isValid);
        }

        [Fact]
        public void WhereBuilderValidity_Affect_QueryValidity()
        { 
            var translator = new SqlTranslator();
            translator.AddTranslation<Car>("Car");
            translator.AddTranslation<CarMaker>("CarMaker");

            var whereIsValid = CountryCondition(new WhereBuilderFactory(translator))
                .TryBuild(out _);

            Assert.False(whereIsValid, "The where clause needs to be invalid");

            var basicQuery = GetBuilder().From<Car>()
                .SelectAll<Car>();

            Assert.True(basicQuery.TryBuild(out _), "The basic query should be valid");

            var isValid = basicQuery
                .Where(CountryCondition) // Fail condition
                .TryBuild(out var query);

            Assert.False(isValid, "An invalid where should cause an otherwise valid query to be invalid");
        }

        private IWhereBuilder CountryCondition(IWhereBuilderFactory factory)
        {
            return factory.Compare<Country>(c => c.Name, Compare.LIKE, "Germany");
        }

        private bool CompareQueries(string first, string second)
        {
            string prep(string s) => s.Trim().ToUpperInvariant().Replace(" ", string.Empty).Replace(Environment.NewLine, string.Empty);
            return prep(first) == prep(second);
        }
    }
}
