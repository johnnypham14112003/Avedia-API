using AppGrpc.Protos;
using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Request.Query;
using BusinessLogic.Interfaces;
using Grpc.Core;
using Mapster;

namespace AppGrpc.Services;

public class AccountGrpcEndpoint : AccountGrpcService.AccountGrpcServiceBase
{
    private readonly IAccountService _accountService;
    public AccountGrpcEndpoint(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public override async Task<AccountResponse> GetByPasswordAsync(AuthRequest request, ServerCallContext context)
    {
        var result = await _accountService.GetByPasswordAsync(request.Adapt<AuthRq>());

        return new AccountResponse
        {
            ResultResponse = result.Adapt<ResultResponse>(),
            AccountInfo = result.Data.Adapt<AccountInfo>()
        };
    }

    public override async Task<AccountResponse> GetNewRefreshToken(AccountGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new AccountResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _accountService.RefreshTokenAsync(parsedId, request.RefreshToken);

        return new AccountResponse
        {
            ResultResponse = result.Adapt<ResultResponse>(),
            AccountInfo = result.Data.Adapt<AccountInfo>()
        };
    }

    public override async Task<AccountResponse> RevokeRefreshTokenAsync(AccountGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new AccountResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _accountService.RevokeRefreshTokenAsync(parsedId);

        return new AccountResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<AccountResponse> ChangePasswordAsync(AuthRequest request, ServerCallContext context)
    {
        var result = await _accountService.ChangePasswordAsync(request.Email, request.Password);

        return new AccountResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<AccountResponse> CreateAccountAsync(AuthRequest request, ServerCallContext context)
    {
        var result = await _accountService.CreateAccountAsync(request.Adapt<AuthRq>());

        return new AccountResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<AccountResponse> GetAccountAsync(AccountGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new AccountResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _accountService.GetAccountAsync(parsedId, request.IncludeBadge);

        // --------- MAP DATA TO PROTO ---------
        // Create proto message response [AccountResponse](0)
        var response = new AccountResponse
        {
            // Assign to proto message [ResultResponse](1)
            ResultResponse = result.Adapt<ResultResponse>()
        };

        if (result.Data != null)
        {
            // Assign to proto message [AccountInfo](2)
            response.AccountInfo = result.Data.Adapt<AccountInfo>();

            // If have list Badge, use Linq to extract and assign
            if (result.Data.AccountBadges != null && result.Data.AccountBadges.Count != 0)
            {
                var mappedAccountBadgesResponse = result.Data.AccountBadges.Select(ab => new AccountResponse.Types.AccountBadgeResponse // AccountBadgeResponse is a nested message inside AccountResponse
                {
                    // Mapster will get AccountId, BadgeId, AwardedDate from ab to assign in AccountBadgeInfo
                    AccountBadgeInfo = ab.Adapt<AccountBadgeInfo>(),

                    // Get navigation property Badge to map into BadgeInfo (check null before get it and adapt to prevent error)
                    BadgeInfo = (ab.Badge != null) ? ab.Badge.Adapt<BadgeInfo>() : null
                });

                // Because repeated in proto is read-only so can't assign directly.
                // Use AddRange to put in array repeated of Protobuf
                response.AccountBadgeResponse.AddRange(mappedAccountBadgesResponse);
            }
        }

        return response;
    }

    public override async Task<AccountPageResponse> GetAccountListAsync(AccountPageRequest request, ServerCallContext context)
    {
        // For parse DateOnly safety
        DateOnly? parsedFromDate = null;
        DateOnly? parsedToDate = null;

        var pageQuery = request.PageQueryRequest;
        var advanceInput = request.AdvanceInput;

        if (request.AdvanceInput != null)
        {
            if (!string.IsNullOrEmpty(request.AdvanceInput.FromDate))
                parsedFromDate = DateOnly.Parse(advanceInput.FromDate);

            if (!string.IsNullOrEmpty(request.AdvanceInput.ToDate))
                parsedToDate = DateOnly.Parse(advanceInput.ToDate);
        }

        // ==========[ Mapping PROTO -> DTOs ]==========
        var queryInput = new PagingQueryRq<AccountQr>
        {
            Keyword = pageQuery.Keyword,
            PageNumber = pageQuery.PageNumber,
            PageSize = pageQuery.PageSize,
            AdvanceInput = (advanceInput == null) ? null :
            new AccountQr
            {
                IsVerified = advanceInput.IsVerified,
                Gender = (short)advanceInput.Gender,
                Nationality = advanceInput.Nationality,
                Role = advanceInput.Role,
                Status = advanceInput.Status,
                FromDate = parsedFromDate,
                ToDate = parsedToDate
            }
        };

        // Call Repository Method to query in database
        var pagedResult = await _accountService.GetAccountsPageAsync(queryInput);

        // ==========[ Mapping DATA -> PROTO ]==========
        // Create proto message [AccountPageResponse](0)
        var response = new AccountPageResponse
        {
            // Assign to proto message [ResultResponse](1)
            ResultResponse = pagedResult.Adapt<ResultResponse>()
        };

        if (pagedResult.Data != null)
        {
            // Create proto message [AccountPagedResult](2*)
            var pagedDataProto = new AccountPageResponse.Types.AccountPagedResult
            {
                // Assign to proto message [BasePageResult](2.1)
                BasePageResult = new BasePageResult
                {
                    PageIndex = pagedResult.Data.PageIndex,
                    PageSize = pagedResult.Data.PageSize,
                    TotalCount = pagedResult.Data.TotalCount,
                    TotalPage = pagedResult.Data.TotalPage
                }
            };

            // Assign to proto message [repeated AccountInfo](2.2)
            if (pagedResult.Data.DataList != null && pagedResult.Data.DataList.Any())
            {
                // AccountRs:cs -> AccountInfo:proto
                var mappedList = pagedResult.Data.DataList.Adapt<IEnumerable<AccountInfo>>();
                pagedDataProto.DataList.AddRange(mappedList);
            }
            // Assign to proto message [AccountPagedResult](2)
            response.PagedData = pagedDataProto;
        }

        return response;
    }

    public override async Task<AccountResponse> UpdateAccountAsync(AccountRequest request, ServerCallContext context)
    {
        var result = await _accountService.UpdateAccountAsync(request.Adapt<AccountRq>(), request.UpdateAll);

        return new AccountResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<AccountResponse> DeleteAccountAsync(AccountGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new AccountResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _accountService.DeleteAccountAsync(parsedId);

        return new AccountResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<AccountResponse> DeletePermanentAccountAsync(AccountGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new AccountResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _accountService.DeletePermanentAccountAsync(parsedId);

        return new AccountResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }
}
