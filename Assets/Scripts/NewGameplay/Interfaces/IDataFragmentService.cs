using UnityEngine;
using System;

namespace NewGameplay.Interfaces
{
    public interface IDataFragmentService
    {
        event Action<Vector2Int> OnFragmentRevealed;
        
        bool IsFragmentAt(Vector2Int position);
        void PlaceFragment(Vector2Int position);
        void RemoveFragment(Vector2Int position);
        void ClearFragments();
        
        // Service Dependencies
        void SetTileElementService(ITileElementService service);
        void SetPayloadService(IPayloadService service);

        bool AnyRevealedFragmentsContainVirus();
        void SpawnFragments(int count);
    }
}
