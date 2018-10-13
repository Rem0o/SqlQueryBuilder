using SqlQueryBuilder.Test.POCO;
using System;
using System.Diagnostics;
using Xunit;

namespace SqlQueryBuilder.Test
{
    public class DeleteQueryBuilderTest
    {
        private IQueryBuilderDeleteFrom GetBuilder()
        {
            ISqlTranslator translator = new SqlTranslator();
            ICompare compareFactory() => new Comparator();
            IWhereBuilderFactory whereBuilderFactory() => new WhereBuilderFactory(compareFactory);
            return new SqlQueryBuilderFactory(translator, whereBuilderFactory, compareFactory).GetDelete();
        }

        [Fact]
        public void DeleteFrom_ValidQuery()
        {
            var builder = GetBuilder().DeleteFrom<Car>();

            Assert.True(builder.TryBuild(out var query));

            var expectedQuery = "DELETE FROM [CAR]";
            AssertCompareQueries(expectedQuery, query);
        }

        [Fact]
        public void DeleteFrom_Join_ValidQuery()
        {
            var builder = GetBuilder().DeleteFrom<Car>()
                .Join<Car, CarMaker>(car => car.CarMakerId, carMaker => carMaker.Id);

            Assert.True(builder.TryBuild(out var query));

            var expectedQuery = "DELETE FROM [CAR] JOIN [CARMAKER] ON [CAR].[CarMakerId] = [CarMaker].[Id]";
            AssertCompareQueries(expectedQuery, query);
        }

        [Fact]
        public void DeleteFrom_JoinWhere_ValidQuery()
        {
            var builder = GetBuilder().DeleteFrom<Car>()
                .Join<Car, CarMaker>(car => car.CarMakerId, carMaker => carMaker.Id)
                .WhereFactory(f => f.Or(
                    f1 => f1.Compare(c => c.Compare<CarMaker>(m => m.FoundationDate).With(Operators.LT, "1950-01-01")),
                    f2 => f2.Compare(c => c.Compare<Car>(car => car.Mileage).With(Operators.LTE, 50_000.ToString()))
                ));

            Assert.True(builder.TryBuild(out var query));

            var expectedQuery = "DELETE FROM [Car] "
                + "JOIN [CarMaker] ON [Car].[CarMakerId] = [CarMaker].[Id] "
                + "WHERE ((([CarMaker].[FoundationDate]) < (1950-01-01)) OR (([Car].[Mileage]) <= (50000)))";

            AssertCompareQueries(expectedQuery, query);
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

            var basicQuery = GetBuilder().DeleteFrom<Car>();

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

        private void AssertCompareQueries(string first, string second)
        {
            string prep(string s) => s.Trim().ToUpperInvariant().Replace(Environment.NewLine, " ");
            string prepFirst = prep(first),
                prepSecond = prep(second);

            Assert.Equal(prepFirst, prepSecond);
        }
    }
}
