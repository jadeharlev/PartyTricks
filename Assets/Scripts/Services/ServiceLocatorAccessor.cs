namespace Services {
    public static class ServiceLocatorAccessor {
        private static IServiceLocator currentSceneLocator;
        public static void SetLocator(IServiceLocator locator) {
            currentSceneLocator = locator;
        }

        public static T GetService<T>() where T : class {
            var service = currentSceneLocator?.GetService<T>();
            return service ?? GlobalServiceLocator.Instance.GetService<T>();
        }

        public static void RegisterGlobal<T>(T service) where T : class {
            GlobalServiceLocator.Instance.RegisterService(service);
        }

        public static void Register<T>(T service) where T : class {
            SceneServiceLocator.Instance.RegisterService(service);
        }

        public static void UnregisterGlobal<T>() where T : class {
            GlobalServiceLocator.Instance.UnregisterService(typeof(T));
        }
        
        public static void Unregister<T>(T service) where T : class {
            if (currentSceneLocator != null) {
                SceneServiceLocator.Instance.UnregisterService(typeof(T));
            }
        }

        public static bool HasService<T>() where T : class {
            if (currentSceneLocator != null && currentSceneLocator.HasService<T>()) {
                return true;
            }
            return GlobalServiceLocator.Instance.HasService<T>();
        }
    }
}