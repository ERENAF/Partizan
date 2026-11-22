using UnityEngine;
using System.Collections;
[RequireComponent(typeof(CanvasGroup))]
public class DialogueUIAnimator : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private float offsetY= 35f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private Vector2 originalPosition;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        originalPosition = rectTransform.anchoredPosition;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void Show()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        gameObject.SetActive(true);

        rectTransform.anchoredPosition =
            originalPosition + Vector2.down * offsetY;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        animationCoroutine = StartCoroutine(Animate(true));
    }
    public void Hide()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(
            Animate(false)
        );
    }

    private IEnumerator Animate(bool show)
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 endPosition = show
            ? originalPosition
            : originalPosition + Vector2.down * offsetY;

        float startAlpha = canvasGroup.alpha;
        float endAlpha = show ? 1f : 0f;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Mathf.Min(Time.unscaledDeltaTime, 0.033f);;

            float t = duration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / duration);

            // Smoothstep
            t = t * t * (3f - 2f * t);

            rectTransform.anchoredPosition =
                Vector2.Lerp(startPosition, endPosition, t);

            canvasGroup.alpha =
                Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }

        rectTransform.anchoredPosition = endPosition;
        canvasGroup.alpha = endAlpha;

        canvasGroup.interactable = show;
        canvasGroup.blocksRaycasts = show;

        animationCoroutine = null;
    }
}
