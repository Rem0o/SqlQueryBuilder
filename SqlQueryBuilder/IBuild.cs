namespace SqlQueryBuilder
{
    public interface IBuildQuery
    {
        bool TryBuild(out string query);
    }
}
