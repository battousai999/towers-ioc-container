using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battousai.DependencyInjection
{
    public class ContainerResolveException : Exception
    {
        public string NonResolvableTypeFullName { get; private set; }

        public ContainerResolveException()
        {
        }

        public ContainerResolveException(string message)
            : base(message)
        {
        }

        public ContainerResolveException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public ContainerResolveException(string message, string nonResolvableTypeFullName)
            : base(message)
        {
            this.NonResolvableTypeFullName = nonResolvableTypeFullName;
        }

        public ContainerResolveException(string message, string nonResolvableTypeFullName, Exception inner)
            : base(message, inner)
        {
            this.NonResolvableTypeFullName = nonResolvableTypeFullName;
        }
    }
}
