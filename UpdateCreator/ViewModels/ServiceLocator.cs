/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UpdateCreator.ViewModels
{
    /// <summary>
    /// When used with the ServiceLocator container, specifies which constructor
    ///             should be used to instantiate when GetInstance is called.
    ///             If there is only one constructor in the class, this attribute is
    ///             not needed.
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public sealed class PreferredConstructorAttribute : Attribute
    {
    }

    public class ServiceLocator
    {

        private readonly Dictionary<Type, ConstructorInfo> _constructorInfos
            = new Dictionary<Type, ConstructorInfo>();

        private readonly string _defaultKey = Guid.NewGuid().ToString();

        private readonly object[] _emptyArguments = new object[0];

        private readonly Dictionary<Type, Dictionary<string, Delegate>> _factories
            = new Dictionary<Type, Dictionary<string, Delegate>>();

        private readonly Dictionary<Type, Dictionary<string, object>> _instancesRegistry
            = new Dictionary<Type, Dictionary<string, object>>();

        private readonly Dictionary<Type, Type> _interfaceToClassMap
            = new Dictionary<Type, Type>();

        private readonly object _syncLock = new object();

        private static ServiceLocator _default;

        /// <summary>
        /// This class' default instance.
        /// </summary>
        public static ServiceLocator Default
        {
            get
            {
                return _default ?? (_default = new ServiceLocator());
            }
        }

        public static TService GetDefaultInstance<TService>()
        {
            return Default.GetInstance<TService>();
        }

        

        /// <summary>
        /// Checks whether at least one instance of a given class is already created in the container.
        /// </summary>
        /// <typeparam name="TClass">The class that is queried.</typeparam>
        /// <returns>True if at least on instance of the class is already created, false otherwise.</returns>
        public bool ContainsCreated<TClass>()
        {
            return this.ContainsCreated<TClass>(null);
        }

        /// <summary>
        /// Checks whether the instance with the given key is already created for a given class
        /// in the container.
        /// </summary>
        /// <typeparam name="TClass">The class that is queried.</typeparam>
        /// <param name="key">The key that is queried.</param>
        /// <returns>True if the instance with the given key is already registered for the given class,
        /// false otherwise.</returns>
        public bool ContainsCreated<TClass>(string key)
        {
            var classType = typeof(TClass);

            if (!this._instancesRegistry.ContainsKey(classType))
            {
                return false;
            }

            if (string.IsNullOrEmpty(key))
            {
                return this._instancesRegistry[classType].Count > 0;
            }

            return this._instancesRegistry[classType].ContainsKey(key);
        }

        /// <summary>
        /// Gets a value indicating whether a given type T is already registered.
        /// </summary>
        /// <typeparam name="T">The type that the method checks for.</typeparam>
        /// <returns>True if the type is registered, false otherwise.</returns>
        public bool IsRegistered<T>()
        {
            var classType = typeof(T);
            return this._interfaceToClassMap.ContainsKey(classType);
        }

        /// <summary>
        /// Gets a value indicating whether a given type T and a give key
        /// are already registered.
        /// </summary>
        /// <typeparam name="T">The type that the method checks for.</typeparam>
        /// <param name="key">The key that the method checks for.</param>
        /// <returns>True if the type and key are registered, false otherwise.</returns>
        public bool IsRegistered<T>(string key)
        {
            var classType = typeof(T);

            if (!this._interfaceToClassMap.ContainsKey(classType)
                || !this._factories.ContainsKey(classType))
            {
                return false;
            }

            return this._factories[classType].ContainsKey(key);
        }

        /// <summary>
        /// Registers a given type for a given interface.
        /// </summary>
        /// <typeparam name="TInterface">The interface for which instances will be resolved.</typeparam>
        /// <typeparam name="TClass">The type that must be used to create instances.</typeparam>
        public void Register<TInterface, TClass>()
            where TClass : class
            where TInterface : class
        {
            this.Register<TInterface, TClass>(false);
        }

        /// <summary>
        /// Registers a given type for a given interface with the possibility for immediate
        /// creation of the instance.
        /// </summary>
        /// <typeparam name="TInterface">The interface for which instances will be resolved.</typeparam>
        /// <typeparam name="TClass">The type that must be used to create instances.</typeparam>
        /// <param name="createInstanceImmediately">If true, forces the creation of the default
        /// instance of the provided class.</param>
        public void Register<TInterface, TClass>(bool createInstanceImmediately)
            where TClass : class
            where TInterface : class
        {
            lock (this._syncLock)
            {
                var interfaceType = typeof(TInterface);
                var classType = typeof(TClass);

                if (this._interfaceToClassMap.ContainsKey(interfaceType))
                {
                    if (this._interfaceToClassMap[interfaceType] != classType)
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                "There is already a class registered for {0}.",
                                interfaceType.FullName));
                    }
                }
                else
                {
                    this._interfaceToClassMap.Add(interfaceType, classType);
                    this._constructorInfos.Add(classType, this.GetConstructorInfo(classType));
                }

                Func<TInterface> factory = this.MakeInstance<TInterface>;
                this.DoRegister(interfaceType, factory, this._defaultKey);

                if (createInstanceImmediately)
                {
                    this.GetInstance<TInterface>();
                }
            }
        }

        /// <summary>
        /// Registers a given type.
        /// </summary>
        /// <typeparam name="TClass">The type that must be used to create instances.</typeparam>
        public void Register<TClass>()
            where TClass : class
        {
            this.Register<TClass>(false);
        }

        /// <summary>
        /// Registers a given type with the possibility for immediate
        /// creation of the instance.
        /// </summary>
        /// <typeparam name="TClass">The type that must be used to create instances.</typeparam>
        /// <param name="createInstanceImmediately">If true, forces the creation of the default
        /// instance of the provided class.</param>
        public void Register<TClass>(bool createInstanceImmediately)
            where TClass : class
        {
            var classType = typeof(TClass);
#if NETFX_CORE
            if (classType.GetTypeInfo().IsInterface)
#else
            if (classType.IsInterface)
#endif
            {
                throw new ArgumentException("An interface cannot be registered alone.");
            }

            lock (this._syncLock)
            {
                if (this._factories.ContainsKey(classType)
                    && this._factories[classType].ContainsKey(this._defaultKey))
                {
                    if (!this._constructorInfos.ContainsKey(classType))
                    {
                        // Throw only if constructorinfos have not been
                        // registered, which means there is a default factory
                        // for this class.
                        throw new InvalidOperationException(
                            string.Format("Class {0} is already registered.", classType));
                    }

                    return;
                }

                if (!this._interfaceToClassMap.ContainsKey(classType))
                {
                    this._interfaceToClassMap.Add(classType, null);
                }

                this._constructorInfos.Add(classType, this.GetConstructorInfo(classType));
                Func<TClass> factory = this.MakeInstance<TClass>;
                this.DoRegister(classType, factory, this._defaultKey);

                if (createInstanceImmediately)
                {
                    this.GetInstance<TClass>();
                }
            }
        }

        /// <summary>
        /// Registers a given instance for a given type.
        /// </summary>
        /// <typeparam name="TClass">The type that is being registered.</typeparam>
        /// <param name="factory">The factory method able to create the instance that
        /// must be returned when the given type is resolved.</param>
        public void Register<TClass>(Func<TClass> factory)
            where TClass : class
        {
            this.Register(factory, false);
        }

        /// <summary>
        /// Registers a given instance for a given type with the possibility for immediate
        /// creation of the instance.
        /// </summary>
        /// <typeparam name="TClass">The type that is being registered.</typeparam>
        /// <param name="factory">The factory method able to create the instance that
        /// must be returned when the given type is resolved.</param>
        /// <param name="createInstanceImmediately">If true, forces the creation of the default
        /// instance of the provided class.</param>
        public void Register<TClass>(Func<TClass> factory, bool createInstanceImmediately)
            where TClass : class
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            lock (this._syncLock)
            {
                var classType = typeof(TClass);

                if (this._factories.ContainsKey(classType)
                    && this._factories[classType].ContainsKey(this._defaultKey))
                {
                    throw new InvalidOperationException(
                        string.Format("There is already a factory registered for {0}.", classType.FullName));
                }

                if (!this._interfaceToClassMap.ContainsKey(classType))
                {
                    this._interfaceToClassMap.Add(classType, null);
                }

                this.DoRegister(classType, factory, this._defaultKey);

                if (createInstanceImmediately)
                {
                    this.GetInstance<TClass>();
                }
            }
        }

        /// <summary>
        /// Registers a given instance for a given type and a given key.
        /// </summary>
        /// <typeparam name="TClass">The type that is being registered.</typeparam>
        /// <param name="factory">The factory method able to create the instance that
        /// must be returned when the given type is resolved.</param>
        /// <param name="key">The key for which the given instance is registered.</param>
        public void Register<TClass>(Func<TClass> factory, string key)
            where TClass : class
        {
            this.Register(factory, key, false);
        }

        /// <summary>
        /// Registers a given instance for a given type and a given key with the possibility for immediate
        /// creation of the instance.
        /// </summary>
        /// <typeparam name="TClass">The type that is being registered.</typeparam>
        /// <param name="factory">The factory method able to create the instance that
        /// must be returned when the given type is resolved.</param>
        /// <param name="key">The key for which the given instance is registered.</param>
        /// <param name="createInstanceImmediately">If true, forces the creation of the default
        /// instance of the provided class.</param>
        public void Register<TClass>(
            Func<TClass> factory,
            string key,
            bool createInstanceImmediately)
            where TClass : class
        {
            lock (this._syncLock)
            {
                var classType = typeof(TClass);

                if (this._factories.ContainsKey(classType)
                    && this._factories[classType].ContainsKey(key))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "There is already a factory registered for {0} with key {1}.",
                            classType.FullName,
                            key));
                }

                if (!this._interfaceToClassMap.ContainsKey(classType))
                {
                    this._interfaceToClassMap.Add(classType, null);
                }

                this.DoRegister(classType, factory, key);

                if (createInstanceImmediately)
                {
                    this.GetInstance<TClass>(key);
                }
            }
        }

        /// <summary>
        /// Resets the instance in its original states. This deletes all the
        /// registrations.
        /// </summary>
        public void Reset()
        {
            this._interfaceToClassMap.Clear();
            this._instancesRegistry.Clear();
            this._constructorInfos.Clear();
            this._factories.Clear();
        }

        /// <summary>
        /// Unregisters a class from the cache and removes all the previously
        /// created instances.
        /// </summary>
        /// <typeparam name="TClass">The class that must be removed.</typeparam>
        public void Unregister<TClass>()
            where TClass : class
        {
            lock (this._syncLock)
            {
                var serviceType = typeof(TClass);
                Type resolveTo;

                if (this._interfaceToClassMap.ContainsKey(serviceType))
                {
                    resolveTo = this._interfaceToClassMap[serviceType] ?? serviceType;
                }
                else
                {
                    resolveTo = serviceType;
                }

                if (this._instancesRegistry.ContainsKey(serviceType))
                {
                    this._instancesRegistry.Remove(serviceType);
                }

                if (this._interfaceToClassMap.ContainsKey(serviceType))
                {
                    this._interfaceToClassMap.Remove(serviceType);
                }

                if (this._factories.ContainsKey(serviceType))
                {
                    this._factories.Remove(serviceType);
                }

                if (this._constructorInfos.ContainsKey(resolveTo))
                {
                    this._constructorInfos.Remove(resolveTo);
                }
            }
        }

        /// <summary>
        /// Removes the given instance from the cache. The class itself remains
        /// registered and can be used to create other instances.
        /// </summary>
        /// <typeparam name="TClass">The type of the instance to be removed.</typeparam>
        /// <param name="instance">The instance that must be removed.</param>
        public void Unregister<TClass>(TClass instance)
            where TClass : class
        {
            lock (this._syncLock)
            {
                var classType = typeof(TClass);

                if (this._instancesRegistry.ContainsKey(classType))
                {
                    var list = this._instancesRegistry[classType];

                    var pairs = list.Where(pair => pair.Value == instance).ToList();
                    for (var index = 0; index < pairs.Count(); index++)
                    {
                        var key = pairs[index].Key;

                        list.Remove(key);

                        if (this._factories.ContainsKey(classType))
                        {
                            if (this._factories[classType].ContainsKey(key))
                            {
                                this._factories[classType].Remove(key);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes the instance corresponding to the given key from the cache. The class itself remains
        /// registered and can be used to create other instances.
        /// </summary>
        /// <typeparam name="TClass">The type of the instance to be removed.</typeparam>
        /// <param name="key">The key corresponding to the instance that must be removed.</param>
        public void Unregister<TClass>(string key)
            where TClass : class
        {
            lock (this._syncLock)
            {
                var classType = typeof(TClass);

                if (this._instancesRegistry.ContainsKey(classType))
                {
                    var list = this._instancesRegistry[classType];

                    var pairs = list.Where(pair => pair.Key == key).ToList();
                    for (var index = 0; index < pairs.Count(); index++)
                    {
                        list.Remove(pairs[index].Key);
                    }
                }

                if (this._factories.ContainsKey(classType))
                {
                    if (this._factories[classType].ContainsKey(key))
                    {
                        this._factories[classType].Remove(key);
                    }
                }
            }
        }

        private object DoGetService(Type serviceType, string key)
        {
            lock (this._syncLock)
            {
                if (string.IsNullOrEmpty(key))
                {
                    key = this._defaultKey;
                }

                Dictionary<string, object> instances;

                if (!this._instancesRegistry.ContainsKey(serviceType))
                {
                    if (!this._interfaceToClassMap.ContainsKey(serviceType))
                    {
                        throw new Exception(
                            string.Format(
                                "Type not found in cache: {0}.",
                                serviceType.FullName));
                    }

                    instances = new Dictionary<string, object>();
                    this._instancesRegistry.Add(serviceType, instances);
                }
                else
                {
                    instances = this._instancesRegistry[serviceType];
                }

                if (instances.ContainsKey(key))
                {
                    return instances[key];
                }

                object instance = null;

                if (this._factories.ContainsKey(serviceType))
                {
                    if (this._factories[serviceType].ContainsKey(key))
                    {
                        instance = this._factories[serviceType][key].DynamicInvoke(null);
                    }
                    else
                    {
                        if (this._factories[serviceType].ContainsKey(this._defaultKey))
                        {
                            instance = this._factories[serviceType][this._defaultKey].DynamicInvoke(null);
                        }
                        else
                        {
                            throw new Exception(
                                string.Format(
                                    "Type not found in cache without a key: {0}", 
                                    serviceType.FullName));
                        }
                    }
                }

                instances.Add(key, instance);
                return instance;
            }
        }

        private void DoRegister<TClass>(Type classType, Func<TClass> factory, string key)
        {
            if (this._factories.ContainsKey(classType))
            {
                if (this._factories[classType].ContainsKey(key))
                {
                    // The class is already registered, ignore and continue.
                    return;
                }

                this._factories[classType].Add(key, factory);
            }
            else
            {
                var list = new Dictionary<string, Delegate>
                {
                    {
                        key,
                        factory
                    }
                };

                this._factories.Add(classType, list);
            }
        }

        private ConstructorInfo GetConstructorInfo(Type serviceType)
        {
            Type resolveTo;

            if (this._interfaceToClassMap.ContainsKey(serviceType))
            {
                resolveTo = this._interfaceToClassMap[serviceType] ?? serviceType;
            }
            else
            {
                resolveTo = serviceType;
            }

#if NETFX_CORE
            var constructorInfos = resolveTo.GetTypeInfo().DeclaredConstructors.Where(c => c.IsPublic).ToArray();
#else
            var constructorInfos = resolveTo.GetConstructors();
#endif

            if (constructorInfos.Length > 1)
            {
                if (constructorInfos.Length > 2)
                {
                    return this.GetPreferredConstructorInfo(constructorInfos, resolveTo);
                }
                
                if (constructorInfos.FirstOrDefault(i => i.Name == ".cctor") == null)
                {
                    return this.GetPreferredConstructorInfo(constructorInfos, resolveTo);
                }

                var first = constructorInfos.FirstOrDefault(i => i.Name != ".cctor");

                if (first == null
                    || !first.IsPublic)
                {
                    throw new Exception(
                        string.Format("Cannot register: No public constructor found in {0}.", resolveTo.Name));
                }

                return first;
            }

            if (constructorInfos.Length == 0
                || (constructorInfos.Length == 1
                    && !constructorInfos[0].IsPublic))
            {
                throw new Exception(
                    string.Format("Cannot register: No public constructor found in {0}.", resolveTo.Name));
            }

            return constructorInfos[0];
        }

        private ConstructorInfo GetPreferredConstructorInfo(IEnumerable<ConstructorInfo> constructorInfos, Type resolveTo)
        {
            var preferredConstructorInfo
                = (from t in constructorInfos
#if NETFX_CORE
                    let attribute = t.GetCustomAttribute(typeof(PreferredConstructorAttribute))
#else
                    let attribute = Attribute.GetCustomAttribute(t, typeof(PreferredConstructorAttribute))
#endif
                    where attribute != null
                    select t).FirstOrDefault();

            if (preferredConstructorInfo == null)
            {
                throw new Exception(
                    string.Format(
                        "Cannot register: Multiple constructors found in {0} but none marked with PreferredConstructor.",
                        resolveTo.Name));
            }

            return preferredConstructorInfo;
        }

        private TClass MakeInstance<TClass>()
        {
            var serviceType = typeof(TClass);

            var constructor = this._constructorInfos.ContainsKey(serviceType)
                                  ? this._constructorInfos[serviceType]
                                  : this.GetConstructorInfo(serviceType);

            var parameterInfos = constructor.GetParameters();

            if (parameterInfos.Length == 0)
            {
                return (TClass)constructor.Invoke(this._emptyArguments);
            }

            var parameters = new object[parameterInfos.Length];

            foreach (var parameterInfo in parameterInfos)
            {
                parameters[parameterInfo.Position] = this.GetService(parameterInfo.ParameterType);
            }

            return (TClass)constructor.Invoke(parameters);
        }

        /// <summary>
        /// Provides a way to get all the created instances of a given type available in the
        /// cache. Registering a class or a factory does not automatically
        /// create the corresponding instance! To create an instance, either register
        /// the class or the factory with createInstanceImmediately set to true,
        /// or call the GetInstance method before calling GetAllCreatedInstances.
        /// Alternatively, use the GetAllInstances method, which auto-creates default
        /// instances for all registered classes.
        /// </summary>
        /// <param name="serviceType">The class of which all instances
        /// must be returned.</param>
        /// <returns>All the already created instances of the given type.</returns>
        public IEnumerable<object> GetAllCreatedInstances(Type serviceType)
        {
            if (this._instancesRegistry.ContainsKey(serviceType))
            {
                return this._instancesRegistry[serviceType].Values;
            }

            return new List<object>();
        }

        /// <summary>
        /// Provides a way to get all the created instances of a given type available in the
        /// cache. Registering a class or a factory does not automatically
        /// create the corresponding instance! To create an instance, either register
        /// the class or the factory with createInstanceImmediately set to true,
        /// or call the GetInstance method before calling GetAllCreatedInstances.
        /// Alternatively, use the GetAllInstances method, which auto-creates default
        /// instances for all registered classes.
        /// </summary>
        /// <typeparam name="TService">The class of which all instances
        /// must be returned.</typeparam>
        /// <returns>All the already created instances of the given type.</returns>
        public IEnumerable<TService> GetAllCreatedInstances<TService>()
        {
            var serviceType = typeof (TService);
            return this.GetAllCreatedInstances(serviceType)
                .Select(instance => (TService)instance);
        }

        #region Implementation of IServiceProvider

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <returns>
        /// A service object of type <paramref name="serviceType" />.
        /// -or- 
        /// null if there is no service object of type <paramref name="serviceType" />.
        /// </returns>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        public object GetService(Type serviceType)
        {
            return this.DoGetService(serviceType, this._defaultKey);
        }

        #endregion

        #region Implementation of IServiceLocator

        /// <summary>
        /// Provides a way to get all the created instances of a given type available in the
        /// cache. Calling this method auto-creates default
        /// instances for all registered classes.
        /// </summary>
        /// <param name="serviceType">The class of which all instances
        /// must be returned.</param>
        /// <returns>All the instances of the given type.</returns>
        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            lock (this._factories)
            {
                if (this._factories.ContainsKey(serviceType))
                {
                    foreach (var factory in this._factories[serviceType])
                    {
                        this.GetInstance(serviceType, factory.Key);
                    }
                }
            }

            if (this._instancesRegistry.ContainsKey(serviceType))
            {
                return this._instancesRegistry[serviceType].Values;
            }


            return new List<object>();
        }

        /// <summary>
        /// Provides a way to get all the created instances of a given type available in the
        /// cache. Calling this method auto-creates default
        /// instances for all registered classes.
        /// </summary>
        /// <typeparam name="TService">The class of which all instances
        /// must be returned.</typeparam>
        /// <returns>All the instances of the given type.</returns>
        public IEnumerable<TService> GetAllInstances<TService>()
        {
            var serviceType = typeof(TService);
            return this.GetAllInstances(serviceType)
                .Select(instance => (TService)instance);
        }

        /// <summary>
        /// Provides a way to get an instance of a given type. If no instance had been instantiated 
        /// before, a new instance will be created. If an instance had already
        /// been created, that same instance will be returned.
        /// <remarks>
        /// If the class has not been registered before, this method
        /// returns null!
        /// </remarks>
        /// </summary>
        /// <param name="serviceType">The class of which an instance
        /// must be returned.</param>
        /// <returns>An instance of the given type.</returns>
        public object GetInstance(Type serviceType)
        {
            return this.DoGetService(serviceType, this._defaultKey);
        }

        /// <summary>
        /// Provides a way to get an instance of a given type corresponding
        /// to a given key. If no instance had been instantiated with this
        /// key before, a new instance will be created. If an instance had already
        /// been created with the same key, that same instance will be returned.
        /// <remarks>
        /// If the class has not been registered before, this method
        /// returns null!
        /// </remarks>
        /// </summary>
        /// <param name="serviceType">The class of which an instance must be returned.</param>
        /// <param name="key">The key uniquely identifying this instance.</param>
        /// <returns>An instance corresponding to the given type and key.</returns>
        public object GetInstance(Type serviceType, string key)
        {
            return this.DoGetService(serviceType, key);
        }

        /// <summary>
        /// Provides a way to get an instance of a given type. If no instance had been instantiated 
        /// before, a new instance will be created. If an instance had already
        /// been created, that same instance will be returned.
        /// <remarks>
        /// If the class has not been registered before, this method
        /// returns null!
        /// </remarks>
        /// </summary>
        /// <typeparam name="TService">The class of which an instance
        /// must be returned.</typeparam>
        /// <returns>An instance of the given type.</returns>
        public TService GetInstance<TService>()
        {
            return (TService)this.DoGetService(typeof(TService), this._defaultKey);
        }

        /// <summary>
        /// Provides a way to get an instance of a given type corresponding
        /// to a given key. If no instance had been instantiated with this
        /// key before, a new instance will be created. If an instance had already
        /// been created with the same key, that same instance will be returned.
        /// <remarks>
        /// If the class has not been registered before, this method
        /// returns null!
        /// </remarks>
        /// </summary>
        /// <typeparam name="TService">The class of which an instance must be returned.</typeparam>
        /// <param name="key">The key uniquely identifying this instance.</param>
        /// <returns>An instance corresponding to the given type and key.</returns>
        public TService GetInstance<TService>(string key)
        {
            return (TService)this.DoGetService(typeof(TService), key);
        }

        #endregion
    }
}
*/

/*
    ////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UpdateCreator.ViewModels
{
    public class ServiceLocator
    {
        private static ServiceLocator _default;
        public static ServiceLocator Default
        {
            get
            {
                return _default ?? (_default = new ServiceLocator());
            }
        }

        private static Dictionary<Type, ServiceInfo> services = new Dictionary<Type, ServiceInfo>();


        /// <summary>
        /// Registers a service.
        /// </summary>
        public static void Register<TInterface, TImplemention>() where TImplemention : TInterface
        {
            Register<TInterface, TImplemention>(false);
        }


        /// <summary>
        /// Registers a service as a singleton.
        /// </summary>
        public static void RegisterSingleton<TInterface, TImplemention>() where TImplemention : TInterface
        {
            Register<TInterface, TImplemention>(true);
        }


        /// <summary>
        /// Resolves a service.
        /// </summary>
        public static TInterface Resolve<TInterface>()
        {
            return (TInterface)services[typeof(TInterface)].ServiceImplementation;
        }


        /// <summary>
        /// Registers a service.
        /// </summary>
        /// <param name="isSingleton">true if service is Singleton; otherwise false.</param>
        private static void Register<TInterface, TImplemention>(bool isSingleton) where TImplemention : TInterface
        {
            services.Add(typeof(TInterface), new ServiceInfo(typeof(TImplemention), isSingleton));
        }

        /// <summary>
        /// Registers a given type.
        /// </summary>
        /// <typeparam name="TClass">The type that must be used to create instances.</typeparam>
        public void Register<TClass>()
            where TClass : class
        {
            this.Register<TClass>(false);
        }




        /// <summary>
        /// Class holding service information.
        /// </summary>
        class ServiceInfo
        {
            private Type serviceImplementationType;
            private object serviceImplementation;
            private bool isSingleton;


            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceInfo"/> class.
            /// </summary>
            /// <param name="serviceImplementationType">Type of the service implementation.</param>
            /// <param name="isSingleton">Whether the service is a Singleton.</param>
            public ServiceInfo(Type serviceImplementationType, bool isSingleton)
            {
                this.serviceImplementationType = serviceImplementationType;
                this.isSingleton = isSingleton;
            }


            /// <summary>
            /// Gets the service implementation.
            /// </summary>
            public object ServiceImplementation
            {
                get
                {
                    if (isSingleton)
                    {
                        if (serviceImplementation == null)
                        {
                            serviceImplementation = CreateInstance(serviceImplementationType);
                        }

                        return serviceImplementation;
                    }
                    else
                    {
                        return CreateInstance(serviceImplementationType);
                    }
                }
            }


            /// <summary>
            /// Creates an instance of a specific type.
            /// </summary>
            /// <param name="type">The type of the instance to create.</param>
            private static object CreateInstance(Type type)
            {
                if (services.ContainsKey(type))
                {
                    return services[type].ServiceImplementation;
                }

                ConstructorInfo ctor = type.GetConstructors().First();

                var parameters =
                    from parameter in ctor.GetParameters()
                    select CreateInstance(parameter.ParameterType);

                return Activator.CreateInstance(type, parameters.ToArray());
            }
        }
    }
}

*/