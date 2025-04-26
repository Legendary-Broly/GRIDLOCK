using System;
using UnityEngine;

namespace NewGameplay.Interfaces
{
    public interface ILoopEffectService
    {
        event Action OnLoopTransformed;
        
        void CheckLoopTransformations();
        void HandleLoopEffect(int x, int y);
    }
} 