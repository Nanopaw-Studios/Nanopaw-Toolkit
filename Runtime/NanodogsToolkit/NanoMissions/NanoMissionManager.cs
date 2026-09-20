// © 2025-2026 Nanodogs Studios. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nanodogs.Toolkit.Missions
{
    /// <summary>
    /// Central manager for NanoMissions. Handles mission lifecycles, objective tracking,
    /// scene triggers, collectables, proximity checks, and UI event broadcasting.
    /// </summary>
    [DisallowMultipleComponent]
    public class NanoMissionManager : MonoBehaviour
    {
        public static NanoMissionManager Instance { get; private set; }

        [Header("Player Target")]
        [Tooltip("Transform of the player. If left empty, automatically finds Camera.main or an object tagged 'Player'.")]
        public Transform playerTransform;

        [Header("Missions Configuration")]
        [Tooltip("List of missions to load into the game.")]
        public List<NanoMissionData> missionsToLoad = new List<NanoMissionData>();

        [Tooltip("If true, automatically starts the first mission on Awake/Start.")]
        public bool autoStartFirst = true;

        [Header("Proximity Settings")]
        [Tooltip("Interval in seconds between distance checks for ReachLocation objectives.")]
        public float distanceCheckInterval = 0.2f;

        [Header("Unity Events")]
        public UnityEvent<string> onMissionStartedEvent;
        public UnityEvent<string> onMissionCompletedEvent;
        public UnityEvent<string, string> onObjectiveCompletedEvent;

        // C# Events
        public event Action<NanoMission> OnMissionStarted;
        public event Action<NanoMission, NanoObjective> OnObjectiveUpdated;
        public event Action<NanoMission> OnMissionCompleted;
        public event Action<NanoMission> OnMissionFailed;
        public event Action<NanoMission> OnTrackedMissionChanged;

        private readonly Dictionary<string, NanoMission> _missions = new Dictionary<string, NanoMission>();
        private readonly List<NanoMission> _activeMissions = new List<NanoMission>();
        private readonly List<NanoMission> _completedMissions = new List<NanoMission>();

        private NanoMission _trackedMission;
        private float _distanceCheckTimer = 0f;

        public NanoMission TrackedMission => _trackedMission;
        public IReadOnlyList<NanoMission> ActiveMissions => _activeMissions;
        public IReadOnlyList<NanoMission> CompletedMissions => _completedMissions;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeMissions();
        }

        private void Start()
        {
            EnsurePlayerReference();

            if (autoStartFirst && missionsToLoad != null && missionsToLoad.Count > 0)
            {
                var first = missionsToLoad[0];
                if (first != null)
                {
                    StartMission(first);
                }
            }

            // Also start any missions with autoStart = true
            foreach (var missionData in missionsToLoad)
            {
                if (missionData != null && missionData.autoStart && !IsMissionActive(missionData.missionId))
                {
                    StartMission(missionData);
                }
            }
        }

        private void Update()
        {
            EnsurePlayerReference();

            float dt = Time.deltaTime;

            // Tick timers on active missions
            for (int i = 0; i < _activeMissions.Count; i++)
            {
                _activeMissions[i].TickTimers(dt);
            }

            // Periodic distance check for ReachLocation objectives
            _distanceCheckTimer += dt;
            if (_distanceCheckTimer >= distanceCheckInterval)
            {
                _distanceCheckTimer = 0f;
                CheckProximityObjectives();
            }
        }

        private void InitializeMissions()
        {
            _missions.Clear();
            _activeMissions.Clear();
            _completedMissions.Clear();

            if (missionsToLoad == null) return;

            foreach (var data in missionsToLoad)
            {
                if (data == null) continue;
                RegisterMission(data);
            }
        }

        public NanoMission RegisterMission(NanoMissionData data)
        {
            if (data == null) return null;

            if (_missions.TryGetValue(data.missionId, out var existing))
            {
                return existing;
            }

            var runtimeMission = new NanoMission(data);
            runtimeMission.OnMissionStarted += HandleMissionStarted;
            runtimeMission.OnObjectiveUpdated += HandleObjectiveUpdated;
            runtimeMission.OnMissionCompleted += HandleMissionCompleted;
            runtimeMission.OnMissionFailed += HandleMissionFailed;

            _missions[data.missionId] = runtimeMission;
            return runtimeMission;
        }

        public NanoMission StartMission(NanoMissionData data)
        {
            if (data == null) return null;
            var mission = RegisterMission(data);
            mission.StartMission();
            return mission;
        }

        public NanoMission StartMission(string missionId)
        {
            if (string.IsNullOrEmpty(missionId)) return null;

            if (_missions.TryGetValue(missionId, out var mission))
            {
                mission.StartMission();
                return mission;
            }

            Debug.LogWarning($"[NanoMissions] Mission with ID '{missionId}' not found in MissionManager.");
            return null;
        }

        public void SetTrackedMission(NanoMission mission)
        {
            _trackedMission = mission;
            OnTrackedMissionChanged?.Invoke(_trackedMission);
        }

        public bool IsMissionActive(string missionId)
        {
            return _activeMissions.Exists(m => m.Data.missionId == missionId);
        }

        public bool IsMissionCompleted(string missionId)
        {
            return _completedMissions.Exists(m => m.Data.missionId == missionId);
        }

        public NanoMission GetMission(string missionId)
        {
            _missions.TryGetValue(missionId, out var m);
            return m;
        }

        #region Event Handlers

        private void HandleMissionStarted(NanoMission mission)
        {
            if (!_activeMissions.Contains(mission))
            {
                _activeMissions.Add(mission);
            }

            if (_trackedMission == null || !_trackedMission.IsInProgress)
            {
                SetTrackedMission(mission);
            }

            OnMissionStarted?.Invoke(mission);
            onMissionStartedEvent?.Invoke(mission.Data.missionTitle);
        }

        private void HandleObjectiveUpdated(NanoMission mission, NanoObjective objective)
        {
            OnObjectiveUpdated?.Invoke(mission, objective);

            if (objective.IsCompleted)
            {
                onObjectiveCompletedEvent?.Invoke(mission.Data.missionTitle, objective.Data.title);
            }
        }

        private void HandleMissionCompleted(NanoMission mission)
        {
            _activeMissions.Remove(mission);
            if (!_completedMissions.Contains(mission))
            {
                _completedMissions.Add(mission);
            }

            OnMissionCompleted?.Invoke(mission);
            onMissionCompletedEvent?.Invoke(mission.Data.missionTitle);

            // Chain next mission if configured
            if (mission.Data.nextMission != null)
            {
                StartMission(mission.Data.nextMission);
            }
            else if (_trackedMission == mission)
            {
                // Auto-track another active mission if available
                SetTrackedMission(_activeMissions.Count > 0 ? _activeMissions[0] : null);
            }
        }

        private void HandleMissionFailed(NanoMission mission)
        {
            _activeMissions.Remove(mission);
            OnMissionFailed?.Invoke(mission);

            if (_trackedMission == mission)
            {
                SetTrackedMission(_activeMissions.Count > 0 ? _activeMissions[0] : null);
            }
        }

        #endregion

        #region Gameplay Reporting API

        public void ReportTriggerEnter(string zoneId, GameObject actor = null)
        {
            if (string.IsNullOrEmpty(zoneId)) return;

            foreach (var mission in _activeMissions)
            {
                foreach (var obj in mission.GetActiveObjectives())
                {
                    if (obj.Data.type == ObjectiveType.TriggerZone && obj.Data.targetZoneId == zoneId)
                    {
                        obj.Complete();
                    }
                }
            }
        }

        public void ReportCollectable(string itemTag, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemTag)) return;

            foreach (var mission in _activeMissions)
            {
                foreach (var obj in mission.GetActiveObjectives())
                {
                    if (obj.Data.type == ObjectiveType.CollectItems &&
                        string.Equals(obj.Data.targetTag, itemTag, StringComparison.OrdinalIgnoreCase))
                    {
                        obj.AddProgress(amount);
                    }
                }
            }
        }

        public void ReportDefeat(string targetTag, int amount = 1)
        {
            if (string.IsNullOrEmpty(targetTag)) return;

            foreach (var mission in _activeMissions)
            {
                foreach (var obj in mission.GetActiveObjectives())
                {
                    if (obj.Data.type == ObjectiveType.DefeatTargets &&
                        string.Equals(obj.Data.targetTag, targetTag, StringComparison.OrdinalIgnoreCase))
                    {
                        obj.AddProgress(amount);
                    }
                }
            }
        }

        public void ReportProgress(string targetTagOrId, int amount = 1)
        {
            if (string.IsNullOrEmpty(targetTagOrId)) return;

            foreach (var mission in _activeMissions)
            {
                foreach (var obj in mission.GetActiveObjectives())
                {
                    if (string.Equals(obj.Data.targetTag, targetTagOrId, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(obj.Data.id, targetTagOrId, StringComparison.OrdinalIgnoreCase))
                    {
                        obj.AddProgress(amount);
                    }
                }
            }
        }

        public void CompleteObjective(string missionId, string objectiveId)
        {
            var mission = GetMission(missionId);
            if (mission == null) return;

            var obj = mission.FindObjective(objectiveId);
            if (obj != null && obj.IsActive)
            {
                obj.Complete();
            }
        }

        private void CheckProximityObjectives()
        {
            if (playerTransform == null) return;

            Vector3 playerPos = playerTransform.position;

            foreach (var mission in _activeMissions)
            {
                foreach (var obj in mission.GetActiveObjectives())
                {
                    if (obj.Data.type == ObjectiveType.ReachLocation)
                    {
                        float distSq = (playerPos - obj.Data.targetPosition).sqrMagnitude;
                        float radSq = obj.Data.radius * obj.Data.radius;

                        if (distSq <= radSq)
                        {
                            obj.Complete();
                        }
                    }
                }
            }
        }

        private void EnsurePlayerReference()
        {
            if (playerTransform != null) return;

            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null)
            {
                playerTransform = playerGo.transform;
                return;
            }

            if (Camera.main != null)
            {
                playerTransform = Camera.main.transform;
            }
        }

        #endregion
    }
}
