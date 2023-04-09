using System.ComponentModel;

namespace LibraryAPI.Data.DTO;

[Description("Читатель")]
public record Reader : BaseReader
{
    [Description("Id читателя")]
    public Guid Id { get; init; } = default;

    [Description("Список выданных книг")]
    public IEnumerable<DTO.Book> Books { get; init; } = new List<DTO.Book>();
}
