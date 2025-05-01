using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NewGameplay.Interfaces;
using NewGameplay.Utility;
using NewGameplay.Models;
using NewGameplay.Services;

namespace NewGameplay.Services
{
    public class ExtractService : IExtractService
    {
        private readonly IGridService gridService;
        private readonly IEntropyService entropyService;
        private readonly IDataFragmentService dataFragmentService;

        public event Action onGridUpdated;

        public ExtractService(
            IGridService gridService,
            IEntropyService entropyService,
            IDataFragmentService dataFragmentService)
        {
            this.gridService = gridService;
            this.entropyService = entropyService;
            this.dataFragmentService = dataFragmentService;
        }

        public void ExtractGrid()
        {
            // Apply passive penalty
            if (entropyService.EntropyPercent >= 100)
            {
                Debug.Log("[ExtractService] Entropy at 100%, forcing reset trigger");
                entropyService.Increase(1); // triggers reset
            }

            SymbolEffectProcessor.ApplyPassiveEntropyPenalty(gridService, entropyService);
            gridService.ClearAllExceptViruses();

            onGridUpdated?.Invoke();
            Debug.Log("[ExtractService] Extraction complete. Grid cleared except viruses.");
        }
    }
}