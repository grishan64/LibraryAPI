using System.ComponentModel;

namespace LibraryAPI.Data.DTO;

public abstract record BaseReader
{
    [Description("ФИО")]
    public string FIO { get; set; } = string.Empty;

    [Description("Дата рождения")]
    public DateTime? BirthDate { get; set; }
}
