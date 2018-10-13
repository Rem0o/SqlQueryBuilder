namespace SqlQueryBuilder.Select
{
    public interface IQueryBuilderSelectFrom
    {
        IQueryBuilderJoinOrSelect From<T>(string tableAlias = null);
    }
}