using SqlQueryBuilder.Test.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SqlQueryBuilder.Test
{
    public class WhereFactoryTest
    {
        private static Dictionary<string, Type> GetMapper()
        {
            return new Dictionary<string, Type>()
            {
                {"Car", typeof(Car) },
                {"CarMaker", typeof(CarMaker) }
            };
        }

        [Fact]
        public void Or_WithinMapper_Valid()
        {
            Dictionary<string, Type> mapper = GetMapper();

            var builder = new WhereFactory(mapper).Or(
                CheapCarCondition,
                SweetSpotLexusCondition
            );

            Assert.True(builder.TryBuild(out var whereClause));
        }

        [Fact]
        public void Or_NotWithinMapper_Invalid()
        {
            var mapper = GetMapper();
            Assert.True(mapper.Where(x => x.Value == typeof(Country)).Count() == 0, "Country should not be in the mapper");

            IWhereBuilder CheapNonAmericanCondition(IWhereBuilderFactory factory) => factory.And(
                CheapCarCondition,
                // FAIL CONDITION: The "Country" table is not in the mapper
                f => f.Compare<Country>(country => country.Name, Compare.NEQ, "USA")
             );

            var builder = new WhereFactory(mapper).Or(
                CheapNonAmericanCondition,
                SweetSpotLexusCondition
            );

            Assert.False(builder.TryBuild(out var whereClause));
        }

        private IWhereBuilder CheapCarCondition(IWhereBuilderFactory factory)
        {
            return factory.And(
                f => f.Compare<Car>(car => car.Mileage, Compare.LT, "100000"),
                f => f.Compare<Car>(car => car.Price, Compare.LT, "5000")
            );
        }

        private IWhereBuilder SweetSpotLexusCondition(IWhereBuilderFactory factory)
        {
            return factory.And(
                f => f.Compare<Car>(car => car.ModelYear, Compare.GT, "2015"),
                f => f.Compare<Car>(car => car.Mileage, Compare.LT, "25000"),
                f => f.Compare<Car>(car => car.Price, Compare.LTE, "32000"),
                f => f.Compare<CarMaker>(maker => maker.Name, Compare.LIKE, "LEXUS")
            );
        }

    }
}
