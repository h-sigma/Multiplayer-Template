using HarshCommon.Patterns.Registry;
using UnityEngine;

namespace Carrom
{
    public enum CodeElementsType
    {
        Board,
        Match,
        Gameplay,
    }
    
    [DefaultExecutionOrder(-150)]
    public abstract class CodeElements : SceneRegistryMonobehaviour<CodeElements, CodeElementsType>
    {
        
    }
}