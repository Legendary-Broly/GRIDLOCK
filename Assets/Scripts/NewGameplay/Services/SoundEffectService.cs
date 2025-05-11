using UnityEngine;

namespace NewGameplay.Services
{
    public class SoundEffectService : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip tileReveal;
        [SerializeField] private AudioClip virusReveal;
        [SerializeField] private AudioClip symbolPlace;
        [SerializeField] private AudioClip roundComplete;

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        public void PlayTileReveal() => Play(tileReveal);
        public void PlayVirusReveal() => Play(virusReveal);
        public void PlaySymbolPlace() => Play(symbolPlace);
        public void PlayRoundComplete() => Play(roundComplete);

        private void Play(AudioClip clip)
        {
            if (clip != null && audioSource != null)
                audioSource.PlayOneShot(clip);
        }
    }
}
