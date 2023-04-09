using LibraryAPI.Data.Abstractions.Entities;

namespace LibraryAPI.Extensions;

public static class DeleteTimeExtension
{
    public static IQueryable<TModel> NotDeleted<TModel>(
    this IQueryable<TModel> queryable)
    where TModel : IHasDeleteTime => queryable
        .Where(x => x.DeleteTime == null);
}
