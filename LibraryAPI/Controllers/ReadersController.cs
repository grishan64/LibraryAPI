using LibraryAPI.Data.Contexts;
using LibraryAPI.Data.Entities;
using LibraryAPI.Extensions;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReadersController : ControllerBase
{
    private ApplicationDbContext Db { get; init; }
    private ILogger Logger { get; init; }
    private IMapper Mapper { get; init; }

    private IQueryable<Reader> DataSet => Db
        .Readers
        .Include(x => x.Books)
        .AsSplitQuery();

    public ReadersController(
        ApplicationDbContext db,
        ILogger<ReadersController> logger,
        IMapper mapper)
    {
        Db = db;
        Logger = logger;
        Mapper = mapper;
    }


    [HttpGet("search")]
    #region Swagger description
    [SwaggerOperation(
        Summary = "Search readers by FIO",
        OperationId = "Reader.Search")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "List of readers matching the search",
        typeof(IEnumerable<DTO.Book>))]
    [SwaggerResponse(
        StatusCodes.Status204NoContent)]
    #endregion
    public async Task<IActionResult> SearchReadersByFIO([Required] string searchText)
    {
        var readers = await DataSet
            .AsNoTrackingWithIdentityResolution()
            .NotDeleted()
            .Where(b => b.FIO.ToLower().Contains(searchText.ToLower()))
            .ToListAsync();

        if (!readers.Any())
            return NoContent();

        return Ok(Mapper.Map<IEnumerable<DTO.Reader>>(readers));
    }

    [HttpGet("{id}")]
    #region Swagger description
    [SwaggerOperation(
        Summary = "Get reader by ID",
        OperationId = "Reader.Get")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Reader",
        typeof(DTO.Book))]
    [SwaggerResponse(
        StatusCodes.Status404NotFound)]
    #endregion
    public async Task<IActionResult> GetReader(Guid id)
    {
        var reader = await FindReaderOrDefaultAsync(id, true);

        if (reader is null)
            return NotFound();

        return Ok(Mapper.Map<DTO.Reader>(reader));
    }

    [HttpPut("{id}")]
    #region Swagger description
    [SwaggerOperation(
        Summary = "Update reader",
        OperationId = "Reader.Update")]
    [SwaggerResponse(
        StatusCodes.Status204NoContent)]
    [SwaggerResponse(
        StatusCodes.Status404NotFound)]
    #endregion
    public async Task<IActionResult> UpdateReader(
        [FromRoute] Guid id,
        [FromBody][Required] DTO.NewReader updatedReader)
    {

        var reader = await FindReaderOrDefaultAsync(id);

        if (reader is null)
            return NotFound();

        Mapper
            .From(updatedReader)
            .AdaptTo(reader);

        await Db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost]
    #region Swagger description
    [SwaggerOperation(
        Summary = "Create new reader",
        OperationId = "Reader.Create")]
    [SwaggerResponse(
        StatusCodes.Status201Created,
        "Created reader",
        typeof(DTO.Book))]
    #endregion
    public async Task<IActionResult> CreateReader([FromBody][Required] DTO.NewReader newReader)
    {

        var reader = Mapper.Map<Reader>(newReader);

        Db.Readers.Add(reader);
        await Db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReader), new { id = reader.Id }, Mapper.Map<DTO.Reader>(reader));
    }

    [HttpDelete("{id}")]
    #region Swagger description
    [SwaggerOperation(
        Summary = "Delete reader by ID",
        OperationId = "Reader.Delete")]
    [SwaggerResponse(
        StatusCodes.Status204NoContent)]
    [SwaggerResponse(
        StatusCodes.Status404NotFound)]
    #endregion
    public async Task<IActionResult> DeleteReader(Guid id)
    {
        var reader = await FindReaderOrDefaultAsync(id);

        if (reader is null)
            return NotFound();

        reader.Books.Clear();
        reader.DeleteTime = DateTime.UtcNow;
        await Db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/give/{bookId}")]
    #region Swagger description
    [SwaggerOperation(
        Summary = "Give the book to the reader",
        OperationId = "Reader.Give")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Reader with book",
        typeof(DTO.Reader))]
    [SwaggerResponse(
        StatusCodes.Status404NotFound)]
    [SwaggerResponse(
        StatusCodes.Status400BadRequest)]
    #endregion
    public async Task<IActionResult> GiveBookToReader(Guid id, Guid bookId)
    {

        var reader = await FindReaderOrDefaultAsync(id);
        var book = await FindBookOrDefaultAsync(bookId);

        if (book is null || reader is null)
            return NotFound();

        if (reader.Books.Any(b => b == book))
            return BadRequest("Reader already has this book");

        if (book.Readers.Count >= book.ExemplarCount)
            return BadRequest("Library dont have available exemplars of this book");

        reader.Books.Add(book);
        await Db.SaveChangesAsync();

        return Ok(Mapper.Map<DTO.Reader>(reader));
    }

    [HttpDelete("{id}/return/{bookId}")]
    #region Swagger description
    [SwaggerOperation(
        Summary = "Return the book to the library",
        OperationId = "Reader.Return")]
    [SwaggerResponse(
        StatusCodes.Status204NoContent)]
    [SwaggerResponse(
        StatusCodes.Status404NotFound)]
    [SwaggerResponse(
        StatusCodes.Status400BadRequest)]
    #endregion
    public async Task<IActionResult> ReturnBookToLibrary(Guid id, Guid bookId)
    {

        var reader = await FindReaderOrDefaultAsync(id);
        var book = await FindBookOrDefaultAsync(bookId);

        if (book is null || reader is null)
            return NotFound();

        if (!reader.Books.Any(b => b == book))
            return BadRequest("Book is not attached to the reader");

        reader.Books.Remove(book);
        await Db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<Reader?> FindReaderOrDefaultAsync(Guid id, bool noTracking = false)
    {
        var q = DataSet;

        if (noTracking)
            q = q.AsNoTrackingWithIdentityResolution();

        return await q.NotDeleted().FirstOrDefaultAsync(x => x.Id.Equals(id));
    }

    private async Task<Book?> FindBookOrDefaultAsync(Guid id, bool noTracking = false)
    {
        var q = Db.Books
            .Include(b => b.Readers)
            .AsSplitQuery();

        if (noTracking)
            q = q.AsNoTrackingWithIdentityResolution();

        return await q.NotDeleted().FirstOrDefaultAsync(x => x.Id.Equals(id));
    }
}
