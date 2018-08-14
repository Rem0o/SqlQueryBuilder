using SqlQueryBuilder.Test.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SqlQueryBuilder.Test
{
    public class WhereFactoryTest
    {
        private IWhereBuilderFactory GetFactory() => new WhereBuilderFactory(() => new Comparator());

        [Fact]
        public void Or_WithinMapper_Valid()
        {
            var translator = GetTranslator();

            var builder = GetFactory().Or(
                CheapCarCondition,
                SweetSpotLexusCondition
            );

            var expectedWhereClause = $"(((([Car].[Mileage]) < ({CHEAPCAR_MILEAGE})) AND (([Car].[Price]) < ({CHEAPCAR_PRICE})))"
                + $" OR ((([Car].[ModelYear]) > ({LEXUS_YEAR})) AND (([Car].[Mileage]) < ({LEXUS_MILEAGE})) AND"
                + $" (([Car].[Price]) <= ({LEXUS_PRICE})) AND (([CarMaker].[Name]) LIKE ({LEXUS_BRAND}))))";

            Assert.True(builder.TryBuild(translator, out var whereClause));
            Assert.True(whereClause == expectedWhereClause);
        }

        [Fact]
        public void Or_NotWithinMapper_Invalid()
        {
            var translator = GetTranslator();

            IWhereBuilder CheapNonAmericanCondition(IWhereBuilderFactory factory) => factory.And(
                CheapCarCondition,
                // FAIL CONDITION: The "Country" table is not in the mapper
                f => f.Compare(c => c.Compare<Country>(country => country.Name).With(Operators.NEQ, "USA"))
             );

            var builder = GetFactory().Or(
                CheapNonAmericanCondition,
                SweetSpotLexusCondition
            );

            Assert.False(builder.TryBuild(translator, out _));
        }

        private const string CHEAPCAR_MILEAGE = "100000";
        private const string CHEAPCAR_PRICE = "5000";

        private IWhereBuilder CheapCarCondition(IWhereBuilderFactory factory)
        {
            return factory.And(
                f => f.Compare(c => c.Compare<Car>(car => car.Mileage).With(Operators.LT, CHEAPCAR_MILEAGE)),
                f => f.Compare(c => c.Compare<Car>(car => car.Price).With(Operators.LT, CHEAPCAR_PRICE))
            );
        }

        private const string LEXUS_YEAR = "2015";
        private const string LEXUS_MILEAGE = "25000";
        private const string LEXUS_PRICE = "32000";
        private const string LEXUS_BRAND = "LEXUS";

        private IWhereBuilder SweetSpotLexusCondition(IWhereBuilderFactory factory)
        {
            return factory.And(
                f => f.Compare(c => c.Compare<Car>(car => car.ModelYear).With(Operators.GT, LEXUS_YEAR)),
                f => f.Compare(c => c.Compare<Car>(car => car.Mileage).With(Operators.LT, LEXUS_MILEAGE)),
                f => f.Compare(c => c.Compare<Car>(car => car.Price).With(Operators.LTE, LEXUS_PRICE)),
                f => f.Compare(c => c.Compare<CarMaker>(maker => maker.Name).With(Operators.LIKE, LEXUS_BRAND))
            );
        }

        private static SqlTranslator GetTranslator()
        {
            var translator = new SqlTranslator();
            translator.AddTable(typeof(Car));
            translator.AddTable(typeof(CarMaker));
            return translator;
        }
    }
}
