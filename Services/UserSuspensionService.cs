using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using BlogApp.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BlogApp.Models;

namespace BlogApp.Services
{
    public class UserSuspensionService
    {
        private readonly IWebHostEnvironment _webHostEnv;
        private readonly ILogger<ImageFileService> _logger; 
        private readonly ApplicationDbContext _dbContext;
        public UserSuspensionService(ApplicationDbContext dbContext, ILogger<ImageFileService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task<bool> ExistsAsync(string username)
        {
            await CheckExpiryAsync(username);
            return _dbContext.Suspension.Any(s => s.Username == username);
        }
        private async Task CheckExpiryAsync(string username)
        {
            var suspension = await FindAsync(username);
            if (suspension != null && DateTime.Compare(DateTime.Now, suspension.Expiry) > 0)
            {
                await RemoveAsync(suspension);
            }
        }

        public async Task<Suspension> FindAsync(string username) {
            return await _dbContext
            .Suspension
            .FirstOrDefaultAsync(s => s.Username == username);
        } 
        public async Task RemoveAsync(Suspension suspension)
        {
            _dbContext.Remove(suspension);
            await _dbContext.SaveChangesAsync();
        }
    }
}
