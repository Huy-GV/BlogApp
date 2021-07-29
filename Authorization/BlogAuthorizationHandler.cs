﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using BlogApp.Models;

namespace BlogApp.Authorization
{
    public class BlogAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Blog>
    {
        UserManager<IdentityUser> _userManager;
        public BlogAuthorizationHandler(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Blog resource)
        {
            if (context.User == null || resource == null)
            {
                return Task.CompletedTask;
            }

            if (requirement.Name != "Delete" && requirement.Name != "Edit" && requirement.Name != "Create")
                return Task.CompletedTask;

            if (resource.UserID == _userManager.GetUserId(context.User))
            {
                context.Succeed(requirement);
            }


            return Task.CompletedTask;
        }
    }
}
