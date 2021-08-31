﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BlogApp.Data;
using BlogApp.Models;
using Microsoft.AspNetCore.Identity;
using BlogApp.Pages;

namespace BlogApp.Pages.Blogs
{
    [AllowAnonymous]
    public class IndexModel : BaseModel
    {
        public IndexModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager) : base(context, userManager)
        {

        }
        [BindProperty]
        public InputBlog InputBlog { get; set; }

        public IList<Blog> Blog { get;set; }

        public async Task OnGetAsync()
        {
            
            Blog = await Context
                .Blog
                .Include(blog => blog.Comments)
                .ToListAsync();

            if (User.Identity.IsAuthenticated)
            {
                var user = await UserManager.GetUserAsync(User);
                await CheckSuspensionExpiry(user.UserName);
                ViewData["IsSuspended"] = SuspensionExists(user.UserName);
            } else
            {
                ViewData["IsSuspended"] = false;
            }

        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge();
            }

            var user = await UserManager.GetUserAsync(User);
            var username = user.UserName;
            await CheckSuspensionExpiry(username);
            if (SuspensionExists(username))
                return RedirectToPage("./Index");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ERROR");
                return Page();
            }
            var blog = new Blog
            {
                Title = InputBlog.Title,
                Content = InputBlog.Content,
                Date = DateTime.Now,
                Author = user.UserName
            };

            Context.Blog.Add(blog);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }

    public class InputBlog
    { 
        public string Title { get; set; }
        public string Content { get; set; }
    }

}
