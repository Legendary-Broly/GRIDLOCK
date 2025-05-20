using System;
using UnityEngine;
using NewGameplay.Interfaces;

namespace NewGameplay.Services
{
    public class ExtractService : IExtractService
    {
        private readonly IGridService gridService;
        private readonly IDataFragmentService dataFragmentService;

        public event Action OnExtractComplete;

        public ExtractService(
            IGridService gridService,
            IDataFragmentService dataFragmentService)
        {
            this.gridService = gridService;
            this.dataFragmentService = dataFragmentService;
        }

        public void ExtractGrid()
        {


            // Clear grid except viruses
            gridService.ClearAllExceptViruses();

            // Trigger extraction event
            OnExtractComplete?.Invoke();


        }
    }
}
