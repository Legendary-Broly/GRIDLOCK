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
    public class SplitInjectController : MonoBehaviour
    {
        [SerializeField] private GridViewNew gridViewA;
        [SerializeField] private GridViewNew gridViewB;

        private ISplitGridService splitGridService;
        private IInjectService injectService;
        private SymbolToolService symbolToolServiceA;
        private SymbolToolService symbolToolServiceB;
        private IChatLogService chatLogService;

        public void Initialize(
            ISplitGridService splitGridService,
            IInjectService injectService,
            SymbolToolService symbolToolServiceA,
            SymbolToolService symbolToolServiceB,
            IChatLogService chatLogService)
        {
            this.splitGridService = splitGridService;
            this.injectService = injectService;
            this.symbolToolServiceA = symbolToolServiceA;
            this.symbolToolServiceB = symbolToolServiceB;
            this.chatLogService = chatLogService;

            // Subscribe to tool events
            if (symbolToolServiceA != null)
            {
                symbolToolServiceA.OnToolUsed += HandleToolUsed;
                symbolToolServiceA.OnPivotActivated += HandlePivotActivated;
                symbolToolServiceA.OnPivotDeactivated += HandlePivotDeactivated;
            }

            if (symbolToolServiceB != null)
            {
                symbolToolServiceB.OnToolUsed += HandleToolUsed;
                symbolToolServiceB.OnPivotActivated += HandlePivotActivated;
                symbolToolServiceB.OnPivotDeactivated += HandlePivotDeactivated;
            }
        }

        public void HandleToolInjection(GridID gridId, int x, int y, string toolType)
        {
            if (!splitGridService.IsInBounds(gridId, x, y)) return;

            var symbolToolService = gridId == GridID.GridA ? symbolToolServiceA : symbolToolServiceB;
            if (symbolToolService == null) return;

            // Apply the tool
            symbolToolService.TryPlaceSymbol(x, y, toolType);
            injectService.RemoveSelectedTool();
        }

        private bool IsValidInjectionPosition(GridID gridId, int x, int y)
        {
            // Check if the tile is revealed
            if (!splitGridService.IsTileRevealed(gridId, x, y)) return false;

            // Check if there's already a symbol
            if (!string.IsNullOrEmpty(splitGridService.GetSymbolAt(gridId, x, y))) return false;

            // Check if there's a virus
            if (splitGridService.IsFlaggedAsVirus(gridId, x, y)) return false;

            return true;
        }

        private void HandleToolUsed()
        {
            // Update both grid views
            if (gridViewA != null) gridViewA.RenderGrid();
            if (gridViewB != null) gridViewB.RenderGrid();
        }

        private void HandlePivotActivated()
        {
            // Update both grid views
            if (gridViewA != null) gridViewA.RenderGrid();
            if (gridViewB != null) gridViewB.RenderGrid();
        }

        private void HandlePivotDeactivated()
        {
            // Update both grid views
            if (gridViewA != null) gridViewA.RenderGrid();
            if (gridViewB != null) gridViewB.RenderGrid();
        }

        private void OnDestroy()
        {
            if (symbolToolServiceA != null)
            {
                symbolToolServiceA.OnToolUsed -= HandleToolUsed;
                symbolToolServiceA.OnPivotActivated -= HandlePivotActivated;
                symbolToolServiceA.OnPivotDeactivated -= HandlePivotDeactivated;
            }

            if (symbolToolServiceB != null)
            {
                symbolToolServiceB.OnToolUsed -= HandleToolUsed;
                symbolToolServiceB.OnPivotActivated -= HandlePivotActivated;
                symbolToolServiceB.OnPivotDeactivated -= HandlePivotDeactivated;
            }
        }
    }
} 