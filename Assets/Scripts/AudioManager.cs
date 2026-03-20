using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [Header("≈фекти")]
    [SerializeField] private AudioClip matchSound;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip boosterSound;

    private AudioSource _sfxSource;

    protected override void Awake()
    {
        base.Awake();
        _sfxSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayMatch(int combo)
    {
        if (matchSound == null) return;

        float vol = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        _sfxSource.pitch = 1f + (combo * 0.1f);
        if (_sfxSource.pitch > 2f) _sfxSource.pitch = 2f;

        _sfxSource.PlayOneShot(matchSound, vol);
    }

    public void PlayExplosion()
    {
        if (explosionSound == null) return;

        float vol = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        _sfxSource.pitch = Random.Range(0.9f, 1.1f); 
        _sfxSource.PlayOneShot(explosionSound, vol);
    }
}