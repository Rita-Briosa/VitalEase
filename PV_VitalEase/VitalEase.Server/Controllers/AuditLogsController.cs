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

        [HttpGet("getLogs")]
        public async Task<IActionResult> GetLogs()
        {
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
    }
}
