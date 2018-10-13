namespace SqlQueryBuilder
{
    public interface IQueryBuilderFactory
    {
        IQueryBuilderSelectFrom GetSelect();
        IQueryBuilderUpdateFrom GetUpdate();
    }


    public interface IQueryBuilderSelectFrom
    {
        IQueryBuilderJoinOrSelect From<T>(string tableAlias = null);
    }

    public interface IQueryBuilderUpdateFrom
    {
        IQueryBuilderJoinOrSet<T> From<T>(string tableAlias = null);
    }
}