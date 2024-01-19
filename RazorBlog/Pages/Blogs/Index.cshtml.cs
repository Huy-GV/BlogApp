using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorBlog.Data;
using RazorBlog.Data.Constants;
using RazorBlog.Data.Dtos;
using RazorBlog.Models;
using RazorBlog.Services;

namespace RazorBlog.Pages.Blogs;

[AllowAnonymous]
public class IndexModel : RichPageModelBase<IndexModel>
{
    private readonly IBlogReader _blogReader;
    public IndexModel(RazorBlogDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<IndexModel> logger, 
        IBlogReader blogReader) : base(context, userManager, logger)
    {
        _blogReader = blogReader;
    }

    [BindProperty]
    public IReadOnlyCollection<IndexBlogDto> Blogs { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string SearchString { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        Blogs = await _blogReader.GetBlogsAsync(searchString: SearchString.Trim().Trim(' '));
    }
}
