using BusinessLogic.Extensions.Exceptions;
using System.Security.Claims;

namespace BusinessLogic.Extensions.Utils;

public static class Helpers
{
    /// <summary>
    ///     Auto get User Identity from jwt token in header request.
    ///     It use Tuple technique (a wrapper contain multi child variables) to return multiple data without need a wrapper class<para/>
    ///     It <c>MUST BE USED IN CONTROLLER LEVEL</c> because <c><paramref name="User">keyword</paramref>{keyword}</c> is a property of <c>ControllerBase</c><para/>
    ///     <![CDATA[Example:
    ///         var userInfo = User.GetUserIdentity();
    ///         _database.Query(a => a.Id == userInfo.id);
    ///         _database.Query(a => a.Role == userInfo.role);
    ///     ]]>
    ///     <![CDATA[Example 2:
    ///         var (userId, userRole) = User.GetUserIdentity();
    ///         _database.Query(a => a.Id == userId);
    ///         _database.Query(a => a.Role == userRole);...
    ///     ]]>
    /// </summary>
    /// <returns>(Guid <c>id</c>, string <c>role</c>) as name declared in the data types of method</returns>
    /// <exception cref="UnauthorizedException"></exception>
    public static (Guid id, string role) GetUserIdentity(this ClaimsPrincipal User)
    {
        var userIdString = User.FindFirst("Id")?.Value;

        if (Guid.TryParse(userIdString, out Guid userId) || userId == Guid.Empty)
            throw new UnauthorizedException("Token don't have valid User Id.");

        //var username = user.Identity?.Name;
        //var email = user.FindFirst(ClaimTypes.Email)?.Value;

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrWhiteSpace(userRole))
            throw new UnauthorizedException("Token don't have valid User Role.");

        return (userId, userRole);
    }
}
