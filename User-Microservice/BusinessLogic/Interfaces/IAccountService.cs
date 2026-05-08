using BusinessLogic.Models.Generic;
using BusinessLogic.Models.View.Request;
using BusinessLogic.Models.View.Response;

namespace BusinessLogic.Interfaces;

public interface IAccountService
{
    Task<ApiResult<AuthRs>> LoginByPasswordAsync(AuthRq authRequest);
    Task<ApiResult<AuthRs>> RefreshAccessToken(RefreshTokenRq request);
}
