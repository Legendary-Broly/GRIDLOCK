using UnityEngine;

namespace NewGameplay.Interfaces
{
    public interface IDataFragmentService
    {
        void SpawnFragments(int count);
        bool AnyRevealedFragmentsContainVirus();
        void RegisterFragmentAt(int x, int y);
        bool IsFragmentAt(Vector2Int pos);
    }
}
