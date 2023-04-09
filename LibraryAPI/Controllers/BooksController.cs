using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data.Contexts;
using LibraryAPI.Data.Entities;
using MapsterMapper;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using LibraryAPI.Extensions;

namespace LibraryAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private ApplicationDbContext Db { get; init; }
    private ILogger Logger { get; init; }
    private IMapper Mapper { get; init; }

    private IQueryable<Book> DataSet => Db
        .Books
        .Include(x => x.Readers)
        .AsSplitQuery();

    public BooksController(
        ApplicationDbContext db,
        ILogger<BooksController> logger,
        IMapper mapper)
    {
        Db = db;
        Logger = logger;
        Mapper = mapper;
    }

    [HttpGet("givenOutBooks")]
    #region Swagger description
    [SwaggerOperation(
        Summary = "Get list of given out books",
        OperationId = "Book.GivenOutBooks")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "List of given out books",
        typeof(IEnumerable<DTO.Book>))]
    [SwaggerResponse(
        StatusCodes.Status204NoContent)]
    #endregion
    public async Task<IActionResult> GetGivenOutBooks()
    {
        var books = await DataSet
            .AsNoTrackingWithIdentityResolution()
            .NotDeleted()
            .Where(b => b.Readers.Any())
            .ToListAsync();

        if (!books.Any())
            return NoContent();

        return Ok(Mapper.Map<IEnumerable<DTO.Book>>(books));
    }

    [HttpGet("availableBooks")]
    #region Swagger description
    [SwaggerOperation(
        Summary = "Get list of books available for given out",
        OperationId = "Book.AvailableBooks")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "List of books available for given out",
        typeof(IEnumerable<DTO.Book>))]
    [SwaggerResponse(
        StatusCodes.Status204NoContent)]
    #endregion
    public async Task<IActionResult> GetAvailableBooks()
    {
        var books = await DataSet
            .AsNoTrackingWithIdentityResolution()
            .NotDeleted()
            .Where(b => b.Readers.Count < b.ExemplarCount)
            .ToListAsync();

        if (!books.Any())
            return NoContent();

        return Ok(Mapper.Map<IEnumerable<DTO.Book>>(books));
    }

    [HttpGet("search")]
    #region Swagger description
    [SwaggerOperation(
        Summary = "Search books by name",
        OperationId = "Book.Search")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "List of books matching the search",
        typeof(IEnumerable<DTO.Book>))]
    [SwaggerResponse(
        StatusCodes.Status204NoContent)]
    #endregion
    public async Task<IActionResult> SearchBooksByName([Required] string searchText)
    {
        var books = await Db.Books
            .AsNoTrackingWithIdentityResolution()
            .NotDeleted()
            .Where(b => b.Name.ToLower().Contains(searchText.ToLower()))
            .ToListAsync();

        if (!books.Any())
            return NoContent();

        return Ok(Mapper.Map<IEnumerable<DTO.Book>>(books));
    }

    [HttpGet("{id}")]
    #region Swagger description
    [SwaggerOperation(
        Summary = "Get book by ID",
        OperationId = "Book.Get")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Book",
        typeof(DTO.Book))]
    [SwaggerResponse(
        StatusCodes.Status404NotFound)]
    #endregion
    public async Task<IActionResult> GetBook(Guid id)
    {
        var book = await FindBookOrDefaultAsync(id, true);

        if (book is null)
            return NotFound();

        return Ok(Mapper.Map<DTO.Book>(book));
    }

    [HttpPut("{id}")]
    #region Swagger description
    [SwaggerOperation(
        Summary = "Update book",
        OperationId = "Book.Update")]
    [SwaggerResponse(
        StatusCodes.Status204NoContent)]
    [SwaggerResponse(
        StatusCodes.Status404NotFound)]
    #endregion
    public async Task<IActionResult> UpdateBook(
        [FromRoute] Guid id,
        [FromBody][Required] DTO.NewBook updatedBook)
    {

        var book = await FindBookOrDefaultAsync(id);

        if (book is null)
            return NotFound();

        Mapper
            .From(updatedBook)
            .AdaptTo(book);

        await Db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost]
    #region Swagger description
    [SwaggerOperation(
        Summary = "Create new book",
        OperationId = "Book.Create")]
    [SwaggerResponse(
        StatusCodes.Status201Created,
        "Created book",
        typeof(DTO.Book))]
    #endregion
    public async Task<IActionResult> CreateBook([FromBody][Required] DTO.NewBook newBook)
    {

        var book = Mapper.Map<Book>(newBook);

        Db.Books.Add(book);
        await Db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, Mapper.Map<DTO.Book>(book));
    }

    [HttpDelete("{id}")]
    #region Swagger description
    [SwaggerOperation(
        Summary = "Delete book by ID",
        OperationId = "Book.Delete")]
    [SwaggerResponse(
        StatusCodes.Status204NoContent)]
    [SwaggerResponse(
        StatusCodes.Status404NotFound)]
    #endregion
    public async Task<IActionResult> DeleteBook(Guid id)
    {
        var book = await FindBookOrDefaultAsync(id);

        if (book is null)
            return NotFound();

        book.Readers.Clear();
        book.DeleteTime = DateTime.UtcNow;
        await Db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<Book?> FindBookOrDefaultAsync(Guid id, bool noTracking = false)
    {
        var q = DataSet;

        if (noTracking)
            q = q.AsNoTrackingWithIdentityResolution();

        return await q.NotDeleted().FirstOrDefaultAsync(x => x.Id.Equals(id));
    }
}
