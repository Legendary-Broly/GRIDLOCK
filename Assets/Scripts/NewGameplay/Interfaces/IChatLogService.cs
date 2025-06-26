using UnityEngine;
using NewGameplay.ScriptableObjects;
using System;
using NewGameplay.Services;
using NewGameplay.Enums;


namespace NewGameplay.Interfaces
{
    public interface IChatLogService
    {
        // Chat Log Events
        void LogVirusReveal();
        void LogDataFragmentReveal();
        void LogCorrectFlag();
        void LogIncorrectFlag();
        void LogRandomInjectLine();
        //void LogExtractComplete();
        //void LogRoundStart(int roundNumber);
        //void LogRoundComplete(int roundNumber);
        //void LogGameOver(bool isVictory);
        
        // Chat Log State
        //void ClearLog();
        //void AddMessage(string message);
        //void AddSystemMessage(string message);
        
        // Chat Log Configuration
        //void SetMaxMessages(int count);
        //void SetMessageTimeout(float seconds);
    }
}
