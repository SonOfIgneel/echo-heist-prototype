using UnityEngine;

namespace EchoHeist
{
    public enum GuardTargetKind
    {
        Player,
        Echo,
        Decoy
    }

    public sealed class GuardTarget : MonoBehaviour
    {
        [SerializeField] private GuardTargetKind kind;

        private bool _detectable = true;

        public GuardTargetKind Kind => kind;
        public Vector3 AimPosition => transform.position + Vector3.up * 0.35f;
        public bool IsAvailable => _detectable && isActiveAndEnabled && gameObject.activeInHierarchy;

        public void SetDetectable(bool isDetectable) => _detectable = isDetectable;

        private void OnDisable() => _detectable = true;
    }
}
