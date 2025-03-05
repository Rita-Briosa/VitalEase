using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using VitalEase.Server.Data;
using VitalEase.Server.Models;

namespace VitalEase.Server.Controllers
{
    public class AuditLogsController : Controller
    {
        private readonly VitalEaseServerContext _context;

        public AuditLogsController(VitalEaseServerContext context)
        {
            _context = context;
        }

        [HttpGet("api/getLogs")]
        public async Task<IActionResult> GetLogs()
        {
            await DeleteLogs();
            try
            {
                // Fetch all logs from the database
                var logs = await _context.AuditLogs.ToListAsync();


                if (logs.IsNullOrEmpty()) {
                    return BadRequest(new List<AuditLog>());
                }

                // Return the logs in JSON format
                return Ok(logs);
            }
            catch (Exception ex)
            {
                // Handle error and return a bad request response
                return BadRequest(new { message = "Error fetching logs", error = ex.Message });
            }
        }

        [HttpGet("api/getLogsFilter")]
        public async Task<IActionResult> GetLogs(int? userId, string? userEmail, DateTime? dateFrom, DateTime? dateTo, string? actionType, string? status)
        {
            await DeleteLogs();
            try
            {
                var query = _context.AuditLogs.AsQueryable();

                if (userId.HasValue && userId >= 0)
                {
                    query = query.Where(log => log.UserId == userId);
                }

                if (!string.IsNullOrEmpty(userEmail))
                {
                    var userIdByEmail = await _context.Users.Where(user => user.Email == userEmail).Select(user => user.Id).FirstOrDefaultAsync();
                    query = query.Where(log => log.UserId == userIdByEmail);
                }

                if (dateFrom.HasValue)
                {
                    query = query.Where(log => log.Timestamp >= dateFrom);
                }

                if (dateTo.HasValue)
                {
                    query = query.Where(log => log.Timestamp <= dateTo);
                }

                if (!string.IsNullOrEmpty(actionType))
                {
                    query = query.Where(log => log.Action.Contains(actionType));
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(log => log.Status.Contains(status));
                }

                // Fetch all logs with UserID 
                var logs = await query.ToListAsync();

                if (logs.IsNullOrEmpty())
                {
                    return BadRequest(new List<AuditLog>());
                }

                if (!logs.Any())
                {
                    return NotFound(new { message = "No logs found for this user." });
                }

                return Ok(logs);
            }
            catch (Exception ex)
            {
                //Handle error and return a bad request response
                return BadRequest(new { message = $"Error fetching logs from user with the id" });
            }
        }

        [HttpDelete]
        private async Task <IActionResult> DeleteLogs()
        {
            try
            {
            DateTime now = DateTime.Now;
            var logs = await _context.AuditLogs.Where(logs => logs.Timestamp <= now.AddMonths(-6)).ToListAsync();

            if (logs.IsNullOrEmpty())
            {
                return BadRequest(new List<AuditLog>());
            }
           
            _context.AuditLogs.RemoveRange(logs);
            await _context.SaveChangesAsync();

            return Ok(logs);

            }
            catch
            {
                return BadRequest(new { message = $"Error Deleting Logs" });
            }
        }
    }
}
