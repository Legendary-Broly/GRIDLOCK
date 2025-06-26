using System;
using System.Collections.Generic;
using UnityEngine;
using NewGameplay.Models;
using NewGameplay.Enums;

namespace NewGameplay.Interfaces
{
    public interface IGridService
    {
        // Events
        event Action OnGridUpdated;

        // Properties
        int GridWidth { get; }
        int GridHeight { get; }
        ITileElementService TileElementService { get; }
        ISymbolToolService SymbolToolService { get; }

        // Grid State Management
        void InitializeTileStates(int width, int height);
        void SetSymbol(int x, int y, string symbol);
        string GetSymbolAt(int x, int y);
        void SetTileState(int x, int y, TileState state);
        TileState GetTileState(int x, int y);
        void ClearAllTiles();
        void ClearAllExceptViruses();
        void TriggerGridUpdate();

        // Tile Interaction
        bool IsInBounds(int x, int y);
        bool IsTileRevealed(int x, int y);
        bool CanRevealTile(int x, int y);
        void RevealTile(int x, int y, bool forceReveal = false);
        bool IsFirstRevealDone();
        void SetFirstRevealPermitted(bool value);
        Vector2Int? GetLastRevealedTile();
        void SetLastRevealedTile(Vector2Int pos);

        // Virus Management
        void SetVirusFlag(int x, int y, bool flagged);
        bool IsFlaggedAsVirus(int x, int y);
        bool CanUseVirusFlag();
        void EnableVirusFlag();
        void DisableVirusFlag();

        // Grid Interaction Control
        void LockInteraction();
        void UnlockInteraction();

        // Tile Position Queries
        List<Vector2Int> GetAllEmptyTilePositions();
        List<Vector2Int> GetValidInitialRevealPositions();
        bool IsValidAdjacentMove(int x, int y);
        bool IsValidDiagonalMove(int x, int y);

        // Service Dependencies
        void SetTileElementService(ITileElementService service);
        void SetProgressService(IProgressTrackerService service);
        void SetDataFragmentService(IDataFragmentService service);
        void SetSymbolToolService(ISymbolToolService service);
        void SetChatLogService(IChatLogService service);
        void SetPayloadService(IPayloadService service);
    }
}