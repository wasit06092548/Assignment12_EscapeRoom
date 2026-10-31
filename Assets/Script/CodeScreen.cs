using TMPro;
using UnityEngine;

public class CodeScreen : MonoBehaviour
{
    [Header("Text บนจอที่ใช้แสดงรหัสลับ")]
    [SerializeField] private TextMeshPro secretText;
    [SerializeField] private string prefix = "CODE: ";

    private KeypadController keypad;

    private void Start()
    {
        if (keypad == null) keypad = FindFirstObjectByType<KeypadController>();
    }

    private void Update()
    {
        if (keypad == null || secretText == null) return;
        string code = keypad.CurrentCode;
        if (string.IsNullOrEmpty(code)) return;

        string spaced = "";
        for (int i = 0; i < code.Length; i++)
        {
            spaced += code[i];
            if (i < code.Length - 1) spaced += " ";
        }
        string target = prefix + spaced;
        if (secretText.text != target) secretText.text = target;
    }
}