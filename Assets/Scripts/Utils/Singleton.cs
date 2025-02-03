using System;

namespace TestApp.Utils
{
    /// <summary>
    /// Be aware this will not prevent a non singleton constructor
    ///   such as `T myT = new T();`
    /// To prevent that, add `protected/private T () {}` to your singleton class.
    /// </summary>
    public class Singleton<T> where T : class
    {
        private static T _instance;

        private static object _lock = new object();

        private static T Create()
        {
            lock (_lock)
            {
                if (_instance != null)
                    return _instance;

                _instance = Activator.CreateInstance(typeof(T), true) as T;
                return _instance;
            }
        }

        public static T Instance
        {
            get
            {
                if (_instance == null)
                    return Create();

                return _instance;
            }
        }
    }
}
