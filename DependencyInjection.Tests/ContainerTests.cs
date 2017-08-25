using Battousai.DependencyInjection;
using System;
using Xunit;

namespace Battousai.DependencyInjection.Tests
{
    public class ContainerTests
    {
        private Container container;

        public ContainerTests()
        {
            container = new Container();
        }

        // Tests derived from requirements:

        [Fact]
        public void CanRegisterType()
        {
            container.Register<ITestService, TestService>();

            Assert.True(container.CanResolve<ITestService>());
        }

        [Fact]
        public void CanRegisterSingleton()
        {
            container.Register<ITestService, TestService>(LifecycleType.Singleton);

            var service1 = container.Resolve<ITestService>();
            var service2 = container.Resolve<ITestService>();

            Assert.Equal(service1, service2);
        }

        [Fact]
        public void CanRegisterTransient()
        {
            container.Register<ITestService, TestService>(LifecycleType.Transient);

            var service1 = container.Resolve<ITestService>();
            var service2 = container.Resolve<ITestService>();

            Assert.NotEqual(service1, service2);
        }

        [Fact]
        public void RegisterDefaultsToTransient()
        {
            container.Register<ITestService, TestService>();

            var service1 = container.Resolve<ITestService>();
            var service2 = container.Resolve<ITestService>();

            Assert.NotEqual(service1, service2);
        }

        [Fact]
        public void CanResolveRegisteredType()
        {
            container.Register<ITestService, TestService>();

            var service = container.Resolve<ITestService>();

            Assert.NotNull(service);
            Assert.IsType<TestService>(service);
        }

        [Fact]
        public void NonRegisteredResolveThrows()
        {
            Assert.Throws<ContainerResolveException>(() => { container.Resolve<ITestService>(); });
        }

        [Fact]
        public void CanResolveDependentService()
        {
            container.Register<IFirstChildTestService, FirstChildTestService>();
            container.Register<ISecondChildTestService, SecondChildTestService>();
            container.Register<IParentTestService, ParentTestService>();

            var service = container.Resolve<IParentTestService>();

            Assert.NotNull(service);
            Assert.IsType<ParentTestService>(service);
            Assert.NotNull(service.FirstChildService);
            Assert.NotNull(service.SecondChildService);
            Assert.IsType<FirstChildTestService>(service.FirstChildService);
            Assert.IsType<SecondChildTestService>(service.SecondChildService);
        }

        [Fact]
        public void NonFullyRegisteredDependentServiceThrows()
        {
            container.Register<ISecondChildTestService, SecondChildTestService>();
            container.Register<IParentTestService, ParentTestService>();

            Assert.Throws<ContainerResolveException>(() => { container.Resolve<IParentTestService>(); });
        }

        // Extra tests:

        [Fact]
        public void NonDerivingRegisteredTypeThrows()
        {
            // In this test, TestService does not implement IParentTestService.
            Assert.Throws<InvalidOperationException>(() => { container.Register<IParentTestService, TestService>(); });
        }

        [Fact]
        public void RegisteredTypeWithNoPublicConstructorThrows()
        {
            container.Register<ITestService, NoPublicConstructorTestService>();

            Assert.Throws<ContainerResolveException>(() => { container.Resolve<ITestService>(); });
        }

        // Specific examples referred to in the requirements

        [Fact]
        public void CanRegisterCalculator()
        {
            container.Register<ICalculator, Calculator>();

            Assert.True(container.CanResolve<ICalculator>());
        }

        [Fact]
        public void CanResolveCalculator()
        {
            container.Register<ICalculator, Calculator>();

            var service = container.Resolve<ICalculator>();

            Assert.NotNull(service);
            Assert.IsType<Calculator>(service);
        }

        [Fact]
        public void CanResolveUsersController()
        {
            container.Register<ICalculator, Calculator>();
            container.Register<IEmailService, EmailService>();
            container.Register<IUsersController, UsersController>();

            var service = container.Resolve<IUsersController>();

            Assert.NotNull(service);
            Assert.IsType<UsersController>(service);
            Assert.NotNull(service.Calculator);
            Assert.NotNull(service.EmailService);
            Assert.IsType<Calculator>(service.Calculator);
            Assert.IsType<EmailService>(service.EmailService);
        }
    }
}
