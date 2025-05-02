using UnityEngine;
using System.Collections.Generic;

public class SoundEffectService : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip tileClick;
    [SerializeField] private AudioClip virusWarning;
    [SerializeField] private AudioClip codeShardReveal;
    [SerializeField] private AudioClip dataFragmentReveal;
    [SerializeField] private AudioClip entropyGain;
    [SerializeField] private AudioClip entropyLoss;

    public void PlayTileClick() => Play(tileClick);
    public void PlayWarning() => Play(virusWarning);
    public void PlayCodeShard() => Play(codeShardReveal);
    public void PlayDataFragment() => Play(dataFragmentReveal);
    public void PlayEntropyGain() => Play(entropyGain);
    public void PlayEntropyLoss() => Play(entropyLoss);

    private void Play(AudioClip clip)
    {
        if (clip && audioSource) audioSource.PlayOneShot(clip);
    }
}
