namespace SqlQueryBuilder
{
    public interface IQueryBuilderValues
    {
        IBuildQuery Values(params string[] values);
    }
}