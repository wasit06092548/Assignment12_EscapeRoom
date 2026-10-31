using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class KeypadController : MonoBehaviour
{
    [Header("ตั้งค่ารหัสลับ")]
    [SerializeField] private int codeLength = 4;
    [SerializeField] private bool useRandomCode = true;   // ปิดถ้าจะใช้รหัสตายตัว
    [SerializeField] private string fixedCode = "1234";

    [Header("จอแสดงตัวเลขที่พิมพ์ (Entry Text — ทิ้งว่างได้)")]
    [SerializeField] private TextMeshPro entryText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color successColor = new Color(0.2f, 1f, 0.3f);
    [SerializeField] private Color errorColor = new Color(1f, 0.25f, 0.25f);

    [Header("ตรวจรหัสอัตโนมัติเมื่อพิมพ์ครบ (ไม่ต้องกด Submit)")]
    [SerializeField] private bool autoCheckWhenFull = true;

    [Header("Events — ต่อกับ EscapeDoor.UnlockPadlock")]
    [SerializeField] private UnityEvent onCodeAccepted;
    [SerializeField] private UnityEvent onCodeRejected;

    public string CurrentCode { get; private set; }
    public bool Solved { get; private set; }

    private string entry = "";
    private AudioSource audioSource;

    private void Start()
    {
        if (useRandomCode)
        {
            CurrentCode = "";
            for (int i = 0; i < codeLength; i++) CurrentCode += Random.Range(0, 10).ToString();
        }
        else
        {
            CurrentCode = fixedCode;
            codeLength = fixedCode.Length;
        }
        if (entryText != null) entryText.color = normalColor;
        RefreshDisplay();
    }

    public void RegisterKey(KeypadKey key)
    {
        if (Solved) return;
        switch (key.Type)
        {
            case KeypadKey.KeyType.Clear:
                entry = "";
                PlayTone(320f, 0.08f);
                RefreshDisplay();
                break;

            case KeypadKey.KeyType.Submit:
                CheckCode();
                break;

            default:
                if (key.Digit < 0) return;
                if (entry.Length >= codeLength) entry = "";   // พิมพ์เกิน = เริ่มใหม่
                entry += key.Digit.ToString();
                PlayTone(520f + key.Digit * 45f, 0.06f);
                RefreshDisplay();
                if (autoCheckWhenFull && entry.Length == codeLength) CheckCode();
                break;
        }
    }

    private void CheckCode()
    {
        if (string.IsNullOrEmpty(entry)) return;

        if (entry == CurrentCode)
        {
            Solved = true;
            if (entryText != null) entryText.color = successColor;
            RefreshDisplay();
            StartCoroutine(SuccessJingle());
            onCodeAccepted.Invoke();
        }
        else
        {
            StartCoroutine(WrongFlash());
            onCodeRejected.Invoke();
        }
    }

    private IEnumerator WrongFlash()
    {
        if (entryText != null) entryText.color = errorColor;
        PlayTone(160f, 0.25f);
        yield return new WaitForSeconds(0.6f);
        entry = "";
        if (entryText != null) entryText.color = normalColor;
        RefreshDisplay();
    }

    private IEnumerator SuccessJingle()
    {
        PlayTone(660f, 0.10f);
        yield return new WaitForSeconds(0.13f);
        PlayTone(880f, 0.16f);
    }

    private void RefreshDisplay()
    {
        if (entryText == null) return;
        string display = "";
        for (int i = 0; i < codeLength; i++)
        {
            display += (i < entry.Length) ? entry[i].ToString() : "_";
            if (i < codeLength - 1) display += " ";
        }
        entryText.text = display;
    }

    // สร้างเสียงบี๊บเองในโค้ด ไม่ต้องมีไฟล์เสียง
    private void PlayTone(float frequency, float duration, float volume = 0.25f)
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.9f;
        }
        const int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        var clip = AudioClip.Create("Tone", sampleCount, 1, sampleRate, false);
        var data = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float k = (float)i / sampleCount;
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * k * duration) * (1f - k) * volume;
        }
        clip.SetData(data, 0);
        audioSource.PlayOneShot(clip);
    }
}