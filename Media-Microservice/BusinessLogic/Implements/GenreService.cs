using BusinessLogic.DTOs.Messages;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Response;
using BusinessLogic.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Models;
using Mapster;

namespace BusinessLogic.Implements;

public class GenreService(IUnitOfWork unitOfWork) : IGenreService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    // ===========================< METHODS >===========================
    public async Task<ResultRs<bool>> CreateGenreAsync(GenreRq request)
    {
        var genreRepo = _unitOfWork.GetRepository<Genre>();

        // Query exist Genre
        var existGenre = await genreRepo.GetOneAsync(a => a.Title == request.Title);

        if (existGenre is null)
            return ResultRs<bool>.Failure("This genre already exist!");

        var newGenre = request.Adapt<Genre>();
        newGenre.Id = Guid.NewGuid();

        newGenre.Status = "Active";

        await genreRepo.AddAsync(newGenre);

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true)
            : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<GenreRs>> GetGenreDetailAsync(Guid genreId)
    {
        var genreRepo = _unitOfWork.GetRepository<Genre>();

        var genre = await genreRepo.GetOneAsync(
            expression: a => a.Id == genreId,
            hasTracking: false
        );

        if (genre is null)
            return ResultRs<GenreRs>.NotFound("Not found this genre!");

        return ResultRs<GenreRs>.Ok(genre.Adapt<GenreRs>());
    }

    public async Task<ResultRs<IEnumerable<GenreRs>>> GetAllGenresAsync(bool includeDelete = false)
    {
        static IQueryable<Genre> OrderByTitle(IQueryable<Genre> query) => query.OrderByDescending(g => g.Title);

        var response = await _unitOfWork.GetRepository<Genre>().GetListAsync(includeDelete ? (g => true) : (g => !g.Status.Equals("deleted")), OrderByTitle);

        // 5. Return mapping object
        return ResultRs<IEnumerable<GenreRs>>.Ok(response.Adapt<IEnumerable<GenreRs>>());
    }

    public async Task<ResultRs<bool>> UpdateGenreAsync(GenreRq request)
    {
        var genreRepo = _unitOfWork.GetRepository<Genre>();

        // Query exist Genre
        var existGenre = await genreRepo.GetOneAsync(a => a.Id == request.Id);

        if (existGenre is null)
            return ResultRs<bool>.NotFound("Not found this genre!");

        // Map new data to exist data
        request.Adapt(existGenre);

        return (await _unitOfWork.CompleteAsync() > 0)
             ? ResultRs<bool>.OkBool(true)
             : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<bool>> DeleteGenreAsync(Guid genreId)
    {
        var genreRepo = _unitOfWork.GetRepository<Genre>();
        var existGenre = await genreRepo.GetByIdAsync(genreId);

        if (existGenre == null)
            return ResultRs<bool>.NotFound("Not found this genre!");

        // Soft delete
        existGenre.Status = "Deleted";

        // Hard Delete
        //await genreRepo.DeleteAsync(existGenre);

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true)
            : ResultRs<bool>.Failure();
    }
}
