using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;

            // НОВЕ: Перевіряємо, чи об'єкт знаходиться в корені (не має батька)
            // Тільки кореневі об'єкти можуть бути "DontDestroyOnLoad"
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // Якщо це UI або дитина іншого об'єкта, ми просто залишаємо його 
                // як Singleton у межах цієї сцени, не видаючи попереджень.
                Debug.Log($"[Singleton] {typeof(T).Name} є дитиною іншого об'єкта, DontDestroyOnLoad пропущено.");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}