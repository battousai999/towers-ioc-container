using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battousai.DependencyInjection
{
    public class SingletonTypeContext: BaseTypeContext
    {
        public object Instance { get; set; }

        public SingletonTypeContext(Type registeredType) : base(registeredType)
        {
        }

        public override object GetServiceInstance(Func<Type, Type, object> creator, Type serviceInterfaceType)
        {
            if (Instance == null)
                Instance = creator(RegisteredType, serviceInterfaceType);

            return Instance;
        }
    }
}
