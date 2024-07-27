namespace SimpleForum.Core.Data.ViewModels;

public class PostPermissionViewModel<TId> where TId : notnull
{
    public required TId PostId { get; set; }
    public required string AuthorUserName { get; set; }
    public int? ReportTicketId { get; set; }
}
