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
    
    public class SplitTileElementService : ISplitTileElementService
    {
        private readonly ISplitGridService splitGridService;
        private readonly ITileElementService elementServiceA;
        private readonly ITileElementService elementServiceB;
        private readonly IPayloadManager payloadManager;

        public event Action<int, int> OnElementTriggered;

        public int GridWidth => elementServiceA.GridWidth;
        public int GridHeight => elementServiceA.GridHeight;

        public SplitTileElementService(
            ISplitGridService splitGridService,
            ITileElementService elementServiceA,
            ITileElementService elementServiceB,
            IPayloadManager payloadManager)
        {
            this.splitGridService = splitGridService;
            this.elementServiceA = elementServiceA;
            this.elementServiceB = elementServiceB;
            this.payloadManager = payloadManager;

            // Subscribe to element triggered events
            elementServiceA.OnElementTriggered += (x, y) => OnElementTriggered?.Invoke(x, y);
            elementServiceB.OnElementTriggered += (x, y) => OnElementTriggered?.Invoke(x, y);
        }

        // ITileElementService implementation
        public TileElementType GetElementAt(int x, int y)
        {
            // Determine which grid the element belongs to
            if (splitGridService.IsInBounds(GridID.GridA, x, y))
            {
                return elementServiceA.GetElementAt(x, y);
            }
            else if (splitGridService.IsInBounds(GridID.GridB, x, y))
            {
                return elementServiceB.GetElementAt(x, y);
            }
            return default(TileElementType);
        }

        public void TriggerElementEffect(int x, int y)
        {
            // Determine which grid the element belongs to
            if (splitGridService.IsInBounds(GridID.GridA, x, y))
            {
                elementServiceA.TriggerElementEffect(x, y);
            }
            else if (splitGridService.IsInBounds(GridID.GridB, x, y))
            {
                elementServiceB.TriggerElementEffect(x, y);
            }
        }

        public TileElementSO GetElementSOAt(int x, int y)
        {
            // Determine which grid the element belongs to
            if (splitGridService.IsInBounds(GridID.GridA, x, y))
            {
                return elementServiceA.GetElementSOAt(x, y);
            }
            else if (splitGridService.IsInBounds(GridID.GridB, x, y))
            {
                return elementServiceB.GetElementSOAt(x, y);
            }
            return null;
        }

        public void ResizeGrid(int width, int height)
        {
            elementServiceA.ResizeGrid(width, height);
            elementServiceB.ResizeGrid(width, height);
        }

        public void ClearElements()
        {
            elementServiceA.ClearElements();
            elementServiceB.ClearElements();
        }

        public void AddManualElement(TileElementType elementType)
        {
            elementServiceA.AddManualElement(elementType);
            elementServiceB.AddManualElement(elementType);
        }

        public void AddToSpawnPool(TileElementType element)
        {
            elementServiceA.AddToSpawnPool(element);
            elementServiceB.AddToSpawnPool(element);
        }

        public void OnTileRevealed(int x, int y)
        {
            // Determine which grid the tile belongs to
            if (splitGridService.IsInBounds(GridID.GridA, x, y))
            {
                elementServiceA.OnTileRevealed(x, y);
            }
            else if (splitGridService.IsInBounds(GridID.GridB, x, y))
            {
                elementServiceB.OnTileRevealed(x, y);
            }
        }

        // ISplitTileElementService implementation
        public void InitializeGrids(int widthA, int heightA, int widthB, int heightB)
        {
            elementServiceA.ResizeGrid(widthA, heightA);
            elementServiceB.ResizeGrid(widthB, heightB);
        }

        public void GenerateElements()
        {
            elementServiceA.GenerateElements();
            elementServiceB.GenerateElements();
        }

        public void TriggerElementEffect(GridID gridId, int x, int y)
        {
            var elementService = gridId == GridID.GridA ? elementServiceA : elementServiceB;
            elementService.TriggerElementEffect(x, y);
        }

        public TileElementType GetElementAt(GridID gridId, int x, int y)
        {
            var elementService = gridId == GridID.GridA ? elementServiceA : elementServiceB;
            return elementService.GetElementAt(x, y);
        }

        public TileElementSO GetElementSOAt(GridID gridId, int x, int y)
        {
            var elementService = gridId == GridID.GridA ? elementServiceA : elementServiceB;
            return elementService.GetElementSOAt(x, y);
        }

        public void AddManualElement(GridID gridId, TileElementType elementType)
        {
            var elementService = gridId == GridID.GridA ? elementServiceA : elementServiceB;
            elementService.AddManualElement(elementType);
        }

        public void AddToSpawnPool(GridID gridId, TileElementType element)
        {
            var elementService = gridId == GridID.GridA ? elementServiceA : elementServiceB;
            elementService.AddToSpawnPool(element);
        }

        public void OnTileRevealed(GridID gridId, int x, int y)
        {
            var elementService = gridId == GridID.GridA ? elementServiceA : elementServiceB;
            elementService.OnTileRevealed(x, y);
        }

        public void ResizeGrid(GridID gridId, int width, int height)
        {
            var elementService = gridId == GridID.GridA ? elementServiceA : elementServiceB;
            elementService.ResizeGrid(width, height);
        }

        public void ClearElements(GridID gridId)
        {
            var elementService = gridId == GridID.GridA ? elementServiceA : elementServiceB;
            elementService.ResizeGrid(splitGridService.GetGridWidth(gridId), splitGridService.GetGridHeight(gridId));
        }

        public void ApplyPayloadEffect(PayloadType payloadType)
        {
            // Handle payload effects based on the actual PayloadType enum
            switch (payloadType)
            {
                case PayloadType.DataCluster:
                    // Handle data cluster effect
                    break;
                case PayloadType.Phishing:
                    // Handle phishing effect
                    break;
                case PayloadType.Echo:
                    // Handle echo effect
                    break;
                case PayloadType.ToolkitExpansion:
                    // Handle toolkit expansion effect
                    break;
                case PayloadType.DamageOverTime:
                    // Handle damage over time effect
                    break;
                case PayloadType.WirelessUpload:
                    // Handle wireless upload effect
                    break;
            }
        }

        public void SetElementAt(int x, int y, TileElementType elementType)
        {
            // Determine which grid the element belongs to
            if (splitGridService.IsInBounds(GridID.GridA, x, y))
            {
                elementServiceA.SetElementAt(x, y, elementType);
            }
            else if (splitGridService.IsInBounds(GridID.GridB, x, y))
            {
                elementServiceB.SetElementAt(x, y, elementType);
            }
        }

        public void SetGridService(IGridService service)
        {
            elementServiceA.SetGridService(service);
            elementServiceB.SetGridService(service);
        }

        public void SetCodeShardTracker(ICodeShardTrackerService service)
        {
            elementServiceA.SetCodeShardTracker(service);
            elementServiceB.SetCodeShardTracker(service);
        }

        public void SetInjectService(IInjectService service)
        {
            elementServiceA.SetInjectService(service);
            elementServiceB.SetInjectService(service);
        }

        public void SetSystemIntegrityService(ISystemIntegrityService service)
        {
            elementServiceA.SetSystemIntegrityService(service);
            elementServiceB.SetSystemIntegrityService(service);
        }

        public void SetVirusService(IVirusService service)
        {
            elementServiceA.SetVirusService(service);
            elementServiceB.SetVirusService(service);
        }

        public void SetChatLogService(IChatLogService service)
        {
            elementServiceA.SetChatLogService(service);
            elementServiceB.SetChatLogService(service);
        }

        public void SetDataFragmentService(IDataFragmentService service)
        {
            elementServiceA.SetDataFragmentService(service);
            elementServiceB.SetDataFragmentService(service);
        }

        public void SetProgressTrackerService(IProgressTrackerService service)
        {
            elementServiceA.SetProgressTrackerService(service);
            elementServiceB.SetProgressTrackerService(service);
        }
    }
} 