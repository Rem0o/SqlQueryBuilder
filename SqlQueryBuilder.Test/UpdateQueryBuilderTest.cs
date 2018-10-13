using SqlQueryBuilder.Test.POCO;
using System;
using Xunit;

namespace SqlQueryBuilder.Test
{
    public class UpdateQueryBuilderTest
    {
        private IQueryBuilderUpdateFrom GetBuilder()
        {
            ISqlTranslator translator = new SqlTranslator();
            ICompare compareFactory() => new Comparator();
            IWhereBuilderFactory whereBuilderFactory() => new WhereBuilderFactory(compareFactory);
            return new SqlQueryBuilderFactory(translator, whereBuilderFactory, compareFactory).GetUpdate();
        }

        [Fact]
        public void Update_SetFrom_ValidQuery()
        {
            var builder = GetBuilder().From<Car>()
                .Set(car => car.Mileage, "@mileage");

            Assert.True(builder.TryBuild(out var query));
            Assert.True(CompareQueries("UPDATE [CAR] SET [CAR].[Mileage] = @mileage from [CAR] [CAR]", query));
        }

        [Fact]
        public void Update_SetFromJoin_ValidQuery()
        {
            var builder = GetBuilder().From<Car>()
                .Join<Car, CarMaker>(car => car.CarMakerId, maker => maker.Id)
                .Join<CarMaker, Country>(maker => maker.CountryOfOriginId, country => country.Id)
                .Set(car => car.Mileage, "@mileage");

            Assert.True(builder.TryBuild(out var query));

            var expectedQuery = "UPDATE [CAR] SET [CAR].[Mileage] = @mileage from [CAR] [CAR] " + 
                "JOIN [CarMaker] on [Car].[CarMakerId] = [CarMaker].[Id] " +
                "JOIN [Country] on [CarMaker].[CountryOfOriginId] = [Country].[Id]";

            Assert.True(CompareQueries(expectedQuery, query));
        }

        [Fact]
        public void Update_SetFromJoinSameAlias_InvalidQuery()
        {
            var builder = GetBuilder().From<Car>("Alias")
                .Join<Car, Car>(car => car.CarMakerId, maker => maker.Id, "Alias", "Alias")
                .Set(car => car.Mileage, "@mileage");

            Assert.False(builder.TryBuild(out var query));
        }

        [Fact]
        public void Update_SetFromWhere_ValidQuery()
        {
            var builder = GetBuilder().From<Car>()
               .Set(car => car.Mileage, "@mileage")
               .Where(c => c.Compare<Car>(car => car.Mileage).With(Operators.LT, "0"));

            Assert.True(builder.TryBuild(out var query));

            var expectedQuery = "UPDATE [CAR] SET [CAR].[Mileage] = @mileage from [CAR] [CAR] " +
                "WHERE (([CAR].[Mileage]) < (0))";

            Assert.True(CompareQueries(expectedQuery, query));
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
                .Set(car => car.Mileage, "@value");

            Assert.True(basicQuery.TryBuild(out _), "The basic query should be valid");

            var isValid = basicQuery
                .WhereFactory(CountryCondition) // Fail condition, country is not joined
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
