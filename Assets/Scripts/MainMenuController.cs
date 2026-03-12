using UnityEngine;
using UnityEngine.SceneManagement; // Обов'язково для перемикання сцен

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    void Start()
    {
        // При старті гри вікно налаштувань має бути приховане
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true); // Показуємо вікно
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false); // Ховаємо вікно
    }
    public void PlayGame()
    {
        // Завантажує сцену з назвою "GameScene"
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Debug.Log("Гра закривається...");
        Application.Quit(); // Працює тільки у зібраній грі (.exe), не в редакторі
    }
}