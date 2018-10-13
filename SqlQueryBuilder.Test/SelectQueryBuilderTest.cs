using SqlQueryBuilder.Test.POCO;
using System;
using Xunit;

namespace SqlQueryBuilder.Test
{
    public class SqlQueryBuilderTest
    {
        private IQueryBuilderSelectFrom GetBuilder()
        {
            ISqlTranslator translator = new SqlTranslator();
            ICompare compareFactory() => new Comparator();
            IWhereBuilderFactory whereBuilderFactory() => new WhereBuilderFactory(compareFactory);
            return new SqlQueryBuilderFactory(translator, whereBuilderFactory, compareFactory).GetSelect();
        }

        [Fact]
        public void SelectFrom_POCO_ValidQuery()
        {
            var isValid = GetBuilder().From<Car>()
                .Join<Car, CarMaker>(car => car.CarMakerId, maker => maker.Id)
                .SelectAll<Car>()
                .Where(c => c.Compare<CarMaker>(maker => maker.Name).With(Operators.EQ, "@brand"))
                .OrderBy<CarMaker>(maker => maker.FoundationDate, desc: true)
                .TryBuild(out var query);

            Assert.True(isValid, "The query should be valid");

            var expectedQuery = $"SELECT [Car].* FROM [Car] JOIN [CarMaker] ON [Car].[CarMakerId] = [CarMaker].[Id] "
                + "WHERE (([CarMaker].[Name]) = (@brand)) ORDER BY [CarMaker].[FoundationDate] DESC";
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
                .Where(c => c.Compare<CarMaker>(maker => maker.Name, makerTableAlias).With(Operators.EQ, "@brand"))
                .Where(c => c.Compare<Car>(car => car.ModelYear, carTableAlias).With(Operators.GT, "@year"))
                .TryBuild(out var query);

            Assert.True(isValid, "The query should be valid");

            var expectedQuery = $"SELECT [{carTableAlias}].* FROM [Car] AS [{carTableAlias}] "
                + $"JOIN [CarMaker] AS [{makerTableAlias}] ON [{carTableAlias}].[CarMakerId] = [{makerTableAlias}].[Id] "
                + $"WHERE (([{makerTableAlias}].[Name]) = (@brand)) AND (([{carTableAlias}].[ModelYear]) > (@year))";
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
                    .Where(c => c.Compare<CarMaker>(maker => maker.Name).With(Operators.LIKE, "@brand"))
                    .TryBuild(out var query);

            Assert.True(isValid);

            var expectedQuery = $"SELECT [{CAR_ALIAS}].* FROM [Car] AS [{CAR_ALIAS}] "
                + $"LEFT JOIN [CarMaker] AS [{MAKER_ALIAS}] ON [{CAR_ALIAS}].[CarMakerId] = [{MAKER_ALIAS}].[Id] "
                + $"WHERE (([{MAKER_ALIAS}].[Name]) LIKE (@brand))";

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
                .SelectAs(new Aggregate(AggregateFunctions.AVG).Select<Car>(car => car.Mileage), "AverageMileage")
                .Select<Car>(car => car.ModelYear)
                .Select<CarMaker>(maker => maker.Name)
                .GroupBy<Car>(car => car.ModelYear)
                .GroupBy<CarMaker>(maker => maker.Name)
                .TryBuild(out var query);

            var expectedQuery = $"SELECT AVG([Car].[Mileage]) AS [AverageMileage], [Car].[ModelYear], [CarMaker].[Name] FROM [Car] "
                + $"JOIN [CarMaker] ON [Car].[CarMakerId] = [CarMaker].[Id] "
                + $"GROUP BY [Car].[ModelYear], [CarMaker].[Name]";

            Assert.True(isValid);
            Assert.True(CompareQueries(expectedQuery, query));
        }

        [Theory]
        [InlineData(10, true)]
        [InlineData(1, true)]
        [InlineData(0, true)]
        [InlineData(-1, false)]
        public void SelectTop_Integer_PositiveOnly(int top, bool valid)
        {
            var isValid = GetBuilder().From<CarMaker>()
                .Top(top)
                .Select<CarMaker>(maker => new { maker.FoundationDate, maker.Name })
                .TryBuild(out var query);

            var expectedQuery = $"SELECT {(top > 0 ? $"TOP {top} ": string.Empty)}[CarMaker].[FoundationDate], [CarMaker].[Name] FROM [CarMaker]";

            Assert.True(isValid == valid);
            if (isValid)
                Assert.True(expectedQuery == query);
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
                    .Where(c => c.Compare<Car>(car1 => car1.Id, CAR1).With<Car>(Operators.NEQ, car2 => car2.Id, CAR2))
                    .TryBuild(out _);

            Assert.True(isValid);
        }

        [Fact]
        public void WhereBuilderValidity_Affect_QueryValidity()
        { 
            var translator = new SqlTranslator();
            translator.AddTable(typeof(Car));
            translator.AddTable(typeof(CarMaker));

            var whereBuilderFactory = new WhereBuilderFactory(() => new Comparator());

            var whereIsValid = CountryCondition(whereBuilderFactory)
                .TryBuild(translator, out _);

            Assert.False(whereIsValid, "The where clause needs to be invalid");

            var basicQuery = GetBuilder().From<Car>()
                .SelectAll<Car>();

            Assert.True(basicQuery.TryBuild(out _), "The basic query should be valid");

            var isValid = basicQuery
                .WhereFactory(CountryCondition) // Fail condition
                .TryBuild(out var query);

            Assert.False(isValid, "An invalid where should cause an otherwise valid query to be invalid");
        }

        private IWhereBuilder CountryCondition(IWhereBuilderFactory factory)
        {
            return factory.Compare(c => c.Compare<Country>(country => country.Name).With(Operators.LIKE, "Germany"));
        }

        private bool CompareQueries(string first, string second)
        {
            string prep(string s) => s.Trim().ToUpperInvariant().Replace(" ", string.Empty).Replace(Environment.NewLine, string.Empty);
            return prep(first) == prep(second);
        }
    }
}
