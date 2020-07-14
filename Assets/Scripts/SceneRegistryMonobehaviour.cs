using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface ISceneRegisterable<out TEnum> where TEnum : Enum
{
    TEnum RegisterableType { get; }
    Scene sceneGetter      { get; } //awkward name because monobehaviour has existing property named Scene
}

[DefaultExecutionOrder(-100)]
public abstract class SceneRegistryMonobehaviour<TDerived, TEnum> : MonoBehaviour, ISceneRegisterable<TEnum>,
    IEquatable<TDerived>
    where TEnum : Enum where TDerived : SceneRegistryMonobehaviour<TDerived, TEnum>
{
    [SerializeField]
    protected TEnum registerableType;

    public TEnum RegisterableType => registerableType;

    public bool Equals(TDerived other)
    {
        return other == this;
    }

    public Scene sceneGetter => gameObject.scene;

    public virtual void OnEnable()
    {
        SceneRegistry<TEnum, TDerived>.Register(this as TDerived);
    }

    public virtual void OnDisable()
    {
        SceneRegistry<TEnum, TDerived>.Unregister(this as TDerived);
    }
}

public static class SceneRegistry<TEnum, TRegistered> where TEnum : Enum
    where TRegistered : ISceneRegisterable<TEnum>, IEquatable<TRegistered>
{
    public struct DictKey : IEquatable<DictKey>
    {
        public DictKey(TEnum _registerableType, Scene _scene)
        {
            scene            = _scene;
            registerableType = _registerableType;
        }

        public readonly TEnum registerableType;
        public readonly Scene scene;

        public bool Equals(DictKey other)
        {
            return other.registerableType.Equals(this.registerableType) && other.scene.Equals(this.scene);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is DictKey key)
            {
                return Equals(key);
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + registerableType.GetHashCode();
                hash = hash * 23 + scene.GetHashCode();
                return hash;
            }
        }
    }

    static SceneRegistry()
    {
        s_Registry = new Dictionary<DictKey, List<TRegistered>>();

        s_ToAdd    = new HashSet<TRegistered>();
        s_ToRemove = new HashSet<TRegistered>();
    }

    static bool TryGetListOrAdd(DictKey dictKey, out List<TRegistered> list)
    {
        if (s_Registry.TryGetValue(dictKey, out list))
        {
            return true;
        }
        else
        {
            s_Registry.Add(dictKey, new List<TRegistered>());
            if (s_Registry.TryGetValue(dictKey, out list))
            {
                return true;
            }

            list = default;
            return false;
        }
    }

    private static readonly Dictionary<DictKey, List<TRegistered>> s_Registry;

    private static readonly HashSet<TRegistered> s_ToAdd;
    private static readonly HashSet<TRegistered> s_ToRemove;

    public static void Register(TRegistered point)
    {
        s_ToAdd.Add(point);
    }

    public static void Unregister(TRegistered point)
    {
        s_ToRemove.Add(point);
    }

    private static void ApplyPending()
    {
        if (s_ToAdd.Count > 0)
        {
            foreach (var registerable in s_ToAdd)
            {
                var dictKey = new DictKey(registerable.RegisterableType, registerable.sceneGetter);
                if (TryGetListOrAdd(dictKey, out var list))
                {
                    var index = list.IndexOf(registerable);
                    if (index == -1)
                        list.Add(registerable);
                }
            }

            s_ToAdd.Clear();
        }

        if (s_ToRemove.Count > 0)
        {
            foreach (var registerable in s_ToRemove)
            {
                var dictKey = new DictKey(registerable.RegisterableType, registerable.sceneGetter);
                if (TryGetListOrAdd(dictKey, out var list))
                {
                    var index = list.IndexOf(registerable);
                    if (index != -1)
                    {
                        var temp          = list[index];
                        var countMinusOne = list.Count - 1;
                        list[index]         = list[countMinusOne];
                        list[countMinusOne] = temp;
                        list.RemoveAt(countMinusOne);
                    }
                }
            }

            s_ToRemove.Clear();
        }
    }

    public static bool GetAllRegistered(TEnum registerableType, Scene scene, out IReadOnlyList<TRegistered> list)
    {
        ApplyPending();
        
        var dictKey = new DictKey(registerableType, scene);

        var success = TryGetListOrAdd(dictKey, out var _list);
        list = _list;
        return success;
    }

    public static bool GetRandom(TEnum registerableType, Scene scene, out TRegistered registered)
    {
        ApplyPending();
        
        var dictKey = new DictKey(registerableType, scene);

        if (TryGetListOrAdd(dictKey, out var result) && result.Count > 0)
        {
            registered = result[UnityEngine.Random.Range(0, result.Count)];
            return true;
        }

        registered = default(TRegistered);
        return false;
    }

    public static bool GetRandomExcluding(TEnum registerableType, Scene scene, out TRegistered registered,
        TRegistered                             pointToExclude)
    {
        ApplyPending();
        
        var dictKey = new DictKey(registerableType, scene);

        if (TryGetListOrAdd(dictKey, out var result) && result.Count > 1)
        {
            registered = pointToExclude;
            while (registered.Equals(pointToExclude))
            {
                registered = result[UnityEngine.Random.Range(0, result.Count)];
            }

            return true;
        }

        registered = default(TRegistered);
        return false;
    }
}