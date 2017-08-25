using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Battousai.DependencyInjection
{
    public class Container
    {
        private class TypeContext
        {
            public Type RegisteredType { get; private set; }
            public LifecycleType Lifecycle { get; private set; }
            public Object SingletonInstance { get; set; }

            public TypeContext(Type registeredType, LifecycleType lifecycle)
            {
                this.RegisteredType = registeredType;
                this.Lifecycle = lifecycle;
            }
        }

        private Dictionary<Type, TypeContext> registeredTypes = new Dictionary<Type, TypeContext>();

        public void Register<ServiceInterface, Service>(LifecycleType lifecycle = LifecycleType.Transient)
        {
            var serviceInterfaceType = typeof(ServiceInterface);
            var serviceType = typeof(Service);

            if (!serviceInterfaceType.IsAssignableFrom(serviceType))
                throw new InvalidOperationException($"Cannot register the service type \"{typeof(Service).Name}\" because it does not implement the service interface \"{typeof(ServiceInterface).Name}\".");

            registeredTypes[serviceInterfaceType] = new TypeContext(serviceType, lifecycle);
        }

        public ServiceInterface Resolve<ServiceInterface>()
        {
            var serviceInterfaceType = typeof(ServiceInterface);
            var instance = Resolve(serviceInterfaceType);

            // This cast should always succeed because, within Resolve(), CreateServiceInstance() throws if 
            // the cast were invalid.
            return (ServiceInterface)instance;
        }

        private object Resolve(Type serviceInterfaceType)
        {
            TypeContext context;            

            if (!registeredTypes.TryGetValue(serviceInterfaceType, out context))
                throw new ContainerResolveException($"Could not resolve type \"{serviceInterfaceType.Name}\" because it has not been registered.", serviceInterfaceType.FullName);

            if (context.Lifecycle == LifecycleType.Singleton)
            {
                if (context.SingletonInstance == null)
                    context.SingletonInstance = CreateServiceInstance(context.RegisteredType, serviceInterfaceType);
                                
                return context.SingletonInstance;
            }
            else if (context.Lifecycle == LifecycleType.Transient)
                return CreateServiceInstance(context.RegisteredType, serviceInterfaceType);
            else
                throw new InvalidOperationException("Unexpected lifecycle type: " + context.Lifecycle.ToString());
        }

        private object CreateServiceInstance(Type serviceType, Type serviceInterfaceType)
        {
            var constructors = serviceType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            // Find the minimal constructor (i.e., the one with the fewest parameters).
            var constructor = constructors
                .OrderBy(x => x.GetParameters().Length)
                .FirstOrDefault();

            if (constructor == null)
                throw new ContainerResolveException($"Type was resolved, but its resolved type ({serviceType.Name}) could not be instantiated because it has no public constructors.", serviceType.FullName);

            var constructorParameters = constructor.GetParameters();

            object instance;

            if (!constructorParameters.Any())
                instance = constructor.Invoke(null);
            else
            {
                // As a quick check, look for constructor parameters that we know cannot be resolved (so that
                // we don't resolve/construct some parameters when it's going to fail).
                var firstNonResolvableParameter = constructorParameters.FirstOrDefault(x => !CanResolve(x.ParameterType));

                if (firstNonResolvableParameter != null)
                    throw new ContainerResolveException($"Cannot finish resolving type \"{serviceInterfaceType.Name}\" because the constructor of its resolved type ({serviceType.Name}) has a dependency ({firstNonResolvableParameter.ParameterType.Name}) that could not be resolved.", firstNonResolvableParameter.ParameterType.FullName);

                // Map constructor parameters to corresponding resolved instances.
                var constructorParameterInstances = constructorParameters
                    .Select(x => Resolve(x.ParameterType))
                    .ToArray();

                instance = constructor.Invoke(constructorParameterInstances);
            }

            // Make sure that the new instance is assignable to the service interface.
            if (serviceInterfaceType.IsAssignableFrom(instance.GetType()))
                return instance;
            else
                throw new InvalidOperationException($"Could not cast registered type \"{serviceType.Name}\" as expected type \"{serviceInterfaceType.Name}\".");
        }

        public bool CanResolve<ServiceInterface>()
        {
            return CanResolve(typeof(ServiceInterface));
        }

        private bool CanResolve(Type type)
        {
            return registeredTypes.ContainsKey(type);
        }
    }
}
