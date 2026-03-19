using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private AudioClip clickSound; // Сюди перетягнеш звук у Unity
    private AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();
        // Додаємо AudioSource, якщо його забули додати вручну
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
            // Беремо гучність саме для UI
            float vol = PlayerPrefs.GetFloat("UIVolume", 0.5f);
            _audioSource.PlayOneShot(clickSound, vol);
        }
    }
}