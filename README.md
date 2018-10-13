# SqlQueryBuilder [![Build Status](https://travis-ci.com/Rem0o/SqlQueryBuilder.svg?branch=master)](https://travis-ci.com/Rem0o/SqlQueryBuilder)

### Main features:
  - Build all your Select or Update T-SQL queries arround your POCOs!
  - Fluent interface
  - Try-Build pattern (basic validation)
  - Evaluate once: use parameters ("@param") within the builder to parametrize your queries!
  
### Other features
  - Build your complex "WHERE" clauses as sub-blocks you can assemble
  - Use complex selectors like aggregates and SQL functions by inheriting the SelectBuilder class.
  - Supports table name alias (when you join the same table multiple times)
  - All depedencies are abstracted and injected, can easily be integrated with any IoC library.

## Code exemples

Here is a basic factory method to get a builder. This factory method can be registered into any container library. 
```c#
private IQueryBuilderFactory GetSelectBuilder()
{
	ISqlTranslator translator = new SqlTranslator();
	ICompare compareFactory() => new Comparator();
	IWhereBuilderFactory whereBuilderFactory() => new WhereBuilderFactory(compareFactory);
	return new SqlQueryBuilderFactory(translator, whereBuilderFactory, compareFactory);
}
```

### A basic SELECT query

Simply write your query with terms you are familiar with.
```c#
bool isValid = GetBuilder().GetSelect().From<Car>()
    .SelectAll<Car>()
    .Where(comparator => comparator.Compare<Car>(car => car.ModelYear).With(Operators.GT, "@year"))
    .TryBuild(out string query);
    
//@year can later be replaced with your favorite library
```
Resulting SQL:
```sql
SELECT [Car].* 
FROM [Car]
WHERE [CarMaker].[ModelYear] > @year
```

### A basic UPDATE query

An update query is just as simple.
```c#
bool isValid = GetBuilder().GetUpdate().From<Car>("CarTableAlias")
   .Set(car => car.Mileage, "@mileage")
   .Where(c => c.Compare<Car>(car => car.Mileage).With(Operators.LT, "0"))
   .TryBuild(out string query);
    
```
Resulting SQL:
```sql
UPDATE [CarTableAlias]
SET [CarTableAlias].[Mileage] = @mileage
FROM [Car] [CarTableAlias]
WHERE [CarTableAlias].[Mileage] < 0
```
Note the use of an alias for the "Car" table.

### A basic INSERT INTO query

An insert query is also straightforward.
```c#
bool isValid = GetBuilder().GetInsert()
   .InsertInto<Car>(car => new {
	car.Id,
	car.ModelYear,
	car.Mileage,
	car.Price,
	car.CarMakerId
   }).Values("@id", "@modelYear", "@mileage", "@price", "@carMakerId")
   .TryBuild(out string query);
    
```
Resulting SQL:
```sql
INSERT INTO [CAR] 
([Car].[Id], [Car].[ModelYear], [Car].[Mileage], [Car].[Price], [Car].[CarMakerId])
VALUES (@id, @modelYear, @mileage, @price, @carMakerId)
```

### Table alias

You can use table aliases if you want to join the same table multiple times.

```c#
const string TABLE1 = "MAKER1";
const string TABLE2 = "MAKER2";

var isValid = GetBuilder().GetSelect().From<CarMaker>(TABLE1)
    .Join<CarMaker, CarMaker>(maker1 => maker1.CountryOfOriginId, maker2 => maker2.CountryOfOriginId, TABLE1, TABLE2)
    .SelectAll<CarMaker>(TABLE1)
    .Where(comparator => comparator.Compare<CarMaker>(maker1 => maker1.Id, TABLE1).With<CarMaker>(Operators.NEQ, maker2 => maker2.Id, TABLE2))
    .TryBuild(out string query);
    
```
Resulting SQL:
```sql
SELECT [MAKER1].* 
FROM [CarMaker] AS [MAKER1]
JOIN [CarMaker] AS [MAKER2] ON [Maker1].[CountryOfOriginId] = [Maker2].[CountryOfOriginId]
WHERE [Maker1].[Id] <> [Maker2].Id
```

### A more complex SELECT query

