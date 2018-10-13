using SqlQueryBuilder.Insert;
using SqlQueryBuilder.Test.POCO;
using SqlQueryBuilder.Where;
using System;
using System.Linq;
using Xunit;

namespace SqlQueryBuilder.Test
{
    public class InsertQueryBuilderTest
    {
        private IQueryBuilderInsertInto GetBuilder()
        {
            ISqlTranslator translator = new SqlTranslator();
            ICompare compareFactory() => new Comparator();
            IWhereBuilderFactory whereBuilderFactory() => new WhereBuilderFactory(compareFactory);
            return new SqlQueryBuilderFactory(translator, whereBuilderFactory, compareFactory).GetInsert();
        }

        [Fact]
        public void InsertInto_ValidQuery()
        {

            var builder = GetBuilder().InsertInto<Car>(car => new
            {
                car.Id,
                car.ModelYear,
                car.Mileage,
                car.Price,
                car.CarMakerId
            }).Values(typeof(Car).GetProperties().Select(p => $"@{p.Name}").ToArray());

            Assert.True(builder.TryBuild(out var query));

            var expectedQuery = "INSERT INTO [CAR] ([Car].[Id], [Car].[ModelYear], [Car].[Mileage], [Car].[Price], [Car].[CarMakerId]) "
                + "VALUES (@Id, @ModelYear, @Mileage, @Price, @CarMakerId)";
            Assert.True(CompareQueries(expectedQuery, query));
        }

        [Fact]
        public void InsertInto_MissingValues_InvalidQuery()
        {

            var builder = GetBuilder().InsertInto<Car>(car => new
            {
                car.Id,
                car.ModelYear,
                car.Mileage,
                car.Price,
                car.CarMakerId
            }).Values("@id", "@modelYear"); // missing values

            Assert.False(builder.TryBuild(out var query));
        }

        private bool CompareQueries(string first, string second)
        {
            string prep(string s) => s.Trim().ToUpperInvariant().Replace(Environment.NewLine, " ");
            return prep(first) == prep(second);
        }
    }
}
