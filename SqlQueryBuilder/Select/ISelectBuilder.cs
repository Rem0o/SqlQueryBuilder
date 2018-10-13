namespace SqlQueryBuilder.Select
{
    public interface ISelectBuilder
    {
        bool TryBuild(ISqlTranslator translator, out string select);
    }
}
