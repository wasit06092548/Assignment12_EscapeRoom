using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class CardReaderLock : MonoBehaviour
{
    [Header("วัสดุสีเขียวเมื่ออ่านบัตรสำเร็จ (ลาก Green.mat)")]
    [SerializeField] private Material successMaterial;

    [Header("ตำแหน่ง/มุมที่บัตรจะถูกดูดไปวาง (local ของเครื่องอ่าน)")]
    [SerializeField] private Vector3 cardSlotLocalPosition = new Vector3(0f, 0f, 0.03f);
    [SerializeField] private Vector3 cardSlotLocalEulers = Vector3.zero;

    [Header("Event เมื่ออ่านบัตรสำเร็จ — ต่อกับ EscapeDoor.OpenDoor")]
    [SerializeField] private UnityEvent onCardAccepted;

    private readonly List<MeshRenderer> lightRenderers = new List<MeshRenderer>();
    private bool accepted;

    private void Awake()
    {
        // บังคับให้เป็น trigger (ใช้ตรวจว่าบัตรเสียบเข้ามา)
        var box = GetComponent<BoxCollider>();
        box.isTrigger = true;

        // หาไฟสัญญาณอัตโนมัติ (ชื่อมีคำว่า Light เช่น ReaderLight1/2)
        foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>(true))
        {
            if (meshRenderer.name.ToLower().Contains("light"))
                lightRenderers.Add(meshRenderer);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (accepted) return;
        var card = other.GetComponentInParent<Keycard>();
        if (card == null) return;

        accepted = true;

        // ดูดบัตรเข้าไปในช่องเสียบ
        card.DisableGrab();
        var cardRb = card.GetComponent<Rigidbody>();
        if (cardRb != null) { cardRb.isKinematic = true; cardRb.detectCollisions = false; }
        card.transform.SetParent(transform);
        card.transform.localPosition = cardSlotLocalPosition;
        card.transform.localEulerAngles = cardSlotLocalEulers;

        // เปลี่ยนไฟเป็นสีเขียว
        if (successMaterial != null)
        {
            foreach (var meshRenderer in lightRenderers)
            {
                var mats = meshRenderer.materials;
                for (int i = 0; i < mats.Length; i++) mats[i] = successMaterial;
                meshRenderer.materials = mats;
            }
        }

        onCardAccepted.Invoke();
    }
}