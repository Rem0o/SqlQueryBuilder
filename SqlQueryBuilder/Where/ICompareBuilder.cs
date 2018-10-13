namespace SqlQueryBuilder.Where
{
    public interface ICompareBuilder
    {
        bool TryBuild(ISqlTranslator translator, out string comparison);
    }
}
