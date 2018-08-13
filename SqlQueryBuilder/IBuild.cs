namespace SqlQueryBuilder
{
    public interface IBuildQuery
    {
        bool TryBuild(out string query);
    }

    public interface ISelectBuilder
    {
        bool TryBuild(out string select);
    }

    public interface IWhereBuilder
    {
        bool TryBuild(out string where);
    }
}
