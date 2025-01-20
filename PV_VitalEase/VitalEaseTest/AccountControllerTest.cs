using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalEase.Server.Data;

namespace VitalEaseTest
{
    public class AccountControllerTest : IClassFixture<VitalEaseContextFixture>
    {
        private readonly VitalEaseServerContext _context;

        public AccountControllerTest (VitalEaseContextFixture fixture)
        {
            _context = fixture.VitalEaseTestContext;
        }


    }
}
