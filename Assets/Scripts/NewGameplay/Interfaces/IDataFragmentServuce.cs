using UnityEngine;

namespace NewGameplay
{
    public interface IDataFragmentService
    {
        void SpawnFragment();
        bool IsFragmentFullySurrounded();
        Vector2Int? GetFragmentPosition();
        bool IsFragmentPresent();
        void ClearFragment();
    }

}
