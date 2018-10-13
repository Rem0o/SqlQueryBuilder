namespace SqlQueryBuilder.Where
{
    public interface IWhereBuilder
    {
        bool TryBuild(ISqlTranslator translator, out string where);
    }
}
