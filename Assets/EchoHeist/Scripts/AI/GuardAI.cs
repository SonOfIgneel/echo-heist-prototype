using UnityEngine;

namespace EchoHeist
{
    public enum GuardState
    {
        Patrol,
        Suspicious,
        Chase,
        ReturnToPatrol
    }

    [RequireComponent(typeof(Rigidbody))]
    public sealed class GuardAI : MonoBehaviour, IRunResettable
    {
        [Header("References")]
        [SerializeField] private Rigidbody body;
        [SerializeField] private GuardVision vision;
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private RunManager runManager;
        [SerializeField] private PrototypeHUD hud;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float patrolSpeed = 2f;
        [SerializeField, Min(0f)] private float chaseSpeed = 3.8f;
        [SerializeField, Min(0f)] private float turnSpeed = 420f;
        [SerializeField, Min(0.05f)] private float waypointTolerance = 0.2f;
        [SerializeField, Min(0f)] private float patrolPauseDuration = 0.35f;

        [Header("Detection")]
        [SerializeField, Min(0f)] private float detectionDelay = 0.35f;
        [SerializeField, Min(0f)] private float investigationDuration = 2.25f;
        [SerializeField, Min(0f)] private float targetLossDelay = 0.8f;
        [SerializeField, Min(0f)] private float minimumCommitmentDuration = 1.5f;
        [SerializeField, Min(0f)] private float echoCatchInterestDuration = 1f;
        [SerializeField, Min(0.1f)] private float catchDistance = 0.8f;

        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private GuardTarget _pendingTarget;
        private GuardTarget _currentTarget;
        private GuardTarget _dismissedEchoA;
        private GuardTarget _dismissedEchoB;
        private Vector3 _lastSeenPosition;
        private float _detectionTimer;
        private float _targetLostTimer;
        private float _investigationTimer;
        private float _pauseTimer;
        private float _commitUntil;
        private int _patrolIndex;

        public GuardState State { get; private set; }
        public GuardTarget CurrentTarget => _currentTarget;

        private void Awake()
        {
            if (body == null) body = GetComponent<Rigidbody>();
            if (vision == null) vision = GetComponent<GuardVision>();
            _startPosition = body.position;
            _startRotation = body.rotation;
            ResetState();
        }

        private void FixedUpdate()
        {
            UpdateTargetAwareness();

            switch (State)
            {
                case GuardState.Patrol:
                    UpdatePatrol();
                    break;
                case GuardState.Suspicious:
                    UpdateSuspicious();
                    break;
                case GuardState.Chase:
                    UpdateChase();
                    break;
                case GuardState.ReturnToPatrol:
                    UpdateReturnToPatrol();
                    break;
            }
        }

        public void ResetForRun()
        {
            ResetState();
            if (!body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
            body.detectCollisions = false;
            body.position = _startPosition;
            body.rotation = _startRotation;
            transform.SetPositionAndRotation(_startPosition, _startRotation);
            Physics.SyncTransforms();
            body.detectCollisions = true;
        }

        private void UpdateTargetAwareness()
        {
            if (_currentTarget != null)
            {
                if (vision.CanSee(_currentTarget))
                {
                    _lastSeenPosition = _currentTarget.transform.position;
                    _targetLostTimer = 0f;
                }
                else if (Time.time >= _commitUntil)
                {
                    _targetLostTimer += Time.fixedDeltaTime;
                    if (_targetLostTimer >= targetLossDelay) BeginInvestigation(investigationDuration);
                }

                return;
            }

            if (!vision.TryGetVisibleTarget(_pendingTarget, _dismissedEchoA, _dismissedEchoB,
                    out GuardTarget visibleTarget))
            {
                bool wasDetecting = _pendingTarget != null;
                _pendingTarget = null;
                _detectionTimer = 0f;
                if (State == GuardState.Suspicious && wasDetecting)
                {
                    TransitionTo(GuardState.ReturnToPatrol);
                }

                return;
            }

            if (_pendingTarget != visibleTarget)
            {
                _pendingTarget = visibleTarget;
                _detectionTimer = 0f;
                TransitionTo(GuardState.Suspicious);
            }

            _detectionTimer += Time.fixedDeltaTime;
            _lastSeenPosition = visibleTarget.transform.position;
            if (_detectionTimer >= detectionDelay) AcquireTarget(visibleTarget);
        }

        private void UpdatePatrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;

            if (_pauseTimer > 0f)
            {
                _pauseTimer -= Time.fixedDeltaTime;
                return;
            }

            Transform waypoint = patrolPoints[_patrolIndex];
            if (MoveTowards(waypoint.position, patrolSpeed))
            {
                _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
                _pauseTimer = patrolPauseDuration;
            }
        }

