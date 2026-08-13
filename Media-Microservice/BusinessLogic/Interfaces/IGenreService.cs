using BusinessLogic.DTOs.Messages;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Response;

namespace BusinessLogic.Interfaces;

public interface IGenreService
{
    Task<ResultRs<bool>> CreateGenreAsync(GenreRq request);
    Task<ResultRs<bool>> DeleteGenreAsync(Guid genreId);
    Task<ResultRs<IEnumerable<GenreRs>>> GetAllGenresAsync(bool includeDelete = false);
    Task<ResultRs<GenreRs>> GetGenreDetailAsync(Guid genreId);
    Task<ResultRs<bool>> UpdateGenreAsync(GenreRq request);
}