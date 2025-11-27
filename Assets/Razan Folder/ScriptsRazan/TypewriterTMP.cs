using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TypewriterTMP : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private string playerTag = "Player";
    [Header("References")]
    public TextMeshProUGUI textComponent;

    [Header("Text")]
    [TextArea] public string fullText;
    [SerializeField] private InputActionReference interactAction;   // زر E
    [SerializeField] private InputActionReference gamepadReadAction; // زر R1

    [Header("Timing")]
    public float charDelay = 0.04f;        // الوقت بين كل حرف
    public float punctuationDelay = 0.18f; // تأخير إضافي بعد علامات الترقيم

    [Header("Sound (اختياري)")]
    public AudioSource audioSource;        // يجب أن يكون AudioSource على نفس الكائن أو أي كائن آخر
    public AudioClip typingSound;          // الصوت الذي سيشتغل بشكل متواصل أثناء الكتابة

    [Header("Options")]
    public bool playOnStart = false;

    [Header("After Typing")]
    [Tooltip("الزر اللي يظهر بعد ما يخلص الكتابة")]
    public GameObject continueButton;      // زر يكمل للمشهد التالي
    [Tooltip("اسم المشهد اللي يفتح بعد شاشة Night 2")]
    public string nextSceneName;           // اكتب اسم المشهد بالضبط مثل الـ Build Settings

    [Header("Night 2 Fade")]
    [Tooltip("CanvasGroup حق لوحة Night 2")]
    public CanvasGroup night2CanvasGroup;
    public float nightFadeIn = 1.0f;
    public float nightHold = 1.0f;
    public float nightFadeOut = 1.0f;

    Coroutine typingCoroutine;
    bool typingFinished = false;
    bool isNightSequenceRunning = false;

    void Start()
    {
        
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
        // نخفي الزر في البداية
        if (continueButton != null)
            continueButton.SetActive(false);
            

        // نتاكد ان لوحة Night 2 مخفية
        if (night2CanvasGroup != null)
        {
            if (!night2CanvasGroup.gameObject.activeSelf)
                night2CanvasGroup.gameObject.SetActive(true); // نخليها شغالة لكن شفافة
            night2CanvasGroup.alpha = 0f;
        }

        if (playOnStart)
            StartTyping();
    }

    public void StartTyping()
    {
        if (textComponent == null)
            return;

        typingFinished = false;

        // إخفاء الزر كل مرة نبدأ كتابة جديدة
        if (continueButton != null)
            continueButton.SetActive(false);

        // إيقاف أي عملية كتابة سابقة
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText());
    }

    public void StopTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = null;

        // تأكد من إيقاف الصوت عند التوقف
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }

    public void SkipToEnd()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        textComponent.text = fullText;
        textComponent.ForceMeshUpdate();
        textComponent.maxVisibleCharacters = int.MaxValue;
        typingCoroutine = null;

        // إيقاف الصوت عند إظهار النص كاملاً
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        typingFinished = true;
        ShowContinueButton();
    }

    IEnumerator TypeText()
    {
        textComponent.text = fullText;
        textComponent.ForceMeshUpdate();

        int total = textComponent.textInfo.characterCount;
        textComponent.maxVisibleCharacters = 0;

        // ✅ تشغيل الصوت المستمر عند بدء الكتابة
        if (audioSource != null && typingSound != null)
        {
            audioSource.clip = typingSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        for (int i = 1; i <= total; i++)
        {
            textComponent.maxVisibleCharacters = i;

            // تأخير إضافي بعد علامات الترقيم
            char currentChar = GetCharacterAtVisibleIndex(textComponent, i - 1);
            float wait = charDelay;
            if (currentChar == '.' || currentChar == ',' || currentChar == '!' || currentChar == '?')
                wait += punctuationDelay;

            yield return new WaitForSeconds(wait);
        }

        // إيقاف الصوت عند نهاية الكتابة
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        typingCoroutine = null;
        typingFinished = true;

        // ✅ لما يخلص الكتابة نظهر الزر
        ShowContinueButton();
    }

    void ShowContinueButton()
    {
        if (continueButton != null)
            continueButton.SetActive(true);
            if (interactAction != null)
            {
                interactAction.action.Enable();
            }
            if (gamepadReadAction != null)
            {
                gamepadReadAction.action.Enable();
            }
    }

    // ⬇ هذه الدالة تستدعيها من الزر
    public void OnContinueButtonPressed()
    {
        if (!typingFinished) return;
        if (isNightSequenceRunning) return;

        // نخفي الزر عشان ما يقدر يضغطه مرة ثانية
        if (continueButton != null)
            continueButton.SetActive(false);
            if (interactAction != null)
            {
                interactAction.action.Disable();
            }
            if (gamepadReadAction != null)
            {
                gamepadReadAction.action.Disable();
            }

        StartCoroutine(Night2Sequence());
    }

   private IEnumerator Night2Sequence()
{
    isNightSequenceRunning = true;

    Time.timeScale = 1f;

    if (night2CanvasGroup != null)
    {
        night2CanvasGroup.gameObject.SetActive(true);

        // Fade IN Night 2 (on the paycheck scene)
        float t = 0f;
        night2CanvasGroup.alpha = 0f;
        while (t < nightFadeIn)
        {
            t += Time.deltaTime;
            night2CanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / nightFadeIn);
            yield return null;
        }
        night2CanvasGroup.alpha = 1f;

        // Hold Night 2 fully visible
        yield return new WaitForSeconds(nightHold);

        // ❗️IMPORTANT: we DO NOT fade out here anymore
        // We go to the next scene while screen is fully covered by Night 2
    }

    if (!string.IsNullOrEmpty(nextSceneName))
    {
        SceneManager.LoadScene(nextSceneName);
    }
    else
    {
        Debug.LogError("TypewriterTMP: nextSceneName is empty!");
    }

    isNightSequenceRunning = false;
}

    // استخراج الحرف الحقيقي بناءً على الفهرس (يتعامل مع Rich Text)
    char GetCharacterAtVisibleIndex(TextMeshProUGUI tmp, int index)
    {
        if (tmp.textInfo == null || tmp.textInfo.characterInfo == null || tmp.textInfo.characterInfo.Length == 0)
            return ' ';
        if (index < 0 || index >= tmp.textInfo.characterCount)
            return ' ';
        return tmp.textInfo.characterInfo[index].character;
    }
}
