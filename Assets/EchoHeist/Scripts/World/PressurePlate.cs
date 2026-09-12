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
        [SerializeField, Min(0.01f)] private float transitionSpeed = 1.4f;
        [SerializeField] private Renderer plateRenderer;
        [SerializeField] private Color inactiveColor = new Color(0.16f, 0.7f, 0.28f, 1f);
        [SerializeField] private Color activeColor = new Color(0.35f, 1f, 0.55f, 1f);

        private readonly HashSet<PressurePlateActivator> _occupants = new HashSet<PressurePlateActivator>();
        private Vector3 _visualRestPosition;
        private MaterialPropertyBlock _propertyBlock;
        private Color _currentColor;

        public event Action<bool> ActivationChanged;
        public bool IsActivated { get; private set; }

        private void Awake()
        {
            if (plateVisual != null) _visualRestPosition = plateVisual.localPosition;
            if (plateRenderer == null && plateVisual != null) plateRenderer = plateVisual.GetComponentInChildren<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
            ApplyVisualImmediate(false);
        }

        private void Update()
        {
            if (plateVisual != null)
            {
                Vector3 target = _visualRestPosition + Vector3.down * (IsActivated ? pressedDepth : 0f);
                plateVisual.localPosition = Vector3.MoveTowards(
                    plateVisual.localPosition, target, transitionSpeed * Time.deltaTime);
            }

            Color targetColor = IsActivated ? activeColor : inactiveColor;
            _currentColor = Color.Lerp(_currentColor, targetColor, 1f - Mathf.Exp(-12f * Time.deltaTime));
            ApplyColor(_currentColor);
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

        public void ResetForRun()
        {
            ClearOccupants();
            ApplyVisualImmediate(false);
        }
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
                return;
            }

            IsActivated = isActivated;
            ActivationChanged?.Invoke(IsActivated);
        }

        private void ApplyVisualImmediate(bool active)
        {
            if (plateVisual != null) plateVisual.localPosition =
                _visualRestPosition + Vector3.down * (active ? pressedDepth : 0f);
            _currentColor = active ? activeColor : inactiveColor;
            ApplyColor(_currentColor);
        }

        private void ApplyColor(Color color)
        {
            if (plateRenderer == null) return;
            plateRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor("_BaseColor", color);
            _propertyBlock.SetColor("_Color", color);
            plateRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