        private void UpdateSuspicious()
        {
            if (_pendingTarget != null)
            {
                TurnTowards(_pendingTarget.transform.position);
                return;
            }

            if (_investigationTimer > 0f)
            {
                _investigationTimer -= Time.fixedDeltaTime;
                MoveTowards(_lastSeenPosition, patrolSpeed);
                return;
            }

            TransitionTo(GuardState.ReturnToPatrol);
        }

        private void UpdateChase()
        {
            Vector3 destination = _currentTarget != null ? _currentTarget.transform.position : _lastSeenPosition;
            MoveTowards(destination, chaseSpeed);

            if (_currentTarget == null) return;
            Vector3 separation = _currentTarget.transform.position - transform.position;
            separation.y = 0f;
            if (separation.sqrMagnitude <= catchDistance * catchDistance) CatchCurrentTarget();
        }

        private void UpdateReturnToPatrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                if (MoveTowards(_startPosition, patrolSpeed)) TransitionTo(GuardState.Patrol);
                return;
            }

            Transform waypoint = patrolPoints[_patrolIndex];
            if (MoveTowards(waypoint.position, patrolSpeed)) TransitionTo(GuardState.Patrol);
        }

        private void AcquireTarget(GuardTarget target)
        {
            _currentTarget = target;
            _pendingTarget = null;
            _detectionTimer = 0f;
            _targetLostTimer = 0f;
            _lastSeenPosition = target.transform.position;
            _commitUntil = Time.time + minimumCommitmentDuration;
            TransitionTo(GuardState.Chase);

            hud.ShowStatus(target.Kind == GuardTargetKind.Echo
                ? "ECHO DETECTED - MOVE!"
                : "GUARD ALERTED!");
        }

        private void CatchCurrentTarget()
        {
            GuardTarget caughtTarget = _currentTarget;
            _currentTarget = null;
            _pendingTarget = null;
            _targetLostTimer = 0f;

            if (caughtTarget.Kind == GuardTargetKind.Player)
            {
                runManager.FailCurrentRun();
                return;
            }

            if (_dismissedEchoA == null) _dismissedEchoA = caughtTarget;
            else if (_dismissedEchoB == null && _dismissedEchoA != caughtTarget) _dismissedEchoB = caughtTarget;

            _lastSeenPosition = caughtTarget.transform.position;
            BeginInvestigation(echoCatchInterestDuration);
        }

        private void BeginInvestigation(float duration)
        {
            _currentTarget = null;
            _pendingTarget = null;
            _targetLostTimer = 0f;
            _investigationTimer = duration;
            TransitionTo(GuardState.Suspicious);
        }

        private bool MoveTowards(Vector3 destination, float speed)
        {
            Vector3 flatDestination = new Vector3(destination.x, body.position.y, destination.z);
            Vector3 nextPosition = Vector3.MoveTowards(body.position, flatDestination, speed * Time.fixedDeltaTime);
            Vector3 direction = flatDestination - body.position;
            body.MovePosition(nextPosition);
            if (direction.sqrMagnitude > 0.001f) TurnTowards(flatDestination);
            return (flatDestination - nextPosition).sqrMagnitude <= waypointTolerance * waypointTolerance;
        }

        private void TurnTowards(Vector3 destination)
        {
            Vector3 direction = destination - body.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            body.MoveRotation(Quaternion.RotateTowards(body.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }

        private void TransitionTo(GuardState nextState)
        {
            if (State == nextState) return;
            State = nextState;
            vision.SetIndicatorState(State);
        }

        private void ResetState()
        {
            _pendingTarget = null;
            _currentTarget = null;
            _dismissedEchoA = null;
            _dismissedEchoB = null;
            _lastSeenPosition = _startPosition;
            _detectionTimer = 0f;
            _targetLostTimer = 0f;
            _investigationTimer = 0f;
            _pauseTimer = 0f;
            _commitUntil = 0f;
            _patrolIndex = 0;
            State = GuardState.Patrol;
            if (vision != null) vision.SetIndicatorState(State);
        }

        private void OnValidate()
        {
            if (body == null) body = GetComponent<Rigidbody>();
            if (vision == null) vision = GetComponent<GuardVision>();
        }
    }
}
