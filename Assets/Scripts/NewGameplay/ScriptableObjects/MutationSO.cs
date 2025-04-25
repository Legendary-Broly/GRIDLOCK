using UnityEngine;

namespace NewGameplay.ScriptableObjects
{
    public abstract class MutationSO : ScriptableObject
    {
        [SerializeField] private string mutationName;
        [SerializeField] private string mutationDescription;
        [SerializeField] private Sprite mutationIcon;
        [SerializeField] private MutationType mutationType;

        public string MutationName => mutationName;
        public string MutationDescription => mutationDescription;
        public Sprite MutationIcon => mutationIcon;
        public MutationType MutationType => mutationType;
    }
}
