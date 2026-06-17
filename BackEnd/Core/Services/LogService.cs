using BackEnd.Core.DbContext;
using BackEnd.Core.Dtos.Log;
using BackEnd.Core.Entities;
using BackEnd.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BackEnd.Core.Services
{
    public class LogService : ILogService
    {
        #region Constructor & DI
        private readonly ApplicationDbContext _context;

        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }
        #endregion

        #region GetLogsAsync
        public async Task<IEnumerable<GetLogDto>> GetLogsAsync()
        {
            try
            {
                var logs = await _context.Logs
                     .Select(q => new GetLogDto
                     {
                         CreatedAt = q.CreatedAt,
                         Description = q.Description,
                         UserName = q.UserName,
                     })
                     .OrderByDescending(q => q.CreatedAt)
                     .ToListAsync();
                return logs;
            }
            catch (Exception ex) 
            {
                Serilog.Log.Error("Failure : {@RequestName} , {@Error} , {@DateTimeUTC}",
                    "GetLogsAsync", ex.Message, DateTime.Today);
                return null;
            }
        }
        #endregion

        #region GetMyLogsAsync
        public async Task<IEnumerable<GetLogDto>> GetMyLogsAsync(ClaimsPrincipal User)
        {
            try
            {
                var logs = await _context.Logs
                    .Where(q => q.UserName == User.Identity.Name)
                   .Select(q => new GetLogDto
                   {
                       CreatedAt = q.CreatedAt,
                       Description = q.Description,
                       UserName = q.UserName,
                   })
                   .OrderByDescending(q => q.CreatedAt)
                   .ToListAsync();
                return logs;
            }
            catch(Exception ex) 
            {
                Serilog.Log.Error("Failure : {@RequestName} , {@Error} , {@DateTimeUTC}",
                    "GetMyLogsAsync", ex.Message, DateTime.Today);
                return null;
            }
        }
        #endregion

        #region SaveNewLog
        public async Task SaveNewLog(string UserName, string Description)
        {
            try
            {
                var newLog = new Log()
                {
                    UserName = UserName,
                    Description = Description
                };

                await _context.Logs.AddAsync(newLog);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex) 
            {
                Serilog.Log.Error("Failure : {@RequestName} , {@Error} , {@DateTimeUTC}",
                    "SaveNewLog", ex.Message, DateTime.Today);
            }
        }
        #endregion
    }
}
