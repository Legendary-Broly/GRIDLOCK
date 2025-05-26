using UnityEngine;
using System.Collections.Generic;
using SplitGrid.Interfaces;
using NewGameplay.Interfaces;
using NewGameplay.Enums;
using NewGameplay.ScriptableObjects;
using NewGameplay.Services;
using NewGameplay.Controllers;
using System.Linq;
using NewGameplay.Views;
using NewGameplay.UI;
using NewGameplay.Strategies;
using System;
using NewGameplay.Models;
using System.Collections;

namespace SplitGrid.Controllers
{
    public class SplitExtractController : MonoBehaviour
    {
        [SerializeField] private GridViewNew gridViewA;
        [SerializeField] private GridViewNew gridViewB;

        private ISplitGridService splitGridService;
        private IExtractService extractService;
        private ISplitProgressTrackerService progressTracker;
        private IVirusService virusServiceA;
        private IVirusService virusServiceB;
        private IChatLogService chatLogService;
        private PayloadManager payloadManager;

        public event Action OnExtractComplete;

        public void Initialize(
            ISplitGridService splitGridService,
            IExtractService extractService,
            ISplitProgressTrackerService progressTracker,
            IVirusService virusServiceA,
            IVirusService virusServiceB,
            IChatLogService chatLogService,
            PayloadManager payloadManager)
        {
            this.splitGridService = splitGridService;
            this.extractService = extractService;
            this.progressTracker = progressTracker;
            this.virusServiceA = virusServiceA;
            this.virusServiceB = virusServiceB;
            this.chatLogService = chatLogService;
            this.payloadManager = payloadManager;

            if (extractService != null)
                extractService.OnExtractComplete += HandleExtractComplete;
        }

        public void HandleExtractRequest()
        {

            // Clear both grids except viruses
            ClearAllExceptViruses();

            // Trigger extraction
            extractService?.ExtractGrid();
        }

        private bool CanExtract()
        {
            // Check if all required fragments are revealed
            if (!progressTracker.AreAllFragmentsRevealed())
                return false;

            // Check if both grids are virus-free
            if (!AreGridsVirusFree())
                return false;

            return true;
        }

        private bool AreGridsVirusFree()
        {
            // Check Grid A
            for (int y = 0; y < splitGridService.GetGridHeight(GridID.GridA); y++)
                for (int x = 0; x < splitGridService.GetGridWidth(GridID.GridA); x++)
                    if (virusServiceA.HasVirusAt(x, y))
                        return false;

            // Check Grid B
            for (int y = 0; y < splitGridService.GetGridHeight(GridID.GridB); y++)
                for (int x = 0; x < splitGridService.GetGridWidth(GridID.GridB); x++)
                    if (virusServiceB.HasVirusAt(x, y))
                        return false;

            return true;
        }

        private void ClearAllExceptViruses()
        {
            // Clear Grid A
            for (int y = 0; y < splitGridService.GetGridHeight(GridID.GridA); y++)
                for (int x = 0; x < splitGridService.GetGridWidth(GridID.GridA); x++)
                    if (!virusServiceA.HasVirusAt(x, y))
                        splitGridService.SetSymbol(GridID.GridA, x, y, "");

            // Clear Grid B
            for (int y = 0; y < splitGridService.GetGridHeight(GridID.GridB); y++)
                for (int x = 0; x < splitGridService.GetGridWidth(GridID.GridB); x++)
                    if (!virusServiceB.HasVirusAt(x, y))
                        splitGridService.SetSymbol(GridID.GridB, x, y, "");
        }

        private void HandleExtractComplete()
        {
            // Update views
            if (gridViewA != null) gridViewA.RenderGrid();
            if (gridViewB != null) gridViewB.RenderGrid();

            // Notify listeners
            OnExtractComplete?.Invoke();
        }

        private void OnDestroy()
        {
            if (extractService != null)
                extractService.OnExtractComplete -= HandleExtractComplete;
        }
    }
} 