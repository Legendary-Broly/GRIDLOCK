using System;
using UnityEngine;
using SplitGrid.Interfaces;
using NewGameplay.Interfaces;

namespace SplitGrid.Services
{
    public class SplitProgressTrackerService : ISplitProgressTrackerService
    {
        private readonly ISplitGridService splitGridService;
        private readonly IDataFragmentService dataFragmentServiceA;
        private readonly IDataFragmentService dataFragmentServiceB;

        private int requiredFragmentsA;
        private int requiredFragmentsB;
        private int revealedFragmentsA;
        private int revealedFragmentsB;

        public event Action OnProgressUpdated;

        public SplitProgressTrackerService(
            ISplitGridService splitGridService,
            IDataFragmentService dataFragmentServiceA,
            IDataFragmentService dataFragmentServiceB)
        {
            this.splitGridService = splitGridService;
            this.dataFragmentServiceA = dataFragmentServiceA;
            this.dataFragmentServiceB = dataFragmentServiceB;
        }

        public void SetRequiredFragments(GridID gridId, int count)
        {
            switch (gridId)
            {
                case GridID.GridA:
                    requiredFragmentsA = count;
                    break;
                case GridID.GridB:
                    requiredFragmentsB = count;
                    break;
            }
            OnProgressUpdated?.Invoke();
        }

        public int GetRequiredFragments(GridID gridId) => gridId switch
        {
            GridID.GridA => requiredFragmentsA,
            GridID.GridB => requiredFragmentsB,
            _ => throw new ArgumentException($"Invalid grid ID: {gridId}")
        };

        public int GetRevealedFragments(GridID gridId) => gridId switch
        {
            GridID.GridA => revealedFragmentsA,
            GridID.GridB => revealedFragmentsB,
            _ => throw new ArgumentException($"Invalid grid ID: {gridId}")
        };

        public void NotifyFragmentRevealed(GridID gridId, int x, int y)
        {
            switch (gridId)
            {
                case GridID.GridA:
                    revealedFragmentsA++;
                    break;
                case GridID.GridB:
                    revealedFragmentsB++;
                    break;
            }
            OnProgressUpdated?.Invoke();
        }

        public bool AreAllFragmentsRevealed()
        {
            return revealedFragmentsA >= requiredFragmentsA && 
                   revealedFragmentsB >= requiredFragmentsB;
        }

        public void ResetProgress()
        {
            revealedFragmentsA = 0;
            revealedFragmentsB = 0;
            OnProgressUpdated?.Invoke();
        }

        public float GetProgressPercentage()
        {
            int totalRequired = requiredFragmentsA + requiredFragmentsB;
            int totalRevealed = revealedFragmentsA + revealedFragmentsB;

            if (totalRequired == 0) return 0f;
            return (float)totalRevealed / totalRequired;
        }

        public float GetGridProgressPercentage(GridID gridId)
        {
            int required = GetRequiredFragments(gridId);
            int revealed = GetRevealedFragments(gridId);

            if (required == 0) return 0f;
            return (float)revealed / required;
        }

        public bool IsGridComplete(GridID gridId)
        {
            return GetRevealedFragments(gridId) >= GetRequiredFragments(gridId);
        }
    }
} 