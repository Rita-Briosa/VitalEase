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
    public class ResetPasswordControllerTest
    {
        private readonly Mock<VitalEaseServerContext> _mockContext;
        private readonly Mock<DbSet<User>> _mockUserDbSet;
        private readonly Mock<DbSet<AuditLog>> _mockAuditLogDbSet;

        public ResetPasswordControllerTest()
        {
            _mockContext = new Mock<VitalEaseServerContext>();
            _mockUserDbSet = new Mock<DbSet<User>>();
            _mockAuditLogDbSet = new Mock<DbSet<AuditLog>>();

            // Configura o contexto simulado para retornar as coleções simuladas
            _mockContext.Setup(c => c.Users).Returns(_mockUserDbSet.Object);
            _mockContext.Setup(c => c.AuditLogs).Returns(_mockAuditLogDbSet.Object);
        }

    }
}
