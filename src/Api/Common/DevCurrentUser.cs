using Gas2s.SharedKernel;

namespace Gas2s.Api.Common;

public class DevCurrentUser : ICurrentUser
{
    public Guid? UserId { get; }
    public bool IsAuthenticated => UserId.HasValue;

    public DevCurrentUser(IHttpContextAccessor accessor, IConfiguration configuration)
    {
        var raw = accessor.HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault();
        UserId = Guid.TryParse(raw, out var id)
            ? id
            : Guid.Parse(configuration["DevAuth:DefaultUserId"]!);
    }
}
