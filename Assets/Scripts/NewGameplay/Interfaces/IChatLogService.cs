using UnityEngine;
using NewGameplay.ScriptableObjects;
using System;
using NewGameplay.Services;
using NewGameplay.Enums;


namespace NewGameplay.Interfaces
{
    public interface IChatLogService
    {
        void Log(string message, ChatMessageType type = ChatMessageType.Info, ChatDisplayMode mode = ChatDisplayMode.Instant);
        void SystemMessage(string message, ChatMessageType type = ChatMessageType.Info, ChatDisplayMode mode = ChatDisplayMode.Instant);
        void LogRandomInjectLine();
        void LogTileElementReveal(TileElementType type);
        void LogVirusReveal();
    }
}
