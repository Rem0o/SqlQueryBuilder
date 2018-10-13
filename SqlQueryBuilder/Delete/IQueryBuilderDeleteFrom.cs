namespace SqlQueryBuilder.Delete
{
    public interface IQueryBuilderDeleteFrom
    {
        IQueryBuilderJoinOrWhere DeleteFrom<T>(string tableAlias = null);
    }
}