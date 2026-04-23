using System;
using System.Collections.Generic;
using UnityEngine;

namespace NarrativeGP
{
    public class GameState : MonoBehaviour
    {
        [Serializable]
        private class SectionRuntimeState
        {
            public SectionId sectionId;
            public bool openedToday;
            public bool completedToday;
            public bool completedEver;
            public bool hasUnreadContent;
        }

        [SerializeField] private int currentDay = 1;
        [SerializeField] private List<SectionRuntimeState> sectionStates = new();
        [SerializeField] private List<string> activeFlags = new();

        private Dictionary<SectionId, SectionRuntimeState> sectionLookup;
        private HashSet<string> flagLookup;

        public int CurrentDay => currentDay;

        public event Action StateChanged;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            sectionStates ??= new List<SectionRuntimeState>();
            activeFlags ??= new List<string>();

            if (sectionLookup == null || flagLookup == null)
            {
                BuildLookups();
                EnsureAllSectionsExist();
            }
        }

        private void BuildLookups()
        {
            sectionLookup = new Dictionary<SectionId, SectionRuntimeState>();
            foreach (SectionRuntimeState state in sectionStates)
            {
                sectionLookup[state.sectionId] = state;
            }

            flagLookup = new HashSet<string>(activeFlags);
        }

        private void EnsureAllSectionsExist()
        {
            foreach (SectionId sectionId in Enum.GetValues(typeof(SectionId)))
            {
                if (sectionLookup.ContainsKey(sectionId))
                {
                    continue;
                }

                var state = new SectionRuntimeState { sectionId = sectionId };
                sectionStates.Add(state);
                sectionLookup.Add(sectionId, state);
            }
        }

        public void StartNewDay()
        {
            EnsureInitialized();
            currentDay++;

            foreach (SectionRuntimeState state in sectionStates)
            {
                state.openedToday = false;
                state.completedToday = false;
                state.hasUnreadContent = false;
            }

            NotifyStateChanged();
        }

        public void MarkSectionOpened(SectionId sectionId)
        {
            EnsureInitialized();
            SectionRuntimeState state = GetState(sectionId);
            if (state.openedToday)
            {
                return;
            }

            state.openedToday = true;
            NotifyStateChanged();
        }

        public void MarkSectionCompleted(SectionId sectionId)
        {
            EnsureInitialized();
            SectionRuntimeState state = GetState(sectionId);

            if (state.openedToday && state.completedToday && state.completedEver && !state.hasUnreadContent)
            {
                return;
            }

            state.openedToday = true;
            state.completedToday = true;
            state.completedEver = true;
            state.hasUnreadContent = false;
            NotifyStateChanged();
        }

        public void SetUnreadContent(SectionId sectionId, bool hasUnreadContent)
        {
            EnsureInitialized();
            SectionRuntimeState state = GetState(sectionId);
            bool didChange = state.hasUnreadContent != hasUnreadContent;
            state.hasUnreadContent = hasUnreadContent;

            if (hasUnreadContent)
            {
                didChange |= state.completedToday;
                state.completedToday = false;
            }

            if (didChange)
            {
                NotifyStateChanged();
            }
        }

        public void SetSectionDailyProgress(SectionId sectionId, bool hasUnreadContent, bool isClearedToday)
        {
            EnsureInitialized();
            SectionRuntimeState state = GetState(sectionId);
            bool didChange = false;

            if (state.hasUnreadContent != hasUnreadContent)
            {
                state.hasUnreadContent = hasUnreadContent;
                didChange = true;
            }

            if (state.completedToday != isClearedToday)
            {
                state.completedToday = isClearedToday;
                didChange = true;
            }

            if (isClearedToday && !state.completedEver)
            {
                state.completedEver = true;
                didChange = true;
            }

            if (didChange)
            {
                NotifyStateChanged();
            }
        }

        public bool WasOpenedToday(SectionId sectionId) => GetState(sectionId).openedToday;

        public bool WasCompletedToday(SectionId sectionId) => GetState(sectionId).completedToday;

        public bool WasCompletedEver(SectionId sectionId) => GetState(sectionId).completedEver;

        public bool HasUnreadContent(SectionId sectionId) => GetState(sectionId).hasUnreadContent;

        public void SetFlag(string flagKey, bool enabled)
        {
            EnsureInitialized();
            if (string.IsNullOrWhiteSpace(flagKey))
            {
                return;
            }

            if (enabled)
            {
                if (flagLookup.Add(flagKey))
                {
                    activeFlags.Add(flagKey);
                    NotifyStateChanged();
                }

                return;
            }

            if (flagLookup.Remove(flagKey))
            {
                activeFlags.Remove(flagKey);
                NotifyStateChanged();
            }
        }

        public bool HasFlag(string flagKey)
        {
            EnsureInitialized();
            if (string.IsNullOrWhiteSpace(flagKey))
            {
                return false;
            }

            return flagLookup.Contains(flagKey);
        }

        private SectionRuntimeState GetState(SectionId sectionId)
        {
            EnsureInitialized();

            if (!sectionLookup.TryGetValue(sectionId, out SectionRuntimeState state))
            {
                state = new SectionRuntimeState { sectionId = sectionId };
                sectionStates.Add(state);
                sectionLookup.Add(sectionId, state);
            }

            return state;
        }

        private void NotifyStateChanged()
        {
            StateChanged?.Invoke();
        }
    }
}
