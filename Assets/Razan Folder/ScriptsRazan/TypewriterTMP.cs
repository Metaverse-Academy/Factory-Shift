using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterTMP : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI textComponent;

    [Header("Text")]
    [TextArea] public string fullText;

    [Header("Timing")]
    public float charDelay = 0.04f;        // الوقت بين كل حرف
    public float punctuationDelay = 0.18f; // تأخير إضافي بعد علامات الترقيم

    [Header("Sound (اختياري)")]
    public AudioSource audioSource;        // يجب أن يكون AudioSource على نفس الكائن أو أي كائن آخر
    public AudioClip typingSound;          // الصوت الذي سيشتغل بشكل متواصل أثناء الكتابة

    [Header("Options")]
    public bool playOnStart = false;

    Coroutine typingCoroutine;

    void Start()
    {
        if (playOnStart)
            StartTyping();
    }

    public void StartTyping()
    {
        if (textComponent == null)
            return;

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
