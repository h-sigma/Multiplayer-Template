using System;
using System.Collections.Generic;
using UnityEngine;

public static class RegistryExtensions
{
    public static IReadOnlyList<TRegistered> GetAllRegisteredInScene<TRegistered, TEnum>(this MonoBehaviour mb, TEnum registerableType) where TEnum : Enum where TRegistered : ISceneRegisterable<TEnum>, IEquatable<TRegistered>
    {
        SceneRegistry<TEnum, TRegistered>.GetAllRegistered(registerableType, mb.gameObject.scene, out var result);
        return result;
    }
    
    public static TRegistered GetRandomRegisteredInScene<TRegistered, TEnum>(this MonoBehaviour mb, TEnum registerableType) where TEnum : Enum where TRegistered : ISceneRegisterable<TEnum>, IEquatable<TRegistered>
    {
        SceneRegistry<TEnum, TRegistered>.GetRandom(registerableType, mb.gameObject.scene, out var result);
        return result;
    }
    
    public static TRegistered GetRandomRegisteredInSceneExcluding<TRegistered, TEnum>(this MonoBehaviour mb, TEnum registerableType, TRegistered excluded) where TEnum : Enum where TRegistered : ISceneRegisterable<TEnum>, IEquatable<TRegistered>
    {
        SceneRegistry<TEnum, TRegistered>.GetRandomExcluding(registerableType, mb.gameObject.scene, out var result, excluded);
        return result;
    }
}