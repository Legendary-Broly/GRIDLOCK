using System;
using UnityEngine;
using NewGameplay.SplitGrid.Interfaces;
using NewGameplay.SplitGrid.Services;
using NewGameplay.SplitGrid.Views;
using NewGameplay.SplitGrid.Data;
using NewGameplay.SplitGrid.Controllers;
using NewGameplay.Interfaces;

namespace NewGameplay.SplitGrid.Services
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

        public event Action OnProgressChanged;

        public int FragmentsFound => revealedFragmentsA + revealedFragmentsB;
        public int RequiredFragments => requiredFragmentsA + requiredFragmentsB;

        public SplitProgressTrackerService(
            ISplitGridService splitGridService,
            IDataFragmentService dataFragmentServiceA,
            IDataFragmentService dataFragmentServiceB)
        {
            this.splitGridService = splitGridService;
            this.dataFragmentServiceA = dataFragmentServiceA;
            this.dataFragmentServiceB = dataFragmentServiceB;
        }

        public void SetRequiredFragments(int count)
        {
            requiredFragmentsA = count;
            requiredFragmentsB = count;
            OnProgressChanged?.Invoke();
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
            OnProgressChanged?.Invoke();
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

        public void NotifyFragmentRevealed()
        {
            OnProgressChanged?.Invoke();
        }

        public void NotifyFragmentRevealed(int x, int y)
        {
            // Determine which grid the fragment was revealed in
            if (splitGridService.IsInBounds(GridID.GridA, x, y))
            {
                revealedFragmentsA++;
            }
            else if (splitGridService.IsInBounds(GridID.GridB, x, y))
            {
                revealedFragmentsB++;
            }
            OnProgressChanged?.Invoke();
        }

        public int GetRevealedFragmentCount()
        {
            return revealedFragmentsA + revealedFragmentsB;
        }

        public bool AreAllFragmentsRevealed()
        {
            return revealedFragmentsA >= requiredFragmentsA && 
                   revealedFragmentsB >= requiredFragmentsB;
        }

        public bool HasMetGoal()
        {
            return AreAllFragmentsRevealed();
        }

        public void ResetProgress()
        {
            revealedFragmentsA = 0;
            revealedFragmentsB = 0;
            OnProgressChanged?.Invoke();
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