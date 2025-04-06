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
    /// <summary>
    /// Controller responsible for managing audit logs.
    /// </summary>
    public class AuditLogsController : Controller
    {
        /// <summary>
        /// Database context used for operations related to audit logs.
        /// </summary>
        private readonly VitalEaseServerContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="AuditLogsController"/>.
        /// </summary>
        /// <param name="context">
        /// The database context (<see cref="VitalEaseServerContext"/>) injected to perform database access operations.
        /// </param>
        public AuditLogsController(VitalEaseServerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all audit logs from the database after removing outdated entries.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> that returns:
        /// <list type="bullet">
        ///   <item>
        ///     An HTTP 200 (OK) response with the list of audit logs in JSON format if logs are found.
        ///   </item>
        ///   <item>
        ///     An HTTP 400 (Bad Request) response with an empty list or error details if no logs are found or an error occurs.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method first calls the <see cref="DeleteLogs"/> method to remove outdated audit logs from the database.
        /// It then retrieves all remaining logs using <c>ToListAsync</c>. If the resulting list is empty,
        /// a BadRequest response is returned; otherwise, the logs are returned in a JSON format.
        /// If any exception occurs during the process, it is caught and returned as a BadRequest with an error message.
        /// </remarks>
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

        /// <summary>
        /// Retrieves audit logs from the database based on various optional filters.
        /// </summary>
        /// <param name="userId">
        /// An optional user identifier to filter logs for a specific user.
        /// </param>
        /// <param name="userEmail">
        /// An optional user email to filter logs based on the email associated with the user.
        /// </param>
        /// <param name="dateFrom">
        /// An optional starting date to filter logs that have a timestamp on or after this date.
        /// </param>
        /// <param name="dateTo">
        /// An optional ending date to filter logs that have a timestamp on or before this date.
        /// </param>
        /// <param name="actionType">
        /// An optional string to filter logs whose action contains the specified substring.
        /// </param>
        /// <param name="status">
        /// An optional string to filter logs whose status contains the specified substring.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// <list type="bullet">
        ///   <item>
        ///     an HTTP 200 (OK) response with the filtered list of audit logs in JSON format if the operation is successful,
        ///   </item>
        ///   <item>
        ///     an HTTP 400 (Bad Request) response with an error message if an exception occurs.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method first calls the <c>DeleteLogs</c> method to remove outdated logs from the database.
        /// It then builds a query on the <c>AuditLogs</c> table applying various optional filters:
        /// <list type="bullet">
        ///   <item>
        ///     If a valid <paramref name="userId"/> is provided, only logs for that user are included.
        ///   </item>
        ///   <item>
        ///     If a <paramref name="userEmail"/> is provided, the method retrieves the corresponding user ID and filters logs accordingly.
        ///   </item>
        ///   <item>
        ///     If <paramref name="dateFrom"/> or <paramref name="dateTo"/> are provided, the logs are filtered based on their timestamp.
        ///   </item>
        ///   <item>
        ///     If <paramref name="actionType"/> or <paramref name="status"/> are provided, the logs are filtered based on whether their action or status contains the specified substring.
        ///   </item>
        /// </list>
        /// Finally, the filtered logs are returned as a JSON response. If an error occurs during processing,
        /// the exception is caught and a Bad Request response is returned with an appropriate error message.
        /// </remarks>
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

                return Ok(logs);
            }
            catch (Exception ex)
            {
                //Handle error and return a bad request response
                return BadRequest(new { message = $"Error fetching logs from user with the id" });
            }
        }

        /// <summary>
        /// Deletes audit logs that are older than 6 months.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> that returns:
        /// <list type="bullet">
        ///   <item>
        ///     an HTTP 200 (OK) response with the list of deleted audit logs if logs older than 6 months exist,
        ///   </item>
        ///   <item>
        ///     an HTTP 400 (Bad Request) response with an empty list if no logs are found,
        ///   </item>
        ///   <item>
        ///     an HTTP 400 (Bad Request) response with an error message if an exception occurs.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following steps:
        /// <list type="bullet">
        ///   <item>
        ///     It calculates the current date and time, and retrieves all audit logs with a timestamp older than 6 months from now.
        ///   </item>
        ///   <item>
        ///     If no such logs are found, it returns a Bad Request response with an empty list.
        ///   </item>
        ///   <item>
        ///     Otherwise, it removes all the retrieved logs from the database and saves the changes.
        ///   </item>
        ///   <item>
        ///     Finally, it returns an OK response containing the list of deleted logs.
        ///   </item>
        /// </list>
        /// If an exception occurs during the process, it is caught and a Bad Request response with an error message is returned.
        /// </remarks>
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
