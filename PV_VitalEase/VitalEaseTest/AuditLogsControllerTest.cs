using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalEase.Server.Data;
using VitalEase.Server.Models;

namespace VitalEaseTest
{
    public class AuditLogsControllerTest
    {
        private readonly VitalEaseServerContext _context;

        public AuditLogsControllerTest(VitalEaseContextFixture fixture)
        {
            _context = fixture.VitalEaseTestContext;
        }

    }
}
