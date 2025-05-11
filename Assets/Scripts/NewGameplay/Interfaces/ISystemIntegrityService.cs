using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using NewGameplay.Controllers;

namespace NewGameplay.Interfaces
{
    public interface ISystemIntegrityService
    {
        event Action<float> OnIntegrityChanged; // float from 0 to 100
        float CurrentIntegrity { get; }
        void Decrease(float amount);
        void Increase(float amount);
        void SetIntegrity(float amount);
        void SetGameOverController(GameOverController controller);
    }
}