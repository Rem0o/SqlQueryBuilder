using SqlQueryBuilder.Delete;
using SqlQueryBuilder.Insert;
using SqlQueryBuilder.Select;
using SqlQueryBuilder.Update;

namespace SqlQueryBuilder
{
    public interface IQueryBuilderFactory
    {
        IQueryBuilderSelectFrom GetSelect();
        IQueryBuilderUpdateFrom GetUpdate();
        IQueryBuilderInsertInto GetInsert();
        IQueryBuilderDeleteFrom GetDelete();
    }
}