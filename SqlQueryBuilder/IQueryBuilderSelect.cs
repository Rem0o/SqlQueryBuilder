namespace SqlQueryBuilder
{
    public interface IQueryBuilderSelect<T>: IQueryBuilderSelectOnly<T>, IQueryBuilderWhere<T>
    {

    }
}
