using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private AudioClip clickSound; 
    private AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayClickSound()
    {
        if (_audioSource != null && clickSound != null)
        {
            float vol = PlayerPrefs.GetFloat("UIVolume", 0.5f);
            _audioSource.PlayOneShot(clickSound, vol);
        }
    }
}