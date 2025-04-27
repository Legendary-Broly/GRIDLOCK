using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Utility;

namespace NewGameplay.Services
{
    public class ExtractService : IExtractService
    {
        private readonly IGridService gridService;
        private readonly IEntropyService entropyService;
        private readonly IScoreService scoreService;
        private readonly IProgressTrackerService progressService;
        private readonly System.Random rng = new();
        private readonly IDataFragmentService dataFragmentService;

        public event System.Action onGridUpdated;
        public int CurrentScore { get; private set; }

        public ExtractService(IGridService gridService, IEntropyService entropyService, IScoreService scoreService, IProgressTrackerService progressService, IDataFragmentService dataFragmentService)
        {
            this.gridService = gridService;
            this.entropyService = entropyService;
            this.scoreService = scoreService;
            this.progressService = progressService;
            this.dataFragmentService = dataFragmentService;
        }

        public void ExtractGrid()
        {
            if (dataFragmentService.GetFragmentPosition().HasValue && !dataFragmentService.IsFragmentFullySurrounded())
            {
                Debug.Log("[ExtractService] Data Fragment exists but is not fully surrounded. Extraction blocked.");
                return;
            }

            Debug.Log($"[ExtractService] Starting extraction. Current entropy: {entropyService.EntropyPercent}%");
            var matches = GridMatchEvaluator.FindMatches(gridService);
            Debug.Log($"[ExtractService] Found {matches.Count} match groups");

            int totalScore = 0;
            foreach (var match in matches)
            {
                if (match.Count == 0) continue;

                string symbol = gridService.GetSymbolAt(match[0].x, match[0].y);
                Debug.Log($"[ExtractService] Processing match: Symbol '{symbol}' -> Size {match.Count}");

                totalScore += SymbolEffectProcessor.Apply(match, gridService, entropyService);
                Debug.Log($"[ExtractService] After match processing, entropy: {entropyService.EntropyPercent}%");
            }

            List<Vector2Int> allMatchedTiles = matches.SelectMany(m => m).ToList();
            totalScore += SymbolEffectProcessor.ApplyUnmatchedSymbols(gridService, allMatchedTiles, entropyService);
            Debug.Log($"[ExtractService] After unmatched symbols, entropy: {entropyService.EntropyPercent}%");

            scoreService.AddScore(totalScore);
            progressService.ApplyScore(totalScore);
            CurrentScore = totalScore;

            // Check if we should spawn a data fragment
            if (progressService.HasMetGoal())
            {
                dataFragmentService.SpawnFragment();
            }

            // Process loops only
            SymbolEffectProcessor.ProcessAllLoops(gridService);

            // Update grid to show effects
            onGridUpdated?.Invoke();
            
            // If entropy is at 100%, let it reset before applying passive penalty
            if (entropyService.EntropyPercent >= 100)
            {
                Debug.Log($"[ExtractService] Entropy at 100%, allowing reset before passive penalty");
                // Apply a small increase to trigger the reset
                entropyService.Increase(1);
            }
            
            Debug.Log($"[ExtractService] Before passive entropy penalty, entropy: {entropyService.EntropyPercent}%");
            SymbolEffectProcessor.ApplyPassiveEntropyPenalty(gridService, entropyService);
            Debug.Log($"[ExtractService] After passive entropy penalty, entropy: {entropyService.EntropyPercent}%");

            // Clear matched symbols
            foreach (var match in matches)
            {
                foreach (var pos in match)
                {
                    gridService.SetSymbol(pos.x, pos.y, null);
                }
            }

            // Clear all non-virus tiles
            gridService.ClearAllExceptViruses();

            // Final grid update
            onGridUpdated?.Invoke();
            Debug.Log($"[ExtractService] Extraction complete. Final entropy: {entropyService.EntropyPercent}%");
        }
    }
}