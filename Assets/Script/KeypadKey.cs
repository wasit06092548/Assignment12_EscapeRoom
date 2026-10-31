using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class KeypadKey : MonoBehaviour
{
    public enum KeyType { Digit, Clear, Submit }

    [Header("ชนิดของปุ่ม (Digit = ตัวเลข, Clear = ล้าง, Submit = ยืนยัน)")]
    [SerializeField] private KeyType keyType = KeyType.Digit;

    [Header("ค่าตัวเลข (เว้นว่าง = อ่านอัตโนมัติจาก Text TMP ที่เป็นลูก)")]
    [SerializeField] private int digit = -1;

    [Header("การกดปุ่ม")]
    [SerializeField] private float pressDepth = 0.004f;
    [SerializeField] private float pressDuration = 0.07f;
    [SerializeField] private float cooldown = 0.35f;

    [Header("วัสดุเมื่อถูกกด (ลาก ButtonTouched.mat มาใส่ — ไม่ใส่ก็ได้)")]
    [SerializeField] private Material pressedMaterial;

    private KeypadController controller;
    private Vector3 restPosition;
    private bool animating;
    private float lastPressTime = -10f;
    private Material[] originalMaterials;
    private MeshRenderer meshRenderer;

    public KeyType Type => keyType;
    public int Digit => digit;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) originalMaterials = meshRenderer.sharedMaterials;

        // สร้าง BoxCollider อัตโนมัติถ้ายังไม่มี (ขนาดตาม mesh ของปุ่ม)
        if (GetComponent<Collider>() == null)
        {
            var mf = GetComponent<MeshFilter>();
            var box = gameObject.AddComponent<BoxCollider>();
            if (mf != null && mf.sharedMesh != null)
            {
                box.center = mf.sharedMesh.bounds.center;
                box.size = mf.sharedMesh.bounds.size;
            }
        }

        var interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnSelect);
        interactable.firstHoverEntered.AddListener(OnHoverEnter); // แตะมือ = กด

        restPosition = transform.localPosition;
        controller = GetComponentInParent<KeypadController>();
    }

    private void Start()
    {
        // อ่านตัวเลขจากป้าย Text (TMP) ลูกอัตโนมัติ เช่น "7" -> 7
        if (keyType == KeyType.Digit && digit < 0)
        {
            var tmp = GetComponentInChildren<TextMeshPro>(true);
            if (tmp != null && int.TryParse(tmp.text.Trim(), out int value))
                digit = value;
        }
        if (keyType == KeyType.Digit && digit < 0)
            Debug.LogWarning($"[KeypadKey] {name} ไม่มีค่าตัวเลข (ไม่พบ TMP ลูกและไม่ได้ตั้งใน Inspector)", this);
    }

    private void OnSelect(SelectEnterEventArgs _) => Press();
    private void OnHoverEnter(HoverEnterEventArgs _) => Press();

    public void Press()
    {
        if (animating || Time.unscaledTime - lastPressTime < cooldown) return;
        lastPressTime = Time.unscaledTime;

        if (controller == null) controller = GetComponentInParent<KeypadController>();
        if (controller != null) controller.RegisterKey(this);

        StartCoroutine(PressRoutine());
    }

    private IEnumerator PressRoutine()
    {
        animating = true;
        Vector3 pressed = restPosition - new Vector3(0f, pressDepth, 0f);

        if (meshRenderer != null && pressedMaterial != null)
        {
            var mats = meshRenderer.materials;
            for (int i = 0; i < mats.Length; i++) mats[i] = pressedMaterial;
            meshRenderer.materials = mats;
        }

        float t = 0f;
        while (t < pressDuration) { t += Time.deltaTime; transform.localPosition = Vector3.Lerp(restPosition, pressed, t / pressDuration); yield return null; }
        yield return new WaitForSeconds(0.06f);
        t = 0f;
        while (t < pressDuration) { t += Time.deltaTime; transform.localPosition = Vector3.Lerp(pressed, restPosition, t / pressDuration); yield return null; }
        transform.localPosition = restPosition;

        if (meshRenderer != null && originalMaterials != null) meshRenderer.sharedMaterials = originalMaterials;
        animating = false;
    }
}