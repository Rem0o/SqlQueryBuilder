namespace SqlQueryBuilder
{
    public interface IBuildQuery
    {
        bool TryBuild(out string query);
    }

    public interface ISelectBuilder
    {
        bool TryBuild(ISqlTranslator translator, out string select);
    }

    public interface IWhereBuilder
    {
        bool TryBuild(ISqlTranslator translator, out string where);
    }

    public interface ICompareBuilder
    {
        bool TryBuild(ISqlTranslator translator, out string comparison);
    }
}
