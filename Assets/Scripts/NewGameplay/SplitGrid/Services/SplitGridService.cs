using UnityEngine;
using System.Collections.Generic;
using NewGameplay.SplitGrid.Interfaces;
using NewGameplay.SplitGrid.Services;
using NewGameplay.SplitGrid.Views;
using NewGameplay.SplitGrid.Data;
using NewGameplay.SplitGrid.Controllers;
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


namespace NewGameplay.SplitGrid.Services
{
    public class SplitGridService : ISplitGridService
    {
        private readonly IGridService gridA;
        private readonly IGridService gridB;
        private readonly IGridStateService gridStateServiceA;
        private readonly IGridStateService gridStateServiceB;
        private readonly IVirusService virusServiceA;
        private readonly IVirusService virusServiceB;

        public event Action OnGridsUpdated;

        public SplitGridService(
            IGridStateService gridStateServiceA,
            IGridStateService gridStateServiceB,
            IVirusService virusServiceA,
            IVirusService virusServiceB)
        {
            this.gridStateServiceA = gridStateServiceA;
            this.gridStateServiceB = gridStateServiceB;
            this.virusServiceA = virusServiceA;
            this.virusServiceB = virusServiceB;

            // Initialize grid services
            gridA = new GridService(gridStateServiceA, virusServiceA, null);
            gridB = new GridService(gridStateServiceB, virusServiceB, null);

            // Subscribe to grid updates
            gridA.OnGridUpdated += () => OnGridsUpdated?.Invoke();
            gridB.OnGridUpdated += () => OnGridsUpdated?.Invoke();
        }

        public IGridService GetGrid(GridID gridId) => gridId switch
        {
            GridID.GridA => gridA,
            GridID.GridB => gridB,
            _ => throw new ArgumentException($"Invalid grid ID: {gridId}")
        };

        public void InitializeGrids(int width, int height)
        {
            gridStateServiceA.SetGridSize(width, height);
            gridStateServiceB.SetGridSize(width, height);
        }

        public void ClearAllGrids()
        {
            gridA.ClearAllTiles();
            gridB.ClearAllTiles();
        }

        public void ClearAllExceptViruses()
        {
            // Clear Grid A except viruses
            for (int y = 0; y < gridA.GridHeight; y++)
                for (int x = 0; x < gridA.GridWidth; x++)
                    if (!virusServiceA.HasVirusAt(x, y))
                        gridA.SetSymbol(x, y, "");

            // Clear Grid B except viruses
            for (int y = 0; y < gridB.GridHeight; y++)
                for (int x = 0; x < gridB.GridWidth; x++)
                    if (!virusServiceB.HasVirusAt(x, y))
                        gridB.SetSymbol(x, y, "");
        }

        public void RevealTile(GridID gridId, int x, int y, bool forceReveal = false)
        {
            GetGrid(gridId).RevealTile(x, y, forceReveal);
        }

        public bool IsTileRevealed(GridID gridId, int x, int y)
        {
            return GetGrid(gridId).IsTileRevealed(x, y);
        }

        public bool CanRevealTile(GridID gridId, int x, int y)
        {
            return GetGrid(gridId).CanRevealTile(x, y);
        }

        public void SetSymbol(GridID gridId, int x, int y, string symbol)
        {
            GetGrid(gridId).SetSymbol(x, y, symbol);
        }

        public string GetSymbolAt(GridID gridId, int x, int y)
        {
            return GetGrid(gridId).GetSymbolAt(x, y);
        }

        public void SetTileState(GridID gridId, int x, int y, TileState state)
        {
            GetGrid(gridId).SetTileState(x, y, state);
        }

        public TileState GetTileState(GridID gridId, int x, int y)
        {
            return GetGrid(gridId).GetTileState(x, y);
        }

        public bool IsInBounds(GridID gridId, int x, int y)
        {
            return GetGrid(gridId).IsInBounds(x, y);
        }

        public int GetGridWidth(GridID gridId)
        {
            return GetGrid(gridId).GridWidth;
        }

        public int GetGridHeight(GridID gridId)
        {
            return GetGrid(gridId).GridHeight;
        }

        public void LockInteraction(GridID gridId)
        {
            GetGrid(gridId).LockInteraction();
        }

        public void UnlockInteraction(GridID gridId)
        {
            GetGrid(gridId).UnlockInteraction();
        }

        public void SetVirusFlag(GridID gridId, int x, int y, bool flagged)
        {
            GetGrid(gridId).SetVirusFlag(x, y, flagged);
        }

        public bool IsFlaggedAsVirus(GridID gridId, int x, int y)
        {
            return GetGrid(gridId).IsFlaggedAsVirus(x, y);
        }

        public bool CanUseVirusFlag(GridID gridId)
        {
            return GetGrid(gridId).CanUseVirusFlag();
        }

        public void DisableVirusFlag(GridID gridId)
        {
            GetGrid(gridId).DisableVirusFlag();
        }

        public void TriggerGridUpdate(GridID gridId)
        {
            GetGrid(gridId).TriggerGridUpdate();
        }

        public Vector2Int? GetLastRevealedTile(GridID gridId)
        {
            return GetGrid(gridId).GetLastRevealedTile();
        }

        public void SetLastRevealedTile(GridID gridId, Vector2Int pos)
        {
            GetGrid(gridId).SetLastRevealedTile(pos);
        }

        public void SetTileElementService(GridID gridId, ITileElementService service)
        {
            if (GetGrid(gridId) is GridService concreteGrid)
            {
                concreteGrid.SetTileElementService(service);
            }
        }

        public void SetSymbolToolService(GridID gridId, ISymbolToolService service)
        {
            GetGrid(gridId).SetSymbolToolService(service);
        }

        public void SetGridSize(GridID gridId, int width, int height)
        {
            if (GetGrid(gridId) is GridService concreteGrid)
            {
                concreteGrid.InitializeTileStates(width, height);
            }
        }

        public void ClearGrid(GridID gridId)
        {
            GetGrid(gridId).ClearAllTiles();
        }

        public void ResetGrids()
        {
            gridA.ClearAllTiles();
            gridB.ClearAllTiles();
        }

        public void EnableVirusFlag(GridID gridId)
        {
            GetGrid(gridId).EnableVirusFlag();
        }

        public IGridService GetGridService(GridID gridId)
        {
            return GetGrid(gridId);
        }

        public void SetVirusService(GridID gridId, IVirusService service)
        {
            // No-op: GridService does not support changing the virus service after construction.
        }

        public void SetDataFragmentService(GridID gridId, IDataFragmentService service)
        {
            if (GetGrid(gridId) is GridService concreteGrid)
            {
                concreteGrid.SetDataFragmentService(service);
            }
        }

        public void RevealTile(GridID gridId, int x, int y)
        {
            GetGrid(gridId).RevealTile(x, y);
        }
        public void HandleVirusFlag(GridID gridId, int x, int y, IChatLogService chatLogService)
        {
            bool isCorrect = false;

            if (gridId == GridID.GridA)
                isCorrect = virusServiceA.HasVirusAt(x, y);
            else if (gridId == GridID.GridB)
                isCorrect = virusServiceB.HasVirusAt(x, y);

            SetVirusFlag(gridId, x, y, isCorrect);

            if (isCorrect)
                chatLogService?.LogCorrectFlag();
            else
                chatLogService?.LogIncorrectFlag();

            DisableVirusFlag(gridId);
        }
    }
} 