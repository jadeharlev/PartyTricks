using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Services {
    public class SceneServiceLocator : MonoBehaviour, IServiceLocator {
        private static SceneServiceLocator instance;
        public static SceneServiceLocator Instance {
            get
            {
                if (instance == null) {
                    var gameObject = new GameObject("SceneServiceLocator");
                    instance = gameObject.AddComponent<SceneServiceLocator>();
                }
                return instance;
            }
        }

        private readonly Dictionary<Type, object> services = new();

        private void Awake() {
            if (instance != null && instance != this) {
                Destroy(gameObject);
                return;
            }

            instance = this;

            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnSceneUnloaded(Scene scene) {
            services.Clear();
        }

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

        private void OnDestroy() {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            if (instance == this) {
                instance = null;
            }
        }
    }
}