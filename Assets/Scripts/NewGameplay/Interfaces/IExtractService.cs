using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewGameplay.Interfaces
{
    public interface IExtractService
    {
        void ExtractGrid();
        event System.Action OnExtractComplete;

    }
}