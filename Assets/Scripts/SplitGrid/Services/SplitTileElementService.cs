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

namespace SplitGrid.Services
{
    public class SplitTileElementService : ISplitTileElementService
    {
        private readonly ISplitGridService splitGridService;
        private readonly ITileElementService elementServiceA;
        private readonly ITileElementService elementServiceB;
        private readonly IPayloadManager payloadManager;

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
        }

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
    }
} 