using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class EscapeRoomManager : MonoBehaviour
{
    [Header("ข้อความชัยชนะ Text-TMP ลอยกลางห้อง (ทิ้งว่างได้)")]
    [SerializeField] private TextMeshPro winText;

    [SerializeField] private UnityEvent onEscaped;

    public static EscapeRoomManager Instance { get; private set; }

    private float startTime;
    private bool escaped;

    private void Awake()
    {
        Instance = this;
        startTime = Time.time;
        if (winText != null) winText.gameObject.SetActive(false);
    }

    public void RegisterEscape()
    {
        if (escaped) return;
        escaped = true;
        float elapsed = Time.time - startTime;
        Debug.Log($"[EscapeRoomManager] หนีสำเร็จใน {elapsed:F1} วินาที");
        if (winText != null)
        {
            winText.gameObject.SetActive(true);
            winText.text = $"YOU ESCAPED!\n{elapsed:F1} sec";
        }
        onEscaped.Invoke();
    }
}