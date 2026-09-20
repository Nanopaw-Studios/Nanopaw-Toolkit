// © 2025-2026 Nanodogs Studios. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nanodogs.Toolkit.Missions
{
    /// <summary>
    /// Immutable mission definition asset. Contains blueprints for objectives and metadata.
    /// Runtime state is NEVER stored here to prevent asset file mutation and play mode persistence bugs.
    /// </summary>
    [CreateAssetMenu(fileName = "NewNanoMission", menuName = "Nanodogs/Missions/Mission Data", order = 1)]
    public class NanoMissionData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique ID for this mission.")]
        public string missionId = "mission_01";

        [Tooltip("Display name shown in UI.")]
        public string missionTitle = "New Mission";

        [Tooltip("Category or group (e.g., 'Main Quest', 'Side Quest', 'Tutorial').")]
        public string category = "Main Quest";

        [TextArea(2, 5)]
        [Tooltip("Detailed description of the mission.")]
        public string description = "Describe the mission objectives and background here.";

        [Header("Flow & Progression")]
        [Tooltip("Automatically start this mission when the scene starts or when loaded into MissionManager.")]
        public bool autoStart = false;

        [Tooltip("If true, objectives must be completed in order one-by-one. If false, all objectives are active in parallel.")]
        public bool sequentialObjectives = false;

        [Tooltip("Optional mission to automatically start when this mission is completed.")]
        public NanoMissionData nextMission;

        [Header("Rewards")]
        [TextArea(1, 3)]
        [Tooltip("Summary of rewards granted upon completion.")]
        public string rewardText = "";

        [Header("Objectives")]
        [Tooltip("List of objectives that define this mission.")]
        public List<NanoObjectiveData> objectives = new List<NanoObjectiveData>();

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(missionId))
            {
                missionId = !string.IsNullOrEmpty(missionTitle)
                    ? missionTitle.ToLowerInvariant().Replace(" ", "_")
                    : "mission_" + Guid.NewGuid().ToString().Substring(0, 6);
            }
        }
    }
}
