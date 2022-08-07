using System.Collections.ObjectModel;
using System.Reflection;

namespace GenshinPatchTools.Game.Tools.PathSearcher
{
    public static class GamePathSearcherManager
    {
        private static Dictionary<Type, IGamePathSearcher> _searchers;
        static GamePathSearcherManager()
        {
            _searchers = new Dictionary<Type, IGamePathSearcher>();
            Assembly asm = typeof(GamePathSearcherManager).Assembly;
            Type[] types = asm.GetTypes();
            Type gamePathSearcherInterfaceType = typeof(IGamePathSearcher);
            foreach (var type in types)
            {
                if (!gamePathSearcherInterfaceType.IsAssignableFrom(type) || type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                ConstructorInfo? constructorInfo = type.GetConstructor(Type.EmptyTypes);
                IGamePathSearcher? gamePathSearcher = (IGamePathSearcher?)constructorInfo?.Invoke(Array.Empty<object>());
                if (gamePathSearcher == null)
                {
                    continue;
                }
                
                _searchers.Add(type, gamePathSearcher);
            }
        }

        public static T? GetGamePathSearcher<T>() where T : IGamePathSearcher
        {
            return (T?)(_searchers.ContainsKey(typeof(T))
                ? _searchers[typeof(T)]
                : default);
        }
        

        public static bool Register(Type type, Type[] argumentsTypes, object[] arguments)
        {
            if (!typeof(IGamePathSearcher).IsAssignableFrom(type) || type.IsAbstract || type.IsInterface)
            {
                return false;
            }
            
            ConstructorInfo? constructorInfo = type.GetConstructor(Type.EmptyTypes);
            IGamePathSearcher? gamePathSearcher = (IGamePathSearcher?)constructorInfo?.Invoke(Array.Empty<object>());
            if (gamePathSearcher == null)
            {
                return false;
            }
                
            _searchers.Add(type, gamePathSearcher);
            return true;
        }

        public static ReadOnlyDictionary<Type, IGamePathSearcher> GetGamePathSearchers() =>
            new ReadOnlyDictionary<Type, IGamePathSearcher>(_searchers);
    }
}