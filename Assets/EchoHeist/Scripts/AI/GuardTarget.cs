using UnityEngine;

namespace EchoHeist
{
    public enum GuardTargetKind
    {
        Player,
        Echo
    }

    public sealed class GuardTarget : MonoBehaviour
    {
        [SerializeField] private GuardTargetKind kind;

        public GuardTargetKind Kind => kind;
        public Vector3 AimPosition => transform.position + Vector3.up * 0.35f;
        public bool IsAvailable => isActiveAndEnabled && gameObject.activeInHierarchy;
    }
}
