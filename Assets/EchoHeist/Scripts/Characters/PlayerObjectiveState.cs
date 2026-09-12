using System;
using UnityEngine;

namespace EchoHeist
{
    public sealed class PlayerObjectiveState : MonoBehaviour, IRunResettable
    {
        public event Action<bool> DataCoreStateChanged;
        public bool HasDataCore { get; private set; }

        public bool CollectDataCore()
        {
            if (HasDataCore) return false;
            HasDataCore = true;
            DataCoreStateChanged?.Invoke(true);
            return true;
        }

        public void ResetForRun()
        {
            HasDataCore = false;
            DataCoreStateChanged?.Invoke(false);
        }
    }
}
