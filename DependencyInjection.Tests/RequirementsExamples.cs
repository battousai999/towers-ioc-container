using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battousai.DependencyInjection.Tests
{
    public interface ICalculator
    {
    }

    public class Calculator : ICalculator
    {
    }

    public interface IEmailService
    {
    }

    public class EmailService : IEmailService
    {
    }

    public interface IUsersController
    {
        ICalculator Calculator { get; }
        IEmailService EmailService { get; }
    }

    public class UsersController : IUsersController
    {
        public ICalculator Calculator { get; private set; }
        public IEmailService EmailService { get; private set; }

        public UsersController(ICalculator calculator, IEmailService emailService)
        {
            this.Calculator = calculator;
            this.EmailService = emailService;
        }
    }
}
