using LibraryAPI.Data.Abstractions.Entities;

namespace LibraryAPI.Data.Entities;

public class Reader : IHasDeleteTime
{

    public Guid Id { get; set; } = Guid.NewGuid();

    public string FIO { get; set; } = string.Empty;

    public DateTime? BirthDate { get; set; }

    public DateTime? DeleteTime { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();

}
