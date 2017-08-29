using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battousai.DependencyInjection
{
    public class TransientTypeContext: BaseTypeContext
    {
        public TransientTypeContext(Type registeredType) : base(registeredType)
        {
        }
    }
}
