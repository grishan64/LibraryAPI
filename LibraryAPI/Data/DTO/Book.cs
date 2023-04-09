using System.ComponentModel;

namespace LibraryAPI.Data.DTO;

[Description("Книга")]
public record Book : BaseBook
{
    [Description("Id книги")]
    public Guid Id { get; init; } = default;
}
