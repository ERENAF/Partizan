
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup menuGroup;
    [SerializeField] private CanvasGroup buttonsGroup;
    [SerializeField] private RectTransform buttonsTransform;

    [SerializeField] private Button playButton;
    [SerializeField] private Button exitButton;

    [Header("Animation")]
    [SerializeField, Min(0f)]
    private float fadeDuration = 0.5f;

    [SerializeField, Min(0f)]
    private float buttonsDelay = 0.2f;

    [SerializeField]
    private float buttonsOffsetY = 40f;

    [Header("Scene")]
    [SerializeField]
    private string gameSceneName = "GameScene";

    private Vector2 buttonsOriginalPosition;
    private bool isTransitioning;

    private void Awake()
    {
        buttonsOriginalPosition =
            buttonsTransform.anchoredPosition;

        menuGroup.alpha = 0f;
        buttonsGroup.alpha = 0f;

        menuGroup.interactable = false;
        menuGroup.blocksRaycasts = false;

        buttonsGroup.interactable = false;
        buttonsGroup.blocksRaycasts = false;

        buttonsTransform.anchoredPosition =
            buttonsOriginalPosition +
            Vector2.down * buttonsOffsetY;

        playButton.onClick.AddListener(Play);
        exitButton.onClick.AddListener(Exit);
    }

    private void Start()
    {
        StartCoroutine(ShowMenu());
    }

    private IEnumerator ShowMenu()
    {
        // Плавное появление меню
        yield return Fade(menuGroup, 0f, 1f, fadeDuration);

        // Небольшая задержка перед кнопками
        yield return new WaitForSecondsRealtime(buttonsDelay);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = fadeDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / fadeDuration);

            t = t * t * (3f - 2f * t);

            buttonsGroup.alpha = t;

            buttonsTransform.anchoredPosition =
                Vector2.Lerp(
                    buttonsOriginalPosition +
                    Vector2.down * buttonsOffsetY,
                    buttonsOriginalPosition,
                    t
                );

            yield return null;
        }

        buttonsGroup.alpha = 1f;
        buttonsTransform.anchoredPosition =
            buttonsOriginalPosition;

        menuGroup.interactable = true;
        menuGroup.blocksRaycasts = true;

        buttonsGroup.interactable = true;
        buttonsGroup.blocksRaycasts = true;
    }

    private IEnumerator Fade(
        CanvasGroup group,
        float from,
        float to,
        float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = duration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / duration);

            t = t * t * (3f - 2f * t);

            group.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        group.alpha = to;
    }

    public void Play()
    {
        if (isTransitioning)
            return;

        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        isTransitioning = true;

        menuGroup.interactable = false;
        menuGroup.blocksRaycasts = false;

        yield return Fade(
            menuGroup,
            menuGroup.alpha,
            0f,
            fadeDuration
        );

        SceneManager.LoadScene(gameSceneName);
    }

    public void Exit()
    {
        if (isTransitioning)
            return;

        StartCoroutine(ExitAnimation());
    }

    private IEnumerator ExitAnimation()
    {
        isTransitioning = true;

        menuGroup.interactable = false;
        menuGroup.blocksRaycasts = false;

        yield return Fade(
            menuGroup,
            menuGroup.alpha,
            0f,
            fadeDuration
        );

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(Play);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(Exit);
    }
}
