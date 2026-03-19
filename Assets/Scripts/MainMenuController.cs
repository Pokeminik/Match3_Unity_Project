using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider uiSlider;

    void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Завантажуємо збережені значення
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        float uiVol = PlayerPrefs.GetFloat("UIVolume", 0.5f);

        // Налаштовуємо слайдери
        if (musicSlider != null)
        {
            musicSlider.value = musicVol;
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVol;
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }
        if (uiSlider != null)
        {
            uiSlider.value = uiVol;
            uiSlider.onValueChanged.AddListener(OnUIChanged);
        }

        // ПРИМУСОВО встановлюємо гучність при старті
        ApplyAllVolumes(musicVol, uiVol);
    }

    private void ApplyAllVolumes(float mVol, float uVol)
    {
        if (MusicManager.Instance != null)
        {
            var source = MusicManager.Instance.GetComponent<AudioSource>();
            if (source != null) source.volume = mVol;
        }
        if (UIManager.Instance != null)
        {
            var source = UIManager.Instance.GetComponent<AudioSource>();
            if (source != null) source.volume = uVol;
        }
    }

    public void OnMusicChanged(float value)
    {
        if (MusicManager.Instance != null)
        {
            AudioSource source = MusicManager.Instance.GetComponent<AudioSource>();
            if (source != null)
            {
                source.volume = value;
                Debug.Log($"Керую об'єктом: {MusicManager.Instance.gameObject.name}, ID: {MusicManager.Instance.gameObject.GetInstanceID()}, Гучність: {value}");
            }
        }
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void OnUIChanged(float value)
    {
        if (UIManager.Instance != null)
        {
            var source = UIManager.Instance.GetComponent<AudioSource>();
            if (source != null) source.volume = value;
        }
        PlayerPrefs.SetFloat("UIVolume", value);
    }

    public void OnSFXChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void PlayClickSound()
    {
        if (UIManager.Instance != null)
        {
            var source = UIManager.Instance.GetComponent<AudioSource>();
            if (source != null)
            {
                source.PlayOneShot(source.clip, PlayerPrefs.GetFloat("UIVolume", 0.5f));
            }
        }
    }

    public void PlayGame()
    {
        //PlayClickSound();
        Invoke("LoadGameScene", 0.2f); 
    }

    private void LoadGameScene() => SceneManager.LoadScene("GameScene");

    public void OpenSettings() => settingsPanel.SetActive(true);
    public void CloseSettings()
    {
        PlayerPrefs.Save();
        settingsPanel.SetActive(false);
    }

    public void QuitGame() => Application.Quit();
}