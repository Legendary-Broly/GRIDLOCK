using UnityEngine;

namespace NewGameplay.Interfaces
{
    public interface IDataFragmentService
    {
        void SpawnFragments(int count);
        int GetRevealedFragmentCount();
        bool AnyRevealedFragmentsContainVirus();
    }
}
