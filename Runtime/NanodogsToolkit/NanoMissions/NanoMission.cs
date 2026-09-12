// © 2025-2026 Nanodogs Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nanodogs.Toolkit.Missions
{
    public enum MissionStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }

    /// <summary>
    /// Pure C# runtime instance of a Mission.
    /// Manages objective progression, events, and completion logic during gameplay.
    /// Does NOT mutate any ScriptableObject assets.
    /// </summary>
    public class NanoMission
    {
        public NanoMissionData Data { get; private set; }
        public MissionStatus Status { get; private set; } = MissionStatus.NotStarted;
        public List<NanoObjective> Objectives { get; private set; } = new List<NanoObjective>();

        public event Action<NanoMission> OnMissionStarted;
        public event Action<NanoMission, NanoObjective> OnObjectiveUpdated;
        public event Action<NanoMission> OnMissionCompleted;
        public event Action<NanoMission> OnMissionFailed;

        public bool IsCompleted => Status == MissionStatus.Completed;
        public bool IsInProgress => Status == MissionStatus.InProgress;

        public NanoMission(NanoMissionData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));

            Objectives.Clear();
            if (data.objectives != null)
            {
                foreach (var objData in data.objectives)
                {
                    if (objData == null) continue;
                    var runtimeObj = new NanoObjective(objData);
                    runtimeObj.OnObjectiveChanged += HandleObjectiveChanged;
                    Objectives.Add(runtimeObj);
                }
            }
        }

        private void HandleObjectiveChanged(NanoObjective objective)
        {
            OnObjectiveUpdated?.Invoke(this, objective);

            if (objective.IsCompleted)
            {
                if (Data.sequentialObjectives)
                {
                    ActivateNextSequentialObjective();
                }

                CheckCompletion();
            }
        }

        public void StartMission()
        {
            if (Status == MissionStatus.InProgress || Status == MissionStatus.Completed)
                return;

            Status = MissionStatus.InProgress;

            if (Objectives.Count > 0)
            {
                if (Data.sequentialObjectives)
                {
                    ActivateNextSequentialObjective();
                }
                else
                {
                    foreach (var obj in Objectives)
                    {
                        obj.Activate();
                    }
                }
            }

            Debug.Log($"[NanoMissions] Mission started: '{Data.missionTitle}' ({Data.missionId})");
            OnMissionStarted?.Invoke(this);

            CheckCompletion();
        }

        private void ActivateNextSequentialObjective()
        {
            foreach (var obj in Objectives)
            {
                if (!obj.IsCompleted)
                {
                    obj.Activate();
                    break;
                }
            }
        }

        public void CheckCompletion()
        {
            if (Status != MissionStatus.InProgress) return;

            bool allRequiredDone = true;
            foreach (var obj in Objectives)
            {
                if (!obj.Data.isOptional && !obj.IsCompleted)
                {
                    allRequiredDone = false;
                    break;
                }
            }

            if (allRequiredDone && Objectives.Count > 0)
            {
                CompleteMission();
            }
        }

        public void CompleteMission()
        {
            if (Status == MissionStatus.Completed) return;

            Status = MissionStatus.Completed;
            Debug.Log($"[NanoMissions] Mission completed: '{Data.missionTitle}'!");
            OnMissionCompleted?.Invoke(this);
        }

        public void FailMission()
        {
            if (Status == MissionStatus.Completed) return;

            Status = MissionStatus.Failed;
            Debug.LogWarning($"[NanoMissions] Mission failed: '{Data.missionTitle}'.");
            OnMissionFailed?.Invoke(this);
        }

        public void Reset()
        {
            Status = MissionStatus.NotStarted;
            foreach (var obj in Objectives)
            {
                obj.Reset();
            }
        }

        public List<NanoObjective> GetActiveObjectives()
        {
            return Objectives.Where(o => o.IsActive).ToList();
        }

        public NanoObjective FindObjective(string objectiveId)
        {
            return Objectives.FirstOrDefault(o => o.Data.id == objectiveId);
        }

        public void TickTimers(float deltaTime)
        {
            if (Status != MissionStatus.InProgress) return;

            for (int i = 0; i < Objectives.Count; i++)
            {
                var obj = Objectives[i];
                if (obj.IsActive && obj.Data.type == ObjectiveType.Timer)
                {
                    obj.TickTimer(deltaTime);
                }
            }
        }
    }
}
