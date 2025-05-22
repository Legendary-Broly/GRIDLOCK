using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NewGameplay.Enums;
using NewGameplay.Interfaces;
using NewGameplay.Models;
using NewGameplay.ScriptableObjects;
using NewGameplay.Services;
using NewGameplay.Controllers;

namespace NewGameplay.Services
{
    public class TileElementService : ITileElementService
    {
        private TileElementType[,] gridElements;
        private int gridWidth;
        private int gridHeight;
        private readonly List<TileElementSO> elementConfigs;
        private readonly List<TileElementType> dynamicSpawnElements = new()
        {
            TileElementType.CodeShard,
            TileElementType.SystemIntegrityIncrease,
            TileElementType.ToolRefresh
        };

        private readonly List<Vector2Int> pendingJunkPiles = new();

        private IGridService gridService;
        private IInjectService injectService;
        private ISystemIntegrityService systemIntegrityService;
        private ICodeShardTracker codeShardTracker;
        private IChatLogService chatLogService;
        private IProgressTrackerService progressService;
        private IVirusService virusService;
        private IDataFragmentService dataFragmentService;
        private PayloadManager payloadManager;
        private InjectController injectController;
        private bool wirelessUploadSuppressed = false;

        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;

        public TileElementService(int width, int height, List<TileElementSO> configs)
        {
            gridWidth = width;
            gridHeight = height;
            elementConfigs = configs;
        }

        public void SetPayloadManager(PayloadManager manager) => payloadManager = manager;
        public void SetSystemIntegrityService(ISystemIntegrityService s) => systemIntegrityService = s;
        public void SetGridService(IGridService s) => gridService = s;
        public void SetCodeShardTracker(ICodeShardTracker tracker) => codeShardTracker = tracker;
        public void SetInjectService(IInjectService s) => injectService = s;
        public void SetInjectController(InjectController controller) => injectController = controller;
        public void SetChatLogService(IChatLogService s) => chatLogService = s;
        public void SetVirusService(IVirusService s) => virusService = s;
        public void SetDataFragmentService(IDataFragmentService s) => dataFragmentService = s;
        public void SetProgressTrackerService(IProgressTrackerService s) => progressService = s;

        public void ResizeGrid(int width, int height)
        {
            gridWidth = width;
            gridHeight = height;
            gridElements = new TileElementType[gridWidth, gridHeight];
            GenerateElements();
        }

        public void GenerateElements()
        {
            for (int y = 0; y < gridHeight; y++)
                for (int x = 0; x < gridWidth; x++)
                    gridElements[x, y] = TileElementType.Empty;

            var candidates = new List<Vector2Int>();
            for (int y = 0; y < gridHeight; y++)
                for (int x = 0; x < gridWidth; x++)
                {
                    if (!gridService.IsInBounds(x, y)) continue;
                    if (!string.IsNullOrEmpty(gridService.GetSymbolAt(x, y))) continue;
                    if (gridService.GetTileState(x, y) != TileState.Hidden) continue;
                    candidates.Add(new Vector2Int(x, y));
                }

            candidates = candidates.OrderBy(_ => Random.value).ToList();

            int tilesPer = Mathf.FloorToInt(candidates.Count * 0.1f);
            int index = 0;

            foreach (var type in dynamicSpawnElements)
            {
                for (int i = 0; i < tilesPer && index < candidates.Count; i++, index++)
                    gridElements[candidates[index].x, candidates[index].y] = type;
            }
        }

        public void TriggerElementEffect(int x, int y)
        {
            TileElementType type = gridElements[x, y];

            if (virusService.HasVirusAt(x, y)) type = TileElementType.Virus;
            else if (dataFragmentService.IsFragmentAt(new Vector2Int(x, y))) type = TileElementType.DataFragment;

            var config = elementConfigs.FirstOrDefault(e => e.elementType == type);
            if (config == null && type != TileElementType.Virus && type != TileElementType.DataFragment) return;

            switch (type)
            {
                case TileElementType.CodeShard:
                    codeShardTracker?.AddShard();
                    break;

                case TileElementType.SystemIntegrityIncrease:
                    float restore = (100f - systemIntegrityService.CurrentIntegrity) * 0.25f;
                    systemIntegrityService.Increase(restore);
                    break;

                case TileElementType.ToolRefresh:
                    injectService?.ResetForNewRound();
                    injectController?.RefreshUI();
                    break;

                case TileElementType.Warp:
                    RevealRandomUninfectedTile();
                    break;

                case TileElementType.FlagPop:
                    FlagAdjacentViruses(x, y);
                    break;

                case TileElementType.JunkPile:
                    pendingJunkPiles.Add(new Vector2Int(x, y));
                    break;

                case TileElementType.CodeShardPlus:
                    codeShardTracker?.AddShard();
                    codeShardTracker?.AddShard();
                    break;

                case TileElementType.SystemIntegrityIncreasePlus:
                    float boost = (100f - systemIntegrityService.CurrentIntegrity) * 0.5f;
                    systemIntegrityService.Increase(boost);
                    break;
            }

            TryWirelessAutoReveal(type, x, y);
            chatLogService?.LogTileElementReveal(type);
        }

