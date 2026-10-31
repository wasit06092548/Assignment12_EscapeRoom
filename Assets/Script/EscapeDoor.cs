using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EscapeDoor : MonoBehaviour
{
    [Header("ลากชิ้นส่วน (ตัวลูกของ DoorPadlock) มาใส่")]
    [SerializeField] private Transform padlockTransform;      // Padlock
    [SerializeField] private Transform padlockRingTransform;  // PadlockRing
    [SerializeField] private Transform lockingBarTransform;   // DoorLockingBar
    [SerializeField] private Transform doorTransform;         // Door
    [SerializeField] private Transform hingeTransform;        // DoorHinge (Empty ที่เราสร้าง)

    [Header("การตกของแม่กุญแจ")]
    [SerializeField] private float padlockFallDuration = 0.9f;
    [SerializeField] private Vector3 padlockFallOffset = new Vector3(0.06f, -0.7f, 0.05f);
    [SerializeField] private Vector3 padlockSpinEulers = new Vector3(240f, 320f, 160f);

    [Header("การเลื่อนของแท่งล็อก")]
    [SerializeField] private float barSlideDuration = 0.6f;
    [SerializeField] private Vector3 barSlideOffset = new Vector3(0f, 0.35f, 0f);

    [Header("การเปิดประตู")]
    [SerializeField] private float doorOpenDuration = 1.6f;
    [SerializeField] private float doorOpenAngle = 95f;   // ถ้าเปิดสวนทาง ให้ใส่ -95

    [Header("AudioSource ที่ใส่ DoorSound ไว้")]
    [SerializeField] private AudioSource doorAudioSource;

    [Header("Events")]
    [SerializeField] private UnityEvent onPadlockRemoved;
    [SerializeField] private UnityEvent onDoorOpened;

    private bool padlockRemoved;
    private bool doorOpened;

    [ContextMenu("Test Unlock Padlock")]
    public void UnlockPadlock()
    {
        if (padlockRemoved) return;
        padlockRemoved = true;
        if (padlockTransform != null) StartCoroutine(PadlockRoutine(padlockTransform));
        if (padlockRingTransform != null) StartCoroutine(PadlockRoutine(padlockRingTransform));
        onPadlockRemoved.Invoke();
    }

    [ContextMenu("Test Open Door")]
    public void OpenDoor()
    {
        if (doorOpened) return;
        doorOpened = true;
        StartCoroutine(DoorRoutine());
    }

    private IEnumerator PadlockRoutine(Transform part)
    {
        Vector3 startPos = part.localPosition;
        Quaternion startRot = part.localRotation;
        float t = 0f;
        while (t < padlockFallDuration)
        {
            t += Time.deltaTime;
            float k = t / padlockFallDuration;
            part.localPosition = startPos + padlockFallOffset * (k * k);          // ตกแบบเร่งความเร็ว
            part.localRotation = startRot * Quaternion.Euler(padlockSpinEulers * k);
            yield return null;
        }
        part.gameObject.SetActive(false);
    }

    private IEnumerator DoorRoutine()
    {
        // 1) เลื่อนแท่งล็อกขึ้น แล้วซ่อน
        if (lockingBarTransform != null)
        {
            Vector3 startPos = lockingBarTransform.localPosition;
            float t = 0f;
            while (t < barSlideDuration)
            {
                t += Time.deltaTime;
                lockingBarTransform.localPosition = startPos + barSlideOffset * (t / barSlideDuration);
                yield return null;
            }
            lockingBarTransform.gameObject.SetActive(false);
        }

        // 2) เปิดประตูหมุนรอบบานพับ
        if (doorAudioSource != null && !doorAudioSource.isPlaying) doorAudioSource.Play();

        if (doorTransform != null && hingeTransform != null)
        {
            Vector3 startPos = doorTransform.position;
            Quaternion startRot = doorTransform.rotation;
            Vector3 pivot = hingeTransform.position;
            Vector3 axis = hingeTransform.up;
            float t = 0f;
            while (t < doorOpenDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / doorOpenDuration);
                Quaternion rot = Quaternion.AngleAxis(doorOpenAngle * k, axis);
                doorTransform.SetPositionAndRotation(pivot + rot * (startPos - pivot), rot * startRot);
                yield return null;
            }
        }

        onDoorOpened.Invoke();
    }
}