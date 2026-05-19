using AppGrpc.Protos;
using BusinessLogic.DTOs.Messages.Request;
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
            ResultResponse = new ResultResponse
            {
                Success = result.Success,
                ErrorCode = result.HttpCode,
                ErrorMessage = result.ErrorMessage,
            },
            AccountInfo = result.Data.Adapt<AccountInfo>()
        };
    }

    public override async Task<AccountResponse> GetNewRefreshToken(TokenRequest request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.AccountId, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new AccountResponse { ResultResponse = new ResultResponse { ErrorCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _accountService.RefreshTokenAsync(parsedId, request.RefreshToken);

        return new AccountResponse
        {
            ResultResponse = new ResultResponse
            {
                Success = result.Success,
                ErrorCode = result.HttpCode,
                ErrorMessage = result.ErrorMessage,
            },
            AccountInfo = result.Data.Adapt<AccountInfo>()
        };
    }

    public override async Task<AccountResponse> RevokeRefreshTokenAsync(AccountGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new AccountResponse { ResultResponse = new ResultResponse { ErrorCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _accountService.RevokeRefreshTokenAsync(parsedId);

        return new AccountResponse
        {
            ResultResponse = new ResultResponse
            {
                Success = result.Success,
                ErrorCode = result.HttpCode,
                ErrorMessage = result.ErrorMessage,
            }
        };
    }

    public override async Task<AccountResponse> ChangePasswordAsync(AuthRequest request, ServerCallContext context)
    {
        var result = await _accountService.ChangePasswordAsync(request.Email, request.Password);

        return new AccountResponse
        {
            ResultResponse = new ResultResponse
            {
                Success = result.Success,
                ErrorCode = result.HttpCode,
                ErrorMessage = result.ErrorMessage,
            }
        };
    }

    public override async Task<AccountResponse> CreateAccountAsync(AuthRequest request, ServerCallContext context)
    {
        var result = await _accountService.CreateAccountAsync(request.Adapt<AuthRq>());

        return new AccountResponse
        {
            ResultResponse = new ResultResponse
            {
                Success = result.Success,
                ErrorCode = result.HttpCode,
                ErrorMessage = result.ErrorMessage,
            }
        };
    }

    public override async Task<AccountResponse> GetAccountAsync(AccountGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new AccountResponse { ResultResponse = new ResultResponse { ErrorCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _accountService.GetAccountAsync(parsedId, request.IncludeBadge);

        var response = new AccountResponse
        {
            ResultResponse = new ResultResponse
            {
                Success = result.Success,
                ErrorCode = result.HttpCode,
                ErrorMessage = result.ErrorMessage,
            },
        };

        // --------- MAP DATA TO PROTO ---------
        if (result.Data != null)
        {
            // Map default Account
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

    public override async Task<AccountResponse> UpdateAccountAsync(AccountRequest request, ServerCallContext context)
    {
        var result = await _accountService.UpdateAccountAsync(request.Adapt<AccountRq>(), request.UpdateAll);

        return new AccountResponse
        {
            ResultResponse = new ResultResponse
            {
                Success = result.Success,
                ErrorCode = result.HttpCode,
                ErrorMessage = result.ErrorMessage,
            }
        };
    }

    public override async Task<AccountResponse> DeleteAccountAsync(AccountGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new AccountResponse { ResultResponse = new ResultResponse { ErrorCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _accountService.DeleteAccountAsync(parsedId);

        return new AccountResponse
        {
            ResultResponse = new ResultResponse
            {
                Success = result.Success,
                ErrorCode = result.HttpCode,
                ErrorMessage = result.ErrorMessage,
            }
        };
    }
}