        public void OnTileRevealed(int x, int y)
        {
            foreach (var pos in pendingJunkPiles.ToList())
                HandleJunkpileTransform(pos.x, pos.y);

            pendingJunkPiles.Clear();
        }

        private void HandleJunkpileTransform(int x, int y)
        {
            int outcome = Random.Range(0, 4);
            switch (outcome)
            {
                case 0: // CodeShard
                    gridElements[x, y] = TileElementType.CodeShard;
                    TriggerElementEffect(x, y);
                    break;
                case 1: // Virus
                    gridElements[x, y] = TileElementType.Empty;
                    gridService.SetSymbol(x, y, "X");
                    systemIntegrityService.Decrease(25f);
                    chatLogService?.LogVirusReveal();
                    break;
                case 2: // DataFragment
                    gridElements[x, y] = TileElementType.Empty;
                    gridService.SetSymbol(x, y, "DATA");
                    dataFragmentService?.RegisterFragmentAt(x, y);
                    progressService?.NotifyFragmentRevealed(x, y);
                    chatLogService?.LogDataFragmentReveal();
                    break;
                case 3: // Nothing
                    gridElements[x, y] = TileElementType.Empty;
                    gridService.SetSymbol(x, y, "");
                    break;
            }

            gridService.TriggerGridUpdate();
        }

        private void RevealRandomUninfectedTile()
        {
            var candidates = new List<Vector2Int>();
            for (int y = 0; y < gridHeight; y++)
                for (int x = 0; x < gridWidth; x++)
                    if (!gridService.IsTileRevealed(x, y) && !virusService.HasVirusAt(x, y))
                        candidates.Add(new Vector2Int(x, y));

            if (candidates.Count > 0)
            {
                var chosen = candidates[Random.Range(0, candidates.Count)];
                gridService.RevealTile(chosen.x, chosen.y, true);
            }
        }

        private void FlagAdjacentViruses(int x, int y)
        {
            var dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in dirs)
            {
                int nx = x + dir.x, ny = y + dir.y;
                if (gridService.IsInBounds(nx, ny) && !gridService.IsTileRevealed(nx, ny) && virusService.HasVirusAt(nx, ny))
                    gridService.SetVirusFlag(nx, ny, true);
            }
            gridService.TriggerGridUpdate();
        }

        private void TryWirelessAutoReveal(TileElementType type, int x, int y)
        {
            if (wirelessUploadSuppressed || payloadManager == null || !payloadManager.ShouldRevealSimilarTile()) return;

            if (Random.value <= 0.5f)
            {
                wirelessUploadSuppressed = true;
                var match = FindNearestMatchingTile(x, y, type);
                if (match.HasValue)
                    gridService.RevealTile(match.Value.x, match.Value.y, true);
                wirelessUploadSuppressed = false;
            }
        }

        private Vector2Int? FindNearestMatchingTile(int ox, int oy, TileElementType match)
        {
            float closest = float.MaxValue;
            Vector2Int? result = null;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if ((x == ox && y == oy) || gridService.IsTileRevealed(x, y)) continue;

                    bool isMatch = match switch
                    {
                        TileElementType.Virus => virusService.HasVirusAt(x, y),
                        TileElementType.DataFragment => dataFragmentService.IsFragmentAt(new Vector2Int(x, y)),
                        _ => gridElements[x, y] == match
                    };

                    if (!isMatch) continue;

                    float dist = Vector2Int.Distance(new Vector2Int(ox, oy), new Vector2Int(x, y));
                    if (dist < closest)
                    {
                        closest = dist;
                        result = new Vector2Int(x, y);
                    }
                }
            }

            return result;
        }

        public TileElementType GetElementAt(int x, int y) => gridElements[x, y];
        public TileElementSO GetElementSOAt(int x, int y) => GetElementSO(gridElements[x, y]);
        public TileElementSO GetElementSO(TileElementType type) => elementConfigs.FirstOrDefault(e => e.elementType == type);

        public void AddManualElement(TileElementType type)
        {
            var candidates = gridService.GetAllEmptyTilePositions()
                .Where(pos => gridElements[pos.x, pos.y] == TileElementType.Empty)
                .ToList();

            if (candidates.Count > 0)
            {
                var chosen = candidates[Random.Range(0, candidates.Count)];
                gridElements[chosen.x, chosen.y] = type;
            }
        }

        public void AddToSpawnPool(TileElementType type)
        {
            if (!dynamicSpawnElements.Contains(type))
                dynamicSpawnElements.Add(type);
        }
        private MonoBehaviour gameRunner;

        public void SetGameRunner(MonoBehaviour runner)
        {
            this.gameRunner = runner;
        }
    }
}
