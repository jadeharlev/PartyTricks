using System;
using System.Collections.Generic;
using UnityEngine;

namespace Services {
    public class GlobalServiceLocator : IServiceLocator {
        private static GlobalServiceLocator instance;
        public static GlobalServiceLocator Instance {
            get
            {
                if (instance == null) {
                    instance = new GlobalServiceLocator();
                }
                return instance;
            }
        }

        private readonly Dictionary<Type, object> services = new();

        private GlobalServiceLocator() { }

        public T GetService<T>() where T : class {
            var type = typeof(T);
            if (services.TryGetValue(type, out var service)) {
                return service as T;
            }
            DebugLogger.Log(LogChannel.Systems, "Service not found: " + type.Name);
            return null;
        }

        public void RegisterService<T>(T service) where T : class {
            var type = typeof(T);
            if (services.Remove(type)) {
                DebugLogger.Log(LogChannel.Systems, "Service was overwritten: " + type.Name, LogLevel.Warning);
            }
            services[type] = service;
            DebugLogger.Log(LogChannel.Systems, "Service registered: " + type.Name);
        }

        public void UnregisterService<T>(T service) where T : class {
            var type = typeof(T);
            services.Remove(type);
            DebugLogger.Log(LogChannel.Systems, "Service unregistered: " + type.Name);
        }

        public bool HasService<T>() where T : class {
            return services.ContainsKey(typeof(T));
        }

        public void Clear() {
            services.Clear();
        }
    }
}