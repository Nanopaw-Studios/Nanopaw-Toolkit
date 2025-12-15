using UnityEngine;

namespace Nanodogs.API.NanoMissions
{
    [CreateAssetMenu(fileName = "TriggerObjective", menuName = "Nanodogs/Missions/Objective - Trigger")]
    public class TriggerObjective : MissionObjective
    {
        [Header("Target")]
        public Vector3 targetPosition;

        [Header("Settings")]
        public float radius = 3f;
        public LayerMask playerLayer;

        private Transform player;
        private Collider objectiveTrigger;

        public void SetPlayer(Transform playerTransform)
        {
            player = playerTransform;

            // Create a trigger zone at the target position
            if (objectiveTrigger == null)
            {
                GameObject triggerZone = new GameObject("ObjectiveTriggerZone");
                objectiveTrigger = triggerZone.AddComponent<Collider>();
                objectiveTrigger.isTrigger = true;
                triggerZone.transform.position = targetPosition;
                triggerZone.transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2); // Scale based on radius
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // If the player enters the trigger zone, complete the objective
            if (((1 << other.gameObject.layer) & playerLayer) != 0) // Check if the other object is the player
            {
                Complete();
            }
        }

        public override bool CheckProgress()
        {
            if (state != ObjectiveState.Active || player == null) return false;

            return state == ObjectiveState.Completed;
        }
    }
}
