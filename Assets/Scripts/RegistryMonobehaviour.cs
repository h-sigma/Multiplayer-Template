using System;
using System.Collections.Generic;
using UnityEngine;

public interface IRegisterable<out TEnum> where TEnum : Enum
{
    TEnum RegisterableType { get; }
}

[DefaultExecutionOrder(-100)]
public abstract class RegistryMonobehaviour<TDerived, TEnum> : MonoBehaviour, IRegisterable<TEnum>, IEquatable<TDerived>
    where TEnum : Enum where TDerived : RegistryMonobehaviour<TDerived, TEnum>
{
    [SerializeField]
    protected TEnum registerableType;

    public TEnum RegisterableType => registerableType;

    public bool Equals(TDerived other)
    {
        return other == this;
    }

    public virtual void OnEnable()
    {
        Registry<TEnum, TDerived>.Register(this as TDerived);
    }

    public virtual void OnDisable()
    {
        Registry<TEnum, TDerived>.Unregister(this as TDerived);
    }
}

public static class Registry<TEnum, TRegistered> where TEnum : Enum
    where TRegistered : IRegisterable<TEnum>, IEquatable<TRegistered>
{
    static Registry()
    {
        s_Registry = new Dictionary<TEnum, List<TRegistered>>();
        foreach (var value in Enum.GetValues(typeof(TEnum)))
        {
            s_Registry.Add((TEnum) value, new List<TRegistered>());
        }

        s_ToAdd    = new HashSet<TRegistered>();
        s_ToRemove = new HashSet<TRegistered>();
    }

    private static readonly Dictionary<TEnum, List<TRegistered>> s_Registry;

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
                if (s_Registry.TryGetValue(registerable.RegisterableType, out var list))
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
                if (s_Registry.TryGetValue(registerable.RegisterableType, out var list))
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

    public static bool GetAllRegistered(TEnum registerableType, out IReadOnlyList<TRegistered> list)
    {
        ApplyPending();

        if (s_Registry.TryGetValue(registerableType, out var result))
        {
            list = result;
            return true;
        }

        list = null;
        return false;
    }

    public static bool GetRandom(TEnum registerableType, out TRegistered registered)
    {
        ApplyPending();

        if (s_Registry.TryGetValue(registerableType, out var result) && result.Count > 0)
        {
            registered = result[UnityEngine.Random.Range(0, result.Count)];
            return true;
        }

        registered = default(TRegistered);
        return false;
    }

    public static bool GetRandomExcluding(TEnum registerableType, out TRegistered registered,
        TRegistered                             pointToExclude)
    {
        ApplyPending();

        if (s_Registry.TryGetValue(registerableType, out var result) && result.Count > 1)
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