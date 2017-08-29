using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battousai.DependencyInjection
{
    public class BaseTypeContext
    {
        public Type RegisteredType { get; private set; }

        public BaseTypeContext(Type registeredType)
        {
            this.RegisteredType = registeredType;
        }

        public static BaseTypeContext CreateTypeContext(Type registeredType, LifecycleType lifecycle)
        {
            switch (lifecycle)
            {
                case LifecycleType.Transient:
                    return new TransientTypeContext(registeredType);
                case LifecycleType.Singleton:
                    return new SingletonTypeContext(registeredType);
                default:
                    throw new InvalidOperationException("Unexpected lifecycle type: " + lifecycle.ToString());
            }
        }

        public virtual object GetServiceInstance(Func<Type, Type, object> creator, Type serviceInterfaceType)
        {
            return creator(RegisteredType, serviceInterfaceType);
        }
    }
}
