using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Configuration;
using NewGameplay.Strategies;
using NewGameplay.Utility;
using NewGameplay;
using NewGameplay.Models;
using NewGameplay.Enums;
using NewGameplay.Controllers;
namespace NewGameplay.Services
{
    public class GridService : IGridService
    {
        private readonly IGridStateService gridStateService;
        private readonly ISymbolPlacementService symbolPlacementService;
        private readonly IPurgeEffectService purgeEffectService;
        private readonly IVirusSpreadService virusSpreadService;

        private ITileElementService tileElementService;
        private IProgressTrackerService progressService;

        public ITileElementService TileElementService => tileElementService;

        private bool hasSpawnedInitialVirus = false;
        private Vector2Int? lastRevealedTile = null;
        private Vector2Int? previousRevealedTile = null;
        private bool gridInteractionLocked = true;
        private bool firstRevealPermitted = false;
        private bool roundSpawnsCompleted = false;

        public string GetSymbolAt(int x, int y) => gridStateService.GetSymbolAt(x, y);
        public bool IsTilePlayable(int x, int y) => gridStateService.IsTilePlayable(x, y);
        public bool IsInBounds(int x, int y) => gridStateService.IsInBounds(x, y);

        private TileState[,] tileStates;
        public event Action OnGridUpdated;
        public int GridSize => gridStateService.GridSize;
        public int GridWidth => gridStateService.GridWidth;
        public int GridHeight => gridStateService.GridHeight;
        public string[,] GridState => gridStateService.GridState;
        public bool[,] TilePlayable => gridStateService.TilePlayable;private GameOverController gameOverController;
        public void SetGameOverController(GameOverController controller)
        {
            this.gameOverController = controller;
        }

        public GridService(
            IGridStateService gridStateService,
            ISymbolPlacementService symbolPlacementService,
            IPurgeEffectService purgeEffectService,
            IVirusSpreadService virusSpreadService)
        {
            this.gridStateService = gridStateService;
            this.symbolPlacementService = symbolPlacementService;
            this.purgeEffectService = purgeEffectService;
            this.virusSpreadService = virusSpreadService;

            virusSpreadService.SetPurgeEffectService(purgeEffectService);
            gridStateService.OnGridStateChanged += HandleGridStateChanged;
            symbolPlacementService.OnSymbolPlaced += HandleSymbolPlaced;
            virusSpreadService.OnVirusSpread += HandleVirusSpread;
            virusSpreadService.SetGridService(this);
        }

        private GridViewNew gridView;
        public void SetGridView(GridViewNew view)
        {
            this.gridView = view;
        }

        public void SetTileElementService(ITileElementService service)
        {
            this.tileElementService = service;
        }

        public void RefreshTile(int x, int y)
        {
            gridView?.RefreshTileAt(x, y);
        }
        public void LockInteraction()
        {
            gridInteractionLocked = true;
        }
        public void UnlockInteraction()
        {
            Debug.Log("[GridService] Grid interaction unlocked.");
            gridInteractionLocked = false;
        }

        public void InitializeTileStates(int width, int height)
        {
            gridStateService.InitializeTileStates(width, height);
        }

        public void RevealTile(int x, int y, bool forceReveal = false)
        {
            if (gridInteractionLocked && !forceReveal) return;
            
            if (!IsInBounds(x, y) || IsTileRevealed(x, y)) return;

            bool isFirstReveal = true;
            for (int yy = 0; yy < GridHeight; yy++)
            {
                for (int xx = 0; xx < GridWidth; xx++)
                {
                    if (GetTileState(xx, yy) == TileState.Revealed)
                    {
                        isFirstReveal = false;
                        break;
                    }
                }
                if (!isFirstReveal) break;
            }

            if (!isFirstReveal && !IsAdjacentToLastRevealed(x, y) && !forceReveal)
            {
                Debug.Log($"[GridService] Tile ({x},{y}) is not adjacent to the last revealed tile. Reveal blocked.");
                return;
            }

            previousRevealedTile = lastRevealedTile;
            lastRevealedTile = new Vector2Int(x, y);

            // Get the symbol before revealing the tile
            string previousSymbol = gridStateService.GetSymbolAt(x, y);
            
            gridStateService.SetTileState(x, y, TileState.Revealed);
            RefreshTile(x, y);

            if (!hasSpawnedInitialVirus)
            {
                hasSpawnedInitialVirus = true;
                tileElementService?.TriggerElementEffectForFirstVirus();
            }

            tileElementService?.TriggerElementEffect(x, y);
            
            // Only increment fragments if the previous symbol was a data fragment
            if (previousSymbol == "DF")
            {
                progressService?.IncrementFragmentsFound();
            }
            
            virusSpreadService?.TrySpreadFromExistingViruses();
            
            // Play sound effects based on tile content
            var gameplayBootstrapper = UnityEngine.Object.FindFirstObjectByType<NewGameplayBootstrapper>();
            var soundService = gameplayBootstrapper?.ExposedSoundService;
            
            if (soundService != null)
            {
                // Play specific sounds based on tile content
                string symbol = GetSymbolAt(x, y);
                bool playedSpecialSound = false;
                
                if (symbol == "X")
                {
                    soundService.PlayWarning();
                    playedSpecialSound = true;
                }
                else if (symbol == "C$")
                {
                    soundService.PlayCodeShard();
                    playedSpecialSound = true;
                }
                else if (symbol == "DF")
                {
                    soundService.PlayDataFragment();
                    playedSpecialSound = true;
                }
                else if (virusSpreadService?.HasVirusAt(x, y) == true)
                {
                    soundService.PlayWarning();
                    playedSpecialSound = true;
                }
                else if (symbol == "∆:/run_PURGE.exe")
                {
                    // Special handling for purge - only play warning if there was a virus
                    if (previousSymbol == "X")
                    {
                        soundService.PlayWarning();
                    }
                    else
                    {
                        soundService.PlayTileClick();
                    }
                    playedSpecialSound = true;
                }
                
                // Only play tile click if no special sound was played
                if (!playedSpecialSound)
                {
                    soundService.PlayTileClick();
                }
            }
            
            if (!roundSpawnsCompleted)
            {
                tileElementService?.TriggerElementEffectForFirstVirus();

                SetTilePlayable(x, y, false);

                var bootstrapperRef = UnityEngine.Object.FindFirstObjectByType<NewGameplayBootstrapper>();
                bootstrapperRef?.ExposedDataFragmentService?.SpawnFragments(3);
                bootstrapperRef?.ExposedSoundService?.PlayTileClick();

                if (string.IsNullOrEmpty(GetSymbolAt(x, y)))
                {
                    SetTilePlayable(x, y, true);
                }

                roundSpawnsCompleted = true;
            }

        }

