using System;
using System.Collections.Generic;

namespace FusionFps.Core {
    public static class ServiceProvider {
        private static readonly Dictionary<Type, Func<object>> InstanceAccessors = new();

        public static void Clear() {
            InstanceAccessors.Clear();
        }

        public static void AddSingleton<T>(Func<T> accessorMethod, bool overwriteIfAccessorExists = true)
            where T : class {
            if ( accessorMethod == null ) return;
            if ( !overwriteIfAccessorExists && InstanceAccessors.ContainsKey(typeof(T)) ) return;

            InstanceAccessors[typeof(T)] = accessorMethod;
        }

        public static T Get<T>() where T : class {
            if ( InstanceAccessors.TryGetValue(typeof(T), out Func<object> accessorMethod) ) {
                return accessorMethod?.Invoke() as T;
            }

            return default(T);
        }
    }
}