using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoHeist
{
    [RequireComponent(typeof(Collider))]
    public sealed class PressurePlate : MonoBehaviour, IRunResettable
    {
        [SerializeField] private Transform plateVisual;
        [SerializeField, Min(0f)] private float pressedDepth = 0.12f;

        private readonly HashSet<PressurePlateActivator> _occupants = new HashSet<PressurePlateActivator>();
        private Vector3 _visualRestPosition;

        public event Action<bool> ActivationChanged;
        public bool IsActivated { get; private set; }

        private void Awake()
        {
            if (plateVisual != null) _visualRestPosition = plateVisual.localPosition;
        }

        private void OnTriggerEnter(Collider other)
        {
            PressurePlateActivator activator = other.GetComponentInParent<PressurePlateActivator>();
            if (activator != null && _occupants.Add(activator))
            {
                activator.RegisterPlate(this);
                RefreshState();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            PressurePlateActivator activator = other.GetComponentInParent<PressurePlateActivator>();
            if (activator != null) Unregister(activator);
        }

        internal void Unregister(PressurePlateActivator activator)
        {
            if (activator != null) activator.ForgetPlate(this);
            if (_occupants.Remove(activator)) RefreshState();
        }

        public void ResetForRun() => ClearOccupants();
        private void OnDisable() => ClearOccupants();

        private void ClearOccupants()
        {
            if (_occupants.Count > 0)
            {
                var occupants = new PressurePlateActivator[_occupants.Count];
                _occupants.CopyTo(occupants);
                foreach (PressurePlateActivator occupant in occupants)
                {
                    if (occupant != null) occupant.ForgetPlate(this);
                }
                _occupants.Clear();
            }

            SetActivated(false);
        }

        private void RefreshState()
        {
            _occupants.RemoveWhere(occupant => occupant == null || !occupant.isActiveAndEnabled);
            SetActivated(_occupants.Count > 0);
        }

        private void SetActivated(bool isActivated)
        {
            if (IsActivated == isActivated)
            {
                UpdateVisual();
                return;
            }

            IsActivated = isActivated;
            UpdateVisual();
            ActivationChanged?.Invoke(IsActivated);
        }

        private void UpdateVisual()
        {
            if (plateVisual != null)
            {
                plateVisual.localPosition = _visualRestPosition + Vector3.down * (IsActivated ? pressedDepth : 0f);
            }
        }
    }
}
