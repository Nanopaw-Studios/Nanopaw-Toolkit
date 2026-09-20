// © 2025-2026 Nanodogs Studios. All rights reserved.

using System;
using UnityEngine;

namespace Nanodogs.Toolkit.Missions
{
    public enum ObjectiveType
    {
        ReachLocation,
        TriggerZone,
        CollectItems,
        DefeatTargets,
        Timer,
        Custom
    }

    [Serializable]
    public class NanoObjectiveData
    {
        [Tooltip("Unique ID for this objective within the mission.")]
        public string id = Guid.NewGuid().ToString().Substring(0, 8);

        [Tooltip("The title or prompt displayed in the UI (e.g. 'Go to the Grass Ball', 'Collect 3 Keys').")]
        public string title = "New Objective";

        [TextArea(1, 3)]
        [Tooltip("Optional longer description or hint.")]
        public string description = "";

        [Tooltip("The type of objective.")]
        public ObjectiveType type = ObjectiveType.ReachLocation;

        [Header("Reach Location Settings")]
        [Tooltip("Target world position the player must reach.")]
        public Vector3 targetPosition;

        [Tooltip("Radius around the target position to trigger completion.")]
        public float radius = 3.5f;

        [Header("Trigger Zone Settings")]
        [Tooltip("The ID of the NanoMissionTriggerZone the player must enter.")]
        public string targetZoneId = "";

        [Header("Collect / Defeat Settings")]
        [Tooltip("The tag or identifier matching NanoMissionCollectable or NanoMissionTarget.")]
        public string targetTag = "Item";

        [Tooltip("Number of items to collect or targets to defeat.")]
        public int requiredAmount = 1;

        [Header("Timer Settings")]
        [Tooltip("Duration in seconds the player must survive or complete the task within.")]
        public float timerDuration = 30f;

        [Header("Extra Settings")]
        [Tooltip("If true, this objective does not block mission completion.")]
        public bool isOptional = false;

        public NanoObjectiveData Clone()
        {
            return new NanoObjectiveData
            {
                id = this.id,
                title = this.title,
                description = this.description,
                type = this.type,
                targetPosition = this.targetPosition,
                radius = this.radius,
                targetZoneId = this.targetZoneId,
                targetTag = this.targetTag,
                requiredAmount = this.requiredAmount,
                timerDuration = this.timerDuration,
                isOptional = this.isOptional
            };
        }
    }
}
