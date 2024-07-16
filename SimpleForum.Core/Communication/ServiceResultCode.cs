namespace SimpleForum.Core.Communication;

public enum ServiceResultCode
{
    /// <summary>
    /// The operation was successful.
    /// </summary>
    Success,

    /// <summary>
    /// The user is not authenticated and needs to log in.
    /// </summary>
    Unauthenticated,

    /// <summary>
    /// The user is authenticated but doesn't have the necessary permissions for the operation.
    /// </summary>
    Unauthorized,

    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The operation cannot be performed in the current state of the system.
    /// </summary>
    InvalidState,

    /// <summary>
    /// The provided arguments or parameters are invalid.
    /// </summary>
    InvalidArguments,

    /// <summary>
    /// An unknown error occurred during the operation.
    /// </summary>
    Error,
}
