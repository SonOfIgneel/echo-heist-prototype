using UnityEngine;

namespace EchoHeist
{
    public sealed class GuardVision : MonoBehaviour
    {
        [SerializeField] private Transform eye;
        [SerializeField] private MeshRenderer visionIndicator;
        [SerializeField, Min(0.1f)] private float visionDistance = 7f;
        [SerializeField, Range(1f, 179f)] private float visionAngle = 70f;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private LayerMask obstructionMask;
        [SerializeField] private Color patrolColor = new Color(0.55f, 0.08f, 0.08f, 0.22f);
        [SerializeField] private Color suspiciousColor = new Color(1f, 0.55f, 0.05f, 0.34f);
        [SerializeField] private Color chaseColor = new Color(1f, 0.05f, 0.03f, 0.48f);

        private readonly Collider[] _overlaps = new Collider[16];
        private readonly GuardTarget[] _candidates = new GuardTarget[8];
        private MaterialPropertyBlock _propertyBlock;

        public float VisionDistance => visionDistance;
        public float VisionAngle => visionAngle;

        private void Awake()
        {
            if (eye == null) eye = transform;
            _propertyBlock = new MaterialPropertyBlock();
            SetIndicatorState(GuardState.Patrol);
        }

        public bool TryGetVisibleTarget(GuardTarget preferredTarget, GuardTarget ignoredTargetA,
            GuardTarget ignoredTargetB, GuardTarget ignoredTargetC, out GuardTarget visibleTarget)
        {
            if (preferredTarget != ignoredTargetA && preferredTarget != ignoredTargetB &&
                preferredTarget != ignoredTargetC && CanSee(preferredTarget))
            {
                visibleTarget = preferredTarget;
                return true;
            }

            visibleTarget = null;
            int candidateCount = 0;
            int overlapCount = Physics.OverlapSphereNonAlloc(
                eye.position, visionDistance, _overlaps, targetMask, QueryTriggerInteraction.Collide);

            float closestDistance = float.PositiveInfinity;
            for (int i = 0; i < overlapCount; i++)
            {
                GuardTarget candidate = _overlaps[i].GetComponentInParent<GuardTarget>();
                if (candidate == null || candidate == ignoredTargetA || candidate == ignoredTargetB ||
                    candidate == ignoredTargetC || !candidate.IsAvailable ||
                    ContainsCandidate(candidate, candidateCount)) continue;
                if (candidateCount < _candidates.Length) _candidates[candidateCount++] = candidate;
                if (!CanSee(candidate)) continue;

                float distance = (candidate.AimPosition - eye.position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    visibleTarget = candidate;
                }
            }

            for (int i = 0; i < candidateCount; i++) _candidates[i] = null;
            return visibleTarget != null;
        }

        public bool CanSee(GuardTarget target)
        {
            if (target == null || !target.IsAvailable) return false;

            Vector3 origin = eye.position;
            Vector3 toTarget = target.AimPosition - origin;
            float distance = toTarget.magnitude;
            if (distance > visionDistance || distance < 0.001f) return false;

            Vector3 direction = toTarget / distance;
            if (Vector3.Angle(transform.forward, direction) > visionAngle * 0.5f) return false;

            return !Physics.Raycast(origin, direction, distance, obstructionMask, QueryTriggerInteraction.Ignore);
        }

        public void SetIndicatorState(GuardState state)
        {
            if (visionIndicator == null) return;
            if (_propertyBlock == null) _propertyBlock = new MaterialPropertyBlock();

            Color color = state == GuardState.Chase
                ? chaseColor
                : state == GuardState.Suspicious
                    ? suspiciousColor
                    : patrolColor;

            visionIndicator.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor("_BaseColor", color);
            _propertyBlock.SetColor("_Color", color);
            visionIndicator.SetPropertyBlock(_propertyBlock);
        }

        private bool ContainsCandidate(GuardTarget candidate, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_candidates[i] == candidate) return true;
            }

            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Transform originTransform = eye != null ? eye : transform;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(originTransform.position, visionDistance);

            Quaternion left = Quaternion.Euler(0f, -visionAngle * 0.5f, 0f);
            Quaternion right = Quaternion.Euler(0f, visionAngle * 0.5f, 0f);
            Gizmos.DrawRay(originTransform.position, left * transform.forward * visionDistance);
            Gizmos.DrawRay(originTransform.position, right * transform.forward * visionDistance);
        }
    }
}
