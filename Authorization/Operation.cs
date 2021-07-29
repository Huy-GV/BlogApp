using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace BlogApp.Authorization
{
    public static class Operations
    {
        public static OperationAuthorizationRequirement Create =
          new OperationAuthorizationRequirement { Name = "Create" };
        public static OperationAuthorizationRequirement Update =
          new OperationAuthorizationRequirement { Name = "Update" };
        public static OperationAuthorizationRequirement Delete =
          new OperationAuthorizationRequirement { Name = "Delete" };
    }
}
