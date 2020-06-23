using UnityEngine;

public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
{
    private static T _instance;
    public static T instance 
    {
        get
        {
            if (_instance == null)
            {
                var all = Resources.FindObjectsOfTypeAll<T>();
                if (all.Length > 0)
                    _instance = all[0];
            }
            else
            {
                return _instance;
            }

            if(_instance == null)
                _instance = CreateInstance<T>();
            return _instance;
        }
    }
   
}
