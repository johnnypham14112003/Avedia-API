using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Request.Query;
using BusinessLogic.DTOs.Messages.Response;

namespace BusinessLogic.Interfaces;

public interface IActorService
{
    Task<ResultRs<bool>> CreateActorAsync(ActorRq request);
    Task<ResultRs<bool>> DeleteActorAsync(Guid actorId);
    Task<ResultRs<ActorRs>> GetActorDetailAsync(Guid actorId);
    Task<ResultRs<PagedResult<ActorRs>>> GetActorsPageAsync(PagingQueryRq<ActorQr> input);
    Task<ResultRs<bool>> UpdateActorAsync(ActorRq request);
}