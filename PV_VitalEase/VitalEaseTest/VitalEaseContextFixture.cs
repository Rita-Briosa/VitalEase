using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalEase.Server.Data;

namespace VitalEaseTest
{
    public class VitalEaseContextFixture : DbContext
    {
        public VitalEaseServerContext VitalEaseTestContext { get; set; }

        public VitalEaseContextFixture() {

            var options = new DbContextOptionsBuilder<VitalEaseServerContext>()
                .UseInMemoryDatabase("VitalEaseTest")  // Nome do banco em memória
                .Options;


            VitalEaseTestContext = new VitalEaseServerContext(options);


            VitalEaseTestContext.Database.EnsureCreated();
        }

    }
}
