namespace SqlQueryBuilder.Insert
{
    public interface IQueryBuilderValues
    {
        IBuildQuery Values(params string[] values);
    }
}