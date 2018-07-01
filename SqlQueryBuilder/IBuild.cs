namespace SqlQueryBuilder
{
    public interface IBuild
    {
        bool TryBuild(out string query);
    }
}
