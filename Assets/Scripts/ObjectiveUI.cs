using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ObjectiveUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TextMeshProUGUI label;     // "Fix the fan..."
    [SerializeField] private CanvasGroup cg;            // on ObjectiveItem
    [SerializeField] private RectTransform rt;          // ObjectiveItem rect
    [SerializeField] private Image checkMark;           // ✅ image (optional)

    [Header("Animation")]
    [SerializeField] private float outUpDistance = 60f;       // px up on complete
    [SerializeField] private float outDuration = 0.35f;
    [SerializeField] private float inFromLeftDistance = 220f; // px from left
    [SerializeField] private float inDuration = 0.35f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);


    private Vector2 restPos;
    private Coroutine running;

    void Reset()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        label = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Awake()
    {
        if (!rt) rt = GetComponent<RectTransform>();
        if (!cg) cg = GetComponent<CanvasGroup>();
        restPos = rt.anchoredPosition;
        cg.alpha = 1f;

        if (checkMark)
        {
            checkMark.enabled = false; // start hidden
        }
    }

    /// <summary>Set the first objective text without animating.</summary>
    public void SetInitial(string text)
    {
        if (running != null) StopCoroutine(running);
        rt.anchoredPosition = restPos;
        cg.alpha = 1f;
        label.text = text;

        if (checkMark)
            checkMark.enabled = false;
            
    }

    /// <summary>
    /// Shows check mark, animates current objective out (up+fade),
    /// then swaps text and animates new one in from left without check.
    /// </summary>
    public void CompleteAndShowNext(string nextObjective)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Co_CompleteAndNext(nextObjective));
    }

    private IEnumerator Co_CompleteAndNext(string nextText)
    {
        // STEP 1: show the check mark on the finished objective
        if (checkMark)
            checkMark.enabled = true;

        // STEP 2: move up + fade out (box + text + ✅ together)
        yield return Co_MoveFade(
            fromPos: restPos,
            toPos: restPos + Vector2.up * outUpDistance,
            fromA: 1f,
            toA: 0f,
            dur: outDuration
        );

        // STEP 3: swap text & reset checkMark for the NEW objective
        label.text = nextText;
        if (checkMark)
            checkMark.enabled = false; // new objective starts without check

        // pre-position to the left, invisible
        rt.anchoredPosition = restPos + Vector2.left * inFromLeftDistance;
        cg.alpha = 0f;

        // STEP 4: move from left → rest + fade in
        yield return Co_MoveFade(
            fromPos: rt.anchoredPosition,
            toPos: restPos,
            fromA: 0f,
            toA: 1f,
            dur: inDuration
        );

        running = null;
    }

    private IEnumerator Co_MoveFade(Vector2 fromPos, Vector2 toPos, float fromA, float toA, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);
            float e = ease.Evaluate(k);

            rt.anchoredPosition = Vector2.LerpUnclamped(fromPos, toPos, e);
            cg.alpha = Mathf.LerpUnclamped(fromA, toA, e);

            yield return null;
        }

        rt.anchoredPosition = toPos;
        cg.alpha = toA;
    }
}
