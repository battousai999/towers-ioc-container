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
        private Dictionary<Type, BaseTypeContext> registeredTypes = new Dictionary<Type, BaseTypeContext>();

        /// <summary>
        /// Registers the type "Service" for the type "ServiceInterface".  After this registration, when
        /// a Resolve call is made for the "ServiceInterface" type, an instance of the "Service" type will
        /// be returned.  The "lifecycle" of the object (returned by the Resolve method) will be controlled
        /// by the "lifecycle" parameter passed to this method.
        /// </summary>
        /// <typeparam name="ServiceInterface">The type under which to register the "Service" type.</typeparam>
        /// <typeparam name="Service">The type to associate with the "ServiceInterface" type.</typeparam>
        /// <param name="lifecycle">
        /// A value controlling the construction of instances of the "Service" type when 
        /// resolved.
        /// </param>
        public void Register<ServiceInterface, Service>(LifecycleType lifecycle = LifecycleType.Transient)
        {
            var serviceInterfaceType = typeof(ServiceInterface);
            var serviceType = typeof(Service);

            if (!serviceInterfaceType.IsAssignableFrom(serviceType))
                throw new InvalidOperationException($"Cannot register the service type \"{typeof(Service).Name}\" because it does not implement the service interface \"{typeof(ServiceInterface).Name}\".");

            registeredTypes[serviceInterfaceType] = BaseTypeContext.CreateTypeContext(serviceType, lifecycle);
        }

        /// <summary>
        /// Returns an instance of the type associated with the "ServiceInterface" type (via a previously-
        /// occurring Register call).
        /// </summary>
        /// <typeparam name="ServiceInterface">The type to resolve.</typeparam>
        /// <returns>An instance of the "ServiceInterface" type.</returns>
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
            BaseTypeContext context;            

            if (!registeredTypes.TryGetValue(serviceInterfaceType, out context))
                throw new ContainerResolveException($"Could not resolve type \"{serviceInterfaceType.Name}\" because it has not been registered.", serviceInterfaceType.FullName);

            return context.GetServiceInstance(CreateServiceInstance, serviceInterfaceType);
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
                // As a quick check, look for constructor parameters that have not been registered (so that
                // we don't resolve/construct some parameters when it will eventually fail).
                var firstNonResolvableParameter = constructorParameters.FirstOrDefault(x => !IsRegistered(x.ParameterType));

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

        /// <summary>
        /// Indicates whether the "ServiceInterface" type has been previously registered.
        /// </summary>
        /// <typeparam name="ServiceInterface">The type to check whether it has been registered.</typeparam>
        /// <returns>A boolean value indicating whether the type has been registered.</returns>
        public bool IsRegistered<ServiceInterface>()
        {
            return IsRegistered(typeof(ServiceInterface));
        }

        private bool IsRegistered(Type type)
        {
            return registeredTypes.ContainsKey(type);
        }
    }
}
