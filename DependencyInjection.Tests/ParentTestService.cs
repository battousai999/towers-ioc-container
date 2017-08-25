using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battousai.DependencyInjection.Tests
{
    public interface IParentTestService
    {
        IFirstChildTestService FirstChildService { get; }
        ISecondChildTestService SecondChildService { get; }
    }

    public class ParentTestService : IParentTestService
    {
        public IFirstChildTestService FirstChildService { get; private set; }
        public ISecondChildTestService SecondChildService { get; private set; }

        public ParentTestService(IFirstChildTestService firstService, ISecondChildTestService secondService)
        {
            this.FirstChildService = firstService;
            this.SecondChildService = secondService;
        }
    }

    public interface IFirstChildTestService
    {
    }

    public class FirstChildTestService : IFirstChildTestService
    {
    }

    public interface ISecondChildTestService
    {
    }

    public class SecondChildTestService : ISecondChildTestService
    {
    }
}
