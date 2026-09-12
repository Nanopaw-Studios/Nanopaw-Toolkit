// © 2025-2026 Nanodogs Studios. All rights reserved.

using System;
using UnityEngine;

namespace Nanodogs.Toolkit.Missions
{
    public enum ObjectiveStatus
    {
        Inactive,
        Active,
        Completed,
        Failed
    }

    /// <summary>
    /// Pure C# runtime instance of an objective.
    /// Never touches or serializes to ScriptableObject assets.
    /// </summary>
    public class NanoObjective
    {
        public NanoObjectiveData Data { get; private set; }
        public ObjectiveStatus Status { get; private set; } = ObjectiveStatus.Inactive;
        public int CurrentAmount { get; private set; } = 0;
        public float CurrentTimer { get; private set; } = 0f;

        public event Action<NanoObjective> OnObjectiveChanged;

        public bool IsCompleted => Status == ObjectiveStatus.Completed;
        public bool IsActive => Status == ObjectiveStatus.Active;

        public NanoObjective(NanoObjectiveData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Reset();
        }

        public void Activate()
        {
            if (Status == ObjectiveStatus.Completed) return;
            Status = ObjectiveStatus.Active;
            OnObjectiveChanged?.Invoke(this);
        }

        public void AddProgress(int amount)
        {
            if (Status != ObjectiveStatus.Active) return;

            CurrentAmount = Mathf.Clamp(CurrentAmount + amount, 0, Data.requiredAmount);
            OnObjectiveChanged?.Invoke(this);

            if (CurrentAmount >= Data.requiredAmount)
            {
                Complete();
            }
        }

        public void SetProgress(int amount)
        {
            if (Status != ObjectiveStatus.Active) return;

            CurrentAmount = Mathf.Clamp(amount, 0, Data.requiredAmount);
            OnObjectiveChanged?.Invoke(this);

            if (CurrentAmount >= Data.requiredAmount)
            {
                Complete();
            }
        }

        public void TickTimer(float deltaTime)
        {
            if (Status != ObjectiveStatus.Active || Data.type != ObjectiveType.Timer) return;

            CurrentTimer += deltaTime;
            OnObjectiveChanged?.Invoke(this);

            if (CurrentTimer >= Data.timerDuration)
            {
                Complete();
            }
        }

        public void Complete()
        {
            if (Status == ObjectiveStatus.Completed) return;
            CurrentAmount = Data.requiredAmount;
            CurrentTimer = Data.timerDuration;
            Status = ObjectiveStatus.Completed;
            OnObjectiveChanged?.Invoke(this);
        }

        public void Fail()
        {
            if (Status == ObjectiveStatus.Completed) return;
            Status = ObjectiveStatus.Failed;
            OnObjectiveChanged?.Invoke(this);
        }

        public void Reset()
        {
            Status = ObjectiveStatus.Inactive;
            CurrentAmount = 0;
            CurrentTimer = 0f;
            OnObjectiveChanged?.Invoke(this);
        }

        public string GetFormattedText()
        {
            string baseTitle = string.IsNullOrEmpty(Data.title) ? "Objective" : Data.title;

            if (Status == ObjectiveStatus.Completed)
            {
                return $"<s>{baseTitle}</s>";
            }

            switch (Data.type)
            {
                case ObjectiveType.CollectItems:
                case ObjectiveType.DefeatTargets:
                    if (Data.requiredAmount > 1)
                    {
                        return $"{baseTitle} ({CurrentAmount}/{Data.requiredAmount})";
                    }
                    return baseTitle;

                case ObjectiveType.Timer:
                    int remaining = Mathf.Max(0, Mathf.CeilToInt(Data.timerDuration - CurrentTimer));
                    return $"{baseTitle} ({remaining}s)";

                default:
                    return baseTitle;
            }
        }
    }
}
