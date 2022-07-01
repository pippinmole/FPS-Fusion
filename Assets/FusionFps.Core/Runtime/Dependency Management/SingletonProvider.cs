using System;
using System.Collections.Generic;

namespace FusionFps.Core {
    public static class SingletonProvider {
        private static readonly Dictionary<Type, Func<object>> instanceAccessors = new();

        public static void Clear() {
            instanceAccessors.Clear();
        }

        public static void AddSingleton<T>(Func<T> accessorMethod, bool overwriteIfAccessorExists = true)
            where T : class {
            if ( accessorMethod == null ) return;
            if ( !overwriteIfAccessorExists && instanceAccessors.ContainsKey(typeof(T)) ) return;

            instanceAccessors[typeof(T)] = accessorMethod;
        }

        public static T Get<T>() where T : class {
            if ( instanceAccessors.TryGetValue(typeof(T), out Func<object> accessorMethod) ) {
                return accessorMethod?.Invoke() as T;
            }

            return default(T);
        }
    }
}