using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class VerticalNotebook : MonoBehaviour
{
    public Canvas canvas;
    public RectTransform NotebookPanel;

    public Sprite[] pages;
    public Sprite background;

    public Image CurrentPage;
    public Image NextPage;

    public bool interactable = true;

    public int currentIndex = 0;

    public float flipDuration = 0.2f;
    public UnityEvent OnFlip;

    Vector3 bottom;
    Vector3 top;
    Vector3 dragPoint;

    bool dragging = false;
    bool flipping = false;

    void Start()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        if (!canvas) Debug.LogError("Canvas not found! Place in a Canvas.");

        UpdateSprites();
        CalculateBounds();
    }

    void CalculateBounds()
    {
        bottom = new Vector3(0, -NotebookPanel.rect.height / 2f, 0);
        top = new Vector3(0, NotebookPanel.rect.height / 2f, 0);
    }

    Vector3 ToLocal(Vector3 screenPos)
    {
        Vector3 world = canvas.worldCamera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, canvas.planeDistance)
        );
        return NotebookPanel.InverseTransformPoint(world);
    }

    void Update()
    {
        if (dragging && interactable && !flipping)
        {
            dragPoint = ToLocal(Input.mousePosition);
            UpdateFlip(dragPoint);
        }
    }

    void UpdateSprites()
    {
        CurrentPage.sprite = (currentIndex < pages.Length) ? pages[currentIndex] : background;
        NextPage.sprite = (currentIndex + 1 < pages.Length) ? pages[currentIndex + 1] : background;
    }

    public void OnMouseDownPage()
    {
        if (!interactable || flipping) return;
        if (currentIndex >= pages.Length - 1) return;

        dragging = true;
    }

    public void OnMouseUpPage()
    {
        if (!interactable || flipping) return;

        dragging = false;

        float distToTop = Vector2.Distance(dragPoint, top);
        float distToBottom = Vector2.Distance(dragPoint, bottom);

        if (distToTop < distToBottom)
            TweenForward();
        else
            TweenBack();
    }

    void UpdateFlip(Vector3 dragPos)
    {
        float clampY = Mathf.Clamp(dragPos.y, bottom.y, top.y);
        CurrentPage.rectTransform.pivot = new Vector2(0.5f, 0);
        CurrentPage.rectTransform.localPosition = new Vector3(0, clampY - bottom.y, 0);

        float t = Mathf.InverseLerp(bottom.y, top.y, clampY);
        CurrentPage.rectTransform.localRotation = Quaternion.Euler(Mathf.Lerp(0, -180, t), 0, 0);
    }

    void TweenForward()
    {
        flipping = true;
        StartCoroutine(FlipTween(top, () =>
        {
            currentIndex++;
            ResetState();
            if (OnFlip != null) OnFlip.Invoke();
        }));
    }

    void TweenBack()
    {
        flipping = true;
        StartCoroutine(FlipTween(bottom, () =>
        {
            ResetState();
        }));
    }

    IEnumerator FlipTween(Vector3 target, System.Action onFinish)
    {
        int steps = (int)(flipDuration / 0.02f);
        Vector3 startPos = CurrentPage.rectTransform.localPosition;
        for (int i = 0; i < steps; i++)
        {
            float t = (float)i / (steps - 1);
            float newY = Mathf.Lerp(startPos.y, target.y - bottom.y, t);

            CurrentPage.rectTransform.localPosition = new Vector3(0, newY, 0);
            CurrentPage.rectTransform.localRotation =
                Quaternion.Euler(Mathf.Lerp(CurrentPage.rectTransform.localEulerAngles.x, -180 * t, 1), 0, 0);

            yield return new WaitForSeconds(0.02f);
        }

        onFinish();
    }

    void ResetState()
    {
        flipping = false;
        dragging = false;

        CurrentPage.rectTransform.localPosition = Vector3.zero;
        CurrentPage.rectTransform.localRotation = Quaternion.identity;

        UpdateSprites();
    }
}
