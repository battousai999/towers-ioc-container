using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battousai.DependencyInjection.Tests
{
    public interface ITestService
    {
    }

    public class TestService : ITestService
    {
    }

    public class NoPublicConstructorTestService : ITestService
    {
        private NoPublicConstructorTestService()
        {
        }
    }
}