        public bool IsTileRevealed(int x, int y)
        {
            return IsInBounds(x, y) && gridStateService.GetTileState(x, y) == TileState.Revealed;
        }

        public bool IsFirstRevealDone()
        {
            for (int y = 0; y < GridHeight; y++)
                for (int x = 0; x < GridWidth; x++)
                    if (GetTileState(x, y) == TileState.Revealed)
                        return true;
            return false;
        }

        private bool IsAdjacentToLastRevealed(int x, int y)
        {
            if (!lastRevealedTile.HasValue) return false;
            Vector2Int last = lastRevealedTile.Value;
            if (previousRevealedTile.HasValue && previousRevealedTile.Value == new Vector2Int(x, y)) return false;
            return Mathf.Abs(x - last.x) + Mathf.Abs(y - last.y) == 1;
        }

        public bool CanRevealTile(int x, int y)
        {
            if (gridInteractionLocked) return false;
            if (!IsInBounds(x, y) || IsTileRevealed(x, y)) return false;
            if (!lastRevealedTile.HasValue) return firstRevealPermitted; // First reveal only allowed if permitted
            return IsAdjacentToLastRevealed(x, y);
        }

        public void SetEntropyService(IEntropyService entropyService)
        {
            virusSpreadService.SetEntropyService(entropyService);
        }

        public void TryPlaceSymbol(int x, int y)
        {
            var symbol = InjectServiceLocator.Service?.SelectedSymbol;
            if (string.IsNullOrEmpty(symbol)) return;

            symbolPlacementService.TryPlaceSymbol(x, y, symbol);
            InjectServiceLocator.Service?.ClearSelectedSymbol(symbol);
        }

        public void TryPlaceSymbol(int x, int y, string symbol)
        {
            symbolPlacementService.TryPlaceSymbol(x, y, symbol);
        }

        public void SetSymbol(int x, int y, string symbol)
        {
            string existingSymbol = gridStateService.GetSymbolAt(x, y);

            if (symbol == "DF")
            {
                gridStateService.SetSymbol(x, y, symbol);
                Debug.Log($"[GridService] SetSymbol called for DF at ({x},{y})");
                return;
            }

            if (existingSymbol == "DF")
            {
                Debug.Log("[GridService] Attempted to overwrite Data Fragment. Action blocked.");
                return;
            }

            if (symbol == "X" && existingSymbol == "DF")
            {
                Debug.Log("[GridService] Attempted to place virus on Data Fragment. Action blocked.");
                return;
            }

            gridStateService.SetSymbol(x, y, symbol);
            Debug.Log($"[GridService] SetSymbol placed symbol '{symbol}' at ({x},{y})");
        }

        public void SpreadVirus()
        {
            virusSpreadService.SpreadVirus();
        }

        public void ClearAllExceptViruses()
        {
            gridStateService.ClearAllExceptViruses();
        }

        public void ClearAllTiles()
        {
            gridStateService.ClearAllTiles(makePlayable: false);
            lastRevealedTile = null;
            previousRevealedTile = null;

        }

        private void HandleGridStateChanged()
        {
            OnGridUpdated?.Invoke();
        }

        private void HandleSymbolPlaced()
        {
            OnGridUpdated?.Invoke();
        }

        private void HandleVirusSpread()
        {
            OnGridUpdated?.Invoke();
        }
        public Vector2Int? GetLastRevealedTile()
        {
            return lastRevealedTile;
        }

        public void TriggerGridUpdate()
        {
            Debug.Log("[GridService] Manual grid update triggered");
            OnGridUpdated?.Invoke();
        }

        public List<Vector2Int> GetAllEmptyTilePositions()
        {
            var emptyPositions = new List<Vector2Int>();

            for (int y = 0; y < gridStateService.GridHeight; y++)
            {
                for (int x = 0; x < gridStateService.GridWidth; x++)
                {
                    if (string.IsNullOrEmpty(gridStateService.GetSymbolAt(x, y)) && gridStateService.IsTilePlayable(x, y))
                    {
                        emptyPositions.Add(new Vector2Int(x, y));
                    }
                }

            }

            return emptyPositions;
        }

        public void SetTilePlayable(int x, int y, bool playable)
        {
            gridStateService.SetTilePlayable(x, y, playable);
        }

        public TileState GetTileState(int x, int y)
        {
            return IsInBounds(x, y) ? gridStateService.GetTileState(x, y) : TileState.Hidden;
        }

        public void SetProgressService(IProgressTrackerService service)
        {
            this.progressService = service;
        }

        public void SetFirstRevealPermitted(bool value)
        {
            firstRevealPermitted = value;
        }
        public void ResetRoundSpawns()
        {
            roundSpawnsCompleted = false;
        }

    }
}
