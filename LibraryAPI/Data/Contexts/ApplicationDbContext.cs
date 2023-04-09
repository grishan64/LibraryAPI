using LibraryAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Data.Contexts;

public class ApplicationDbContext : DbContext
{

    #region Sets

    public DbSet<Book> Books { get; init; } = default!;

    public DbSet<Reader> Readers { get; init; } = default!;

    #endregion

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
    { }

}
