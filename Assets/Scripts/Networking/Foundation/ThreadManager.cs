using System;
using System.Collections.Generic;
using UnityEngine;

namespace Networking.Foundation
{
    //todo -- replace with pump
    public class ThreadManager : MonoBehaviour
    {
        private static readonly List<Action> s_ExecuteOnMainThread         = new List<Action>();
        private static readonly List<Action> s_ExecuteCopiedOnMainThread   = new List<Action>();
        private static          bool         _actionToExecuteOnMainThread = false;

        private void Update()
        {
            UpdateMain();
        }

        /// <summary>Sets an action to be executed on the main thread.</summary>
        /// <param name="action">The action to be executed on the main thread.</param>
        public static void ExecuteOnMainThread(Action action)
        {
            if (action == null)
            {
                Debug.Log("No action to execute on main thread!");
                return;
            }

            lock (s_ExecuteOnMainThread)
            {
                s_ExecuteOnMainThread.Add(action);
                _actionToExecuteOnMainThread = true;
            }
        }

        /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
        public static void UpdateMain()
        {
            if (_actionToExecuteOnMainThread)
            {
                s_ExecuteCopiedOnMainThread.Clear();
                lock (s_ExecuteOnMainThread)
                {
                    s_ExecuteCopiedOnMainThread.AddRange(s_ExecuteOnMainThread);
                    s_ExecuteOnMainThread.Clear();
                    _actionToExecuteOnMainThread = false;
                }

                for (int i = 0; i < s_ExecuteCopiedOnMainThread.Count; i++)
                {
                    s_ExecuteCopiedOnMainThread[i]();
                }
            }
        }
    }
}