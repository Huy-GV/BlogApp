namespace RazorBlog.Communication;

public enum ServiceResultCode
{
    Success,
    Unauthenticated,
    Unauthorized,
    NotFound,
    InvalidState,
    InvalidArguments,
    Error,
}
