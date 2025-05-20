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
        private PayloadManager payloadManager;
        private MonoBehaviour gameRunner;
        private IDataFragmentService dataFragmentService;
        private bool wirelessUploadSuppressed = false;

        public TileElementService(int width, int height, List<TileElementSO> configs)
        {
            this.gridWidth = width;
            this.gridHeight = height;
            this.elementConfigs = configs;
        }
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;

        public void SetPayloadManager(PayloadManager manager) => payloadManager = manager;
        public void SetSystemIntegrityService(ISystemIntegrityService s) => systemIntegrityService = s;
        public void SetGridService(IGridService service) => gridService = service;
        public void SetCodeShardTracker(ICodeShardTracker tracker) => codeShardTracker = tracker;
        public void SetInjectService(IInjectService service) => injectService = service;
        public void SetChatLogService(IChatLogService service) => chatLogService = service;
        public void SetVirusService(IVirusService service) => virusService = service;
        public void SetGameRunner(MonoBehaviour runner) => gameRunner = runner;
        public TileElementType GetElementAt(int x, int y) => gridElements[x, y];
        public TileElementSO GetElementSO(TileElementType type) => elementConfigs.FirstOrDefault(e => e.elementType == type);
        public TileElementSO GetElementSOAt(int x, int y) => GetElementSO(gridElements[x, y]);

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

            List<Vector2Int> candidates = new();
            for (int y = 0; y < gridHeight; y++)
                for (int x = 0; x < gridWidth; x++)
                {
                    if (!gridService.IsInBounds(x, y)) continue;
                    if (!string.IsNullOrEmpty(gridService.GetSymbolAt(x, y))) continue;
                    if (gridService.GetTileState(x, y) != TileState.Hidden) continue;
                    candidates.Add(new Vector2Int(x, y));
                }

            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }

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

            if (virusService.HasVirusAt(x, y))
                type = TileElementType.Virus;
            else if (dataFragmentService.IsFragmentAt(new Vector2Int(x, y)))
                type = TileElementType.DataFragment;

            var config = elementConfigs.FirstOrDefault(e => e.elementType == type);
            if (config == null && type != TileElementType.Virus && type != TileElementType.DataFragment) return;

            Debug.Log($"[TriggerElementEffect] ({x},{y}) → {type}");

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
                    var injectController = UnityEngine.Object.FindFirstObjectByType<InjectController>();
                    injectController?.RefreshUI();
                    break;

                case TileElementType.Warp:
                    List<Vector2Int> candidates = new();
                    for (int gridY = 0; gridY < gridService.GridHeight; gridY++)
                    {
                        for (int gridX = 0; gridX < gridService.GridWidth; gridX++)
                        {
                            if (!gridService.IsTileRevealed(gridX, gridY) && !virusService.HasVirusAt(gridX, gridY))
                                candidates.Add(new Vector2Int(gridX, gridY));
                        }
                    }
                    if (candidates.Count > 0)
                    {
                        var target = candidates[Random.Range(0, candidates.Count)];
                        Debug.Log($"[Warp] Revealing random non-virus tile at ({target.x},{target.y})");
                        gridService.RevealTile(target.x, target.y, true);
                    }
                    else
                        Debug.Log("[Warp] No valid tiles to reveal.");
                    break;

                case TileElementType.FlagPop:
                    Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                    foreach (var dir in dirs)
                    {
                        int nx = x + dir.x, ny = y + dir.y;
                        if (!gridService.IsInBounds(nx, ny) || gridService.IsTileRevealed(nx, ny)) continue;
                        if (virusService.HasVirusAt(nx, ny))
                            gridService.SetVirusFlag(nx, ny, true);
                    }
                    gridService.TriggerGridUpdate();
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

            // ✅ Wireless Upload payload: 50% chance to reveal nearest same element
            if (!wirelessUploadSuppressed && payloadManager != null && payloadManager.ShouldRevealSimilarTile())
            {
                float roll = UnityEngine.Random.value;
                Debug.Log($"[WirelessUpload] Payload active — attempting auto-reveal for {type} @ ({x},{y}) | Roll: {roll:F2}");

                if (roll <= 0.5f)
                {
                    wirelessUploadSuppressed = true;
                    var nearest = FindNearestMatchingTile(x, y, type);

                    if (nearest.HasValue)
                    {
                        Debug.Log($"[WirelessUpload] Success — revealed nearest {type} at {nearest.Value}");
                        gridService.RevealTile(nearest.Value.x, nearest.Value.y, true);
                    }
                    else
                    {
                        Debug.Log($"[WirelessUpload] No matching unrevealed {type} found.");
                    }

                    wirelessUploadSuppressed = false;
                }
                else
                {
                    Debug.Log($"[WirelessUpload] Failed — 50% chance did not trigger.");
                }
            }

            chatLogService?.LogTileElementReveal(type);
        }

        public void OnTileRevealed(int x, int y)
        {
            if (pendingJunkPiles.Count == 0) return;


            foreach (var pos in pendingJunkPiles.ToList())
            {
                if (gameRunner != null)
                    gameRunner.StartCoroutine(HandleJunkpileTransform(pos.x, pos.y));
            }
            pendingJunkPiles.Clear();
        }

        private IEnumerator HandleJunkpileTransform(int x, int y)
        {
            yield return new WaitForSeconds(0.4f);

            int outcome = UnityEngine.Random.Range(0, 4);


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
                    gridService.TriggerGridUpdate();
                    break;

                case 2: // Data Fragment
                    gridElements[x, y] = TileElementType.Empty;
                    gridService.SetSymbol(x, y, "DATA");
                    dataFragmentService?.RegisterFragmentAt(x, y);
                    progressService?.NotifyFragmentRevealed(x, y);
                    chatLogService?.LogDataFragmentReveal();
                    progressService?.NotifyFragmentRevealed();
                    break;

                case 3: // Nothing
                    gridElements[x, y] = TileElementType.Empty;
                    gridService.SetSymbol(x, y, "");
                    break;
            }

            gridService.TriggerGridUpdate();
        }

        public void AddManualElement(TileElementType type)
        {
            var candidates = gridService.GetAllEmptyTilePositions()
                .Where(pos => gridElements[pos.x, pos.y] == TileElementType.Empty).ToList();

            if (candidates.Count == 0) return;
            var chosen = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            gridElements[chosen.x, chosen.y] = type;

        }

        public void AddToSpawnPool(TileElementType type)
        {
            if (!dynamicSpawnElements.Contains(type))
            {
                dynamicSpawnElements.Add(type);

            }
        }
        private Vector2Int? FindNearestMatchingTile(int originX, int originY, TileElementType match)
        {
            float closestDist = float.MaxValue;
            Vector2Int? best = null;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if ((x == originX && y == originY) || gridService.IsTileRevealed(x, y))
                        continue;

                    bool isMatch = false;

                    if (match == TileElementType.Virus && virusService.HasVirusAt(x, y))
                        isMatch = true;
                    else if (match == TileElementType.DataFragment && dataFragmentService.IsFragmentAt(new Vector2Int(x, y)))
                        isMatch = true;
                    else if (gridElements[x, y] == match)
                        isMatch = true;

                    if (!isMatch) continue;

                    float dist = Vector2Int.Distance(new Vector2Int(originX, originY), new Vector2Int(x, y));
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        best = new Vector2Int(x, y);
                    }
                }
            }

            return best;
        }

        public void SetDataFragmentService(IDataFragmentService service)
        {
            dataFragmentService = service;
        }  
        public void SetProgressTrackerService(IProgressTrackerService service)
        {
            progressService = service;
        }
    }
}
