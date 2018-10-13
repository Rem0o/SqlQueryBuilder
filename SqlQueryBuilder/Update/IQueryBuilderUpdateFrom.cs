namespace SqlQueryBuilder.Update
{
    public interface IQueryBuilderUpdateFrom
    {
        IQueryBuilderJoinOrSet<T> From<T>(string tableAlias = null);
    }
}