using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class MagicGemSound : MonoBehaviour, IPointerDownHandler
{
    [Header("Налаштування ноти")]
    [Range(0.5f, 2.0f)] public float notePitch = 1.0f;

    [Header("Візуальні ефекти")]
    public Color gemColor = Color.white;
    public GameObject magicDustPrefab;

    private static Coroutine _resetCoroutine; // Спільна змінна для контролю пітчу

    public void OnPointerDown(PointerEventData eventData)
    {
        // 1. ЗУПИНЯЄМО всі попередні спроби скинути звук
        StopAllCoroutines();

        PlayGemNote();
        SpawnMagicDust();
        StartCoroutine(PunchEffect());
    }

    private void PlayGemNote()
    {
        if (UIManager.Instance != null)
        {
            AudioSource source = UIManager.Instance.GetComponent<AudioSource>();
            if (source != null)
            {
                source.pitch = notePitch;
                UIManager.Instance.PlayClickSound();

                // Запускаємо скидання, яке можна перервати наступним кліком
                StartCoroutine(ResetPitchRoutine(source, 0.4f));
            }
        }
    }

    private IEnumerator ResetPitchRoutine(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (source != null) source.pitch = 1.0f;
    }

    private void SpawnMagicDust()
    {
        if (magicDustPrefab == null) return;

        // Створюємо як дитину, щоб воно було в просторі UI
        GameObject dust = Instantiate(magicDustPrefab, transform);

        // ВАЖЛИВО: Скидаємо Z та Scale, щоб частки не були за екраном або мікроскопічними
        dust.transform.localPosition = new Vector3(0, 0, -10f); // Трохи висуваємо вперед до камери
        dust.transform.localScale = Vector3.one * 100f; // У UI часто потрібен великий масштаб

        ParticleSystem ps = dust.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = gemColor;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy; // Щоб частки слухалися масштабу Canvas

            // Змушуємо рендер бути поверх всього
            var renderer = dust.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 100;
        }

        Destroy(dust, 1.5f);
    }

    private IEnumerator PunchEffect()
    {
        Vector3 originalScale = Vector3.one;
        transform.localScale = originalScale * 1.15f;
        yield return new WaitForSeconds(0.05f);
        transform.localScale = originalScale;
    }
}