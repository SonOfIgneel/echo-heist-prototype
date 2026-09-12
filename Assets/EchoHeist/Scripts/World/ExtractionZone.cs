using UnityEngine;

namespace EchoHeist
{
    [RequireComponent(typeof(Collider))]
    public sealed class ExtractionZone : MonoBehaviour
    {
        [SerializeField] private RunManager runManager;

        private void OnTriggerEnter(Collider other)
        {
            PlayerObjectiveState objectiveState = other.GetComponentInParent<PlayerObjectiveState>();
            if (objectiveState == null) return;

            if (objectiveState.HasDataCore) runManager.CompleteRun();
            else runManager.NotifyStatus("DATA CORE REQUIRED");
        }
    }
}
