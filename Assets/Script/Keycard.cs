using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
public class Keycard : MonoBehaviour
{
    [Header("ถ้าบัตรหล่นตกจากแมพ ให้เด้งกลับที่เดิม")]
    [SerializeField] private bool respawnIfDropped = true;
    [SerializeField] private float fallLimitY = -2f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        startPosition = transform.position;
        startRotation = transform.rotation;

        if (grabInteractable == null)
            Debug.LogWarning("[Keycard] ยังไม่มี XR Grab Interactable บนบัตร — เพิ่มใน Inspector ด้วย", this);
    }

    private void Update()
    {
        if (respawnIfDropped && transform.position.y < fallLimitY) Respawn();
    }

    public void Respawn()
    {
        transform.SetPositionAndRotation(startPosition, startRotation);
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void DisableGrab()
    {
        if (grabInteractable != null) grabInteractable.enabled = false;
    }
}