using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalEase.Server.Data;

namespace VitalEaseTest
{
    public class ResetPasswordControllerTest
    {
        private readonly VitalEaseServerContext _context;

        public ResetPasswordControllerTest(VitalEaseContextFixture fixture)
        {
            _context = fixture.VitalEaseTestContext;
        }
    }

}
