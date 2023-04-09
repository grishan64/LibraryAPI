namespace LibraryAPI.Data.Abstractions.Entities;

public interface IHasDeleteTime
{
    public DateTime? DeleteTime { get; set; }
}