Here is a more complex query. Note the use of a complex selector (average aggregate).
```c#
var isValid = GetBuilder().GetSelect().From<Car>()
    .Join<Car, CarMaker>(car => car.CarMakerId, maker => maker.Id)
    .Select<CarMaker>(maker => maker.Name)
    .Select<Car>(car => car.ModelYear)
    .SelectAs(new Aggregate(AggregateFunctions.AVG).Select<Car>(car => car.Price), "AveragePrice")
    .GroupBy<Car>(car => car.ModelYear)
    .GroupBy<CarMaker>(maker => maker.Name)
    .TryBuild(out string query);
```
Resulting SQL:
```sql
SELECT AVG([Car].[Price]) AS [AveragePrice], [CarMaker].[Name], [Car].[ModelYear] 
FROM [Car]
JOIN [CarMaker] ON [Car].[CarMakerId] = [CarMaker].[Id]
GROUP BY [Car].[ModelYear], [CarMaker].[Name]
```

A complex selector is described as a class. Simply inherit SelectBuilder<T> to create your own easily. The generic type \<T\> will enforce (or not using object) a specific type on your special selector. For example, here is a DATEDIFF implementation that requires the selector to be of a DateTime type.
```c#
// definition
public class DateDiff : SelectBuilder<DateTime>
{
    private readonly DateDiffType type;
    private readonly DateTime compareTo;

    public DateDiff(DateDiffType type, DateTime compareTo)
    {
        this.type = type;
        this.compareTo = compareTo;
    }

    protected override string BuildSelectClause(string column)
    {
        return $"DATEDIFF({type.ToString()}, '{compareTo.ToString("yyyy-MM-dd")}', {column})";
    }
}

// (...)

// usage
new DateDiff(DateDiffType.YEAR, new DateTime(2018,1,1))
    .Select<CarMaker>(maker => maker.FoundationDate);
```

Resulting SQL:
```sql
DATEDIFF(YEAR, '2018-01-01', [CarMaker].[FoundationDate])
```

### Where is the fun?

People's car tastes can be all over the place, and so can be your "WHERE" clauses! Here are some "WHERE" conditions extracted as functions so we can use them later.
```c#
private IWhereBuilder CheapCarCondition(IWhereBuilderFactory factory)
{
    return factory.And(
        f => f.Compare(c => c.Compare<Car>(car => car.Mileage, Compare.LT, "@cheap_mileage")),
        f => f.Compare(c => c.Compare<Car>(car => car.Price, Compare.LT, "@cheap_price")),
        f => f.Compare(c => c.Compare<CarMaker>(maker => => maker.Name, Compare.NEQ, "@cheap_name")),
        f => f.Compare(c => c.Compare<Country>(country => country.Name, Compare.NEQ, "@cheap_country"))
    );
}

private IWhereBuilder DreamCarExceptionCondition(IWhereBuilderFactory factory)
{
    return factory.And(
        f => f.Compare(c => c.Compare<Car>(car => car.Mileage, Compare.LT, "@dream_mileage")),
        f => f.Compare(c => c.Compare<Car>(car => car.Price, Compare.LT, "@dream_price")),
        f => f.Compare(c => c.Compare<CarMaker>(maker => maker.Name, Compare.EQ, "@dream_maker")),
    );
}
```
The conditions above are assembled with a "OR" to create our very specific query! Also, notice the anonymous object used inside the select function.
```c#

var isValid = GetBuilder().GetSelect().From<Car>()
    .Join<Car, CarMaker>(car => car.CarMakerId, maker => maker.Id)
    .Join<CarMaker, Country>(maker => maker.CountryOfOriginId, country => country.Id)
    .Select<Car>(car => new { car.Id, car.Price })
    .WhereFactory(factory => factory.Or(
        CheapCarCondition,
        DreamCarExceptionCondition
    ))
    .OrderBy<Car>(car => car.Price, desc: true)
    .TryBuild(out string query);
```

Resulting SQL:
```sql
SELECT [Car].[Id], [Car].[Price] 
FROM [Car] 
JOIN [CarMaker] ON [Car].[CarMakerId] = [CarMaker].[Id] 
JOIN [Country] ON [CarMaker].[CountryOfOriginId] = [Country].[Id] 
WHERE (
  ((([Car].[Mileage]) < (@cheap_mileage)) AND 
  (([Car].[Price]) < (@cheap_price)) AND
  (([CarMaker].[Name]) <> (@cheap_maker)) AND
  (([Country].[Name]) <> (@cheap_country))
) OR (
  (([Car].[Mileage]) < (@dream_mileage)) AND
  (([Car].[Price]) < (@dream_price)) AND
  (([CarMaker].[Name]) = (@dream_maker)))
)
ORDER BY [Car].[Price] DESC
```
