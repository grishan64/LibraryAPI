using LibraryAPI.Data.Abstractions.Entities;

namespace LibraryAPI.Data.Entities;

public class Book : IHasDeleteTime
{

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string Article { get; set; } = string.Empty;

    public string PublicationYear { get; set; } = string.Empty;

    public int ExemplarCount { get; set; } = 0;

    public DateTime? DeleteTime { get; set; }

    public virtual ICollection<Reader> Readers { get; set; } = new List<Reader>();

}
