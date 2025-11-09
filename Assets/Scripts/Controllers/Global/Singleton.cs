using UnityEngine;

namespace KOJC
{
    public class MonoSingleton<T> : AbstractController where T : MonoSingleton<T>, new()
    {
        private static T _instance;

        public static T Inst
        {
            get
            {
                if (_instance == null)
                {
                    // 如果实例不存在，则查找场景中已有的SingletonClass组件并将其设置为_instance
                    _instance = FindObjectOfType<T>();

                    // 如果没有找到实例，创建一个新的GameObject挂载SingletonClass脚本并设置为_instance
                    if (_instance == null)
                    {
                        var singletonObject = new GameObject();
                        _instance            = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).Name;
                        DontDestroyOnLoad(singletonObject); // 保持单例在整个应用生命周期内不被销毁
                    }
                }

                return _instance;
            }
        }

        protected MonoSingleton()
        {
        }

        protected virtual void Awake()
        {
            // 如果_instance尚未初始化，并且当前对象不是_instance（即第一次Awake调用）
            if (_instance == null && this != _instance)
            {
                // 设置当前对象为_instance
                _instance = this as T;

                // 防止切换场景时该单例对象被销毁
                DontDestroyOnLoad(gameObject);
            }
            else if (this != _instance) // 若_instance已存在但不是当前对象，则销毁多余的实例
            {
                Destroy(gameObject);
            }
        }
        protected void Log(object log) => Debug.Log($"[{GetType().Name}]:" + log);
        protected void LogWarning(object log) => Debug.LogWarning($"[{GetType().Name}]:" + log);
        protected void LogError(object log) => Debug.LogError($"[{GetType().Name}]:" + log);
    }
}