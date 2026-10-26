namespace Gas2s.SharedKernel;

public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
}
