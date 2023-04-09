using System.ComponentModel;

namespace LibraryAPI.Data.DTO;

public abstract record BaseBook
{
    [Description("Наименование")]
    public string Name { get; init; } = string.Empty;

    [Description("Автор")]
    public string Author { get; init; } = string.Empty;

    [Description("Артикул")]
    public string Article { get; init; } = string.Empty;

    [Description("Год издания")]
    public string PublicationYear { get; init; } = string.Empty;

    [Description("Количество экземпляров")]
    public int ExemplarCount { get; init; } = 0;

}
