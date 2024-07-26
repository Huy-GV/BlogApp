using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.Dtos;
using SimpleForum.Core.Models;
using SimpleForum.Core.ReadServices;

namespace SimpleForum.Web.Pages.Threads;

[AllowAnonymous]
public class IndexModel : RichPageModelBase<IndexModel>
{
    private readonly IThreadReader _threadReader;
    public IndexModel(
        SimpleForumDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<IndexModel> logger,
        IThreadReader threadReader) : base(context, userManager, logger)
    {
        _threadReader = threadReader;
    }

    [BindProperty]
    public IReadOnlyCollection<IndexThreadDto> Threads { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string SearchString { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        Threads = await _threadReader.GetThreadsAsync(searchString: SearchString.Trim().Trim(' '));
    }
}
