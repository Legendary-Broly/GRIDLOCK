using System;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Utility;
using NewGameplay.Models;
using System.Collections.Generic;
using System.Linq;


namespace NewGameplay.Services
{
    public class SymbolPlacementService : ISymbolPlacementService
    {
        private readonly IGridStateService gridStateService;
        private IPurgeEffectService purgeEffectService;
        private IMutationEffectService mutationEffectService;
        private readonly IEntropyService entropyService;
        private ITileElementService tileElementService;
        private IVirusSpreadService virusSpreadService;
        private GridViewNew gridView;
        private IGridService gridService;
        private bool initialVirusSpawned = false;
        private Vector2Int? lastPurgedPosition = null;
        
        public event Action OnSymbolPlaced;

        public SymbolPlacementService(
            IGridStateService gridStateService,
            IPurgeEffectService purgeEffectService,
            IMutationEffectService mutationEffectService = null,
            IEntropyService entropyService = null,
            ITileElementService tileElementService = null,
            IGridService gridService = null)
        {
            this.gridStateService = gridStateService;
            this.purgeEffectService = purgeEffectService;
            this.mutationEffectService = mutationEffectService;
            this.entropyService = entropyService;
            this.tileElementService = tileElementService;
            this.gridService = gridService;
        }
        public void SetGridView(GridViewNew view)
        {
            gridView = view;
        }

        public void SetTileElementService(ITileElementService service)
        {
            tileElementService = service;
        }

        public void SetVirusSpreadService(IVirusSpreadService service)
        {
            virusSpreadService = service;
        }

        public void SetPurgeEffectService(IPurgeEffectService purgeEffectService)
        {
            this.purgeEffectService = purgeEffectService;
        }
        public void SetGridService(IGridService gridService)
        {
            this.gridService = gridService;
        }

        public void TryPlaceSymbol(int x, int y, string symbol)
        {
            Debug.Log($"[SymbolPlacement] Attempting to place symbol '{symbol}' at ({x},{y})");
            lastPurgedPosition = null;  // Reset at start of new placement
            
            if (string.IsNullOrEmpty(symbol))
            {
                Debug.Log("[SymbolPlacement] Symbol is null or empty, returning");
                return;
            }

            // Check playability unless it's a purge
            if (!gridStateService.IsTilePlayable(x, y) && symbol != "∆")
            {
                Debug.Log($"[SymbolPlacement] Tile ({x},{y}) is not playable and symbol is not purge, returning");
                return;
            }

            // Debug current state
            string currentSymbol = gridStateService.GetSymbolAt(x, y);
            bool hasVirus = virusSpreadService?.HasVirusAt(x, y) ?? false;
            Debug.Log($"[SymbolPlacement] Current state at ({x},{y}): Symbol='{currentSymbol}', HasVirus={hasVirus}");

            // Trigger tile element effects first
            if (tileElementService != null)
            {
                var elementType = tileElementService.GetElementAt(x, y);
                if (elementType != NewGameplay.Enums.TileElementType.Empty)
                {
                    Debug.Log($"[SymbolPlacement] Triggering tile element effect at ({x},{y}) before placing symbol");
                    tileElementService.TriggerElementEffect(x, y);
                }
            }

            // Special case: purge placed on virus
            if (symbol == "∆" && virusSpreadService?.HasVirusAt(x, y) == true)
            {
                Debug.Log($"[Purge] Purge removing virus at ({x},{y})");
                virusSpreadService.RemoveVirusAt(x, y);
                gridStateService.SetSymbol(x, y, null);  // Clear visual
                gridStateService.SetTilePlayable(x, y, false);
                lastPurgedPosition = new Vector2Int(x, y);  // Track this position

                // Reveal the tile if it's adjacent to the last revealed tile
                if (gridService.CanRevealTile(x, y))
                {
                    Debug.Log($"[Purge] Revealing tile at ({x},{y}) after virus removal");
                    gridService.RevealTile(x, y);
                }

                Debug.Log($"[Purge] After removal - Symbol: '{gridStateService.GetSymbolAt(x, y)}', HasVirus: {virusSpreadService.HasVirusAt(x, y)}");
            }
            else
            {
                Debug.Log($"[SymbolPlacement] Placing symbol '{symbol}' normally at ({x},{y})");
                // Place the symbol normally
                gridStateService.SetSymbol(x, y, symbol);
                gridStateService.SetTilePlayable(x, y, false);

                // Handle initial virus spawn (only for non-purge symbols)
                if (!initialVirusSpawned && symbol != "∆")
                {
                    Vector2Int? nestPos = tileElementService.GetVirusNestPosition();
                    if (nestPos.HasValue)
                    {
                        var pos = nestPos.Value;
                        gridService.SetSymbol(pos.x, pos.y, "X");
                        gridService.SetTilePlayable(pos.x, pos.y, false);
                        Debug.Log($"[VirusSpawn] Initial virus spawned at ({pos.x},{pos.y})");
                        initialVirusSpawned = true;
                    }
                }
            }

            // Spread virus after every placement
            virusSpreadService.TrySpreadFromExistingViruses();
            SymbolEffectProcessor.ApplySymbolEffectAtPlacement(symbol, x, y, gridService, entropyService, tileElementService);
            // Notify listeners
            OnSymbolPlaced?.Invoke();
            Debug.Log($"[SymbolPlacement] Final state at ({x},{y}): Symbol='{gridStateService.GetSymbolAt(x, y)}', HasVirus={virusSpreadService.HasVirusAt(x, y)}");
        }

        private void RevealRandomHiddenTiles(int centerX, int centerY, int count)
        {
            if (gridStateService == null)
            {
                Debug.LogError("[RevealRandomHiddenTiles] GridStateService is NULL!");
                return;
            }

            List<(int x, int y)> hiddenTiles = new List<(int x, int y)>();

            for (int y = 0; y < gridStateService.GridHeight; y++)
            {
                for (int x = 0; x < gridStateService.GridWidth; x++)
                {
                    if (gridStateService.GetTileState(x, y) == TileState.Hidden)
                    {
                        hiddenTiles.Add((x, y));
                    }
                }
            }

            hiddenTiles = hiddenTiles.OrderBy(t => UnityEngine.Random.value).ToList();

            int revealed = 0;
            foreach (var tile in hiddenTiles)
            {
                if (revealed >= count) break;
                gridStateService.SetTileState(tile.x, tile.y, TileState.Revealed);

                // 🔥 Refresh the tile visually immediately
                gridView?.RefreshTileAt(tile.x, tile.y);
               
                Canvas.ForceUpdateCanvases();

                revealed++;
            }
        }

        public bool IsAdjacentToSymbol(int x, int y, string target)
        {
            Vector2Int[] directions = new[] {
                new Vector2Int(1, 0), new Vector2Int(-1, 0),
                new Vector2Int(0, 1), new Vector2Int(0, -1)
            };

            foreach (var d in directions)
            {
                int nx = x + d.x;
                int ny = y + d.y;
                if (gridStateService.IsInBounds(nx, ny) && gridStateService.GetSymbolAt(nx, ny) == target)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
