using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Fire extinguisher that sprays when trigger is pressed while grabbed.
/// Attach to same GameObject as XRGrabInteractable.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class Extinguisher : MonoBehaviour
{
    [Header("Spray Effect")]
    [Tooltip("Your water/nitrogen particle effect")]
    public ParticleSystem sprayEffect;

    [Header("Spray Point")]
    [Tooltip("Where the spray originates from (nozzle tip)")]
    public Transform sprayPoint;
    [Tooltip("Direction the spray goes (choose which axis)")]
    public SprayDirection sprayDirection = SprayDirection.Forward;

    [Header("Raycast Settings")]
    [Tooltip("How far the spray reaches")]
    public float sprayRange = 5f;
    [Tooltip("Radius of spray detection")]
    public float sprayRadius = 0.3f;
    [Tooltip("Damage dealt per second")]
    public float damagePerSecond = 30f;

    [Header("Audio (Optional)")]
    public AudioSource sprayAudio;
    public AudioClip emptyTankSound;

    [Header("Tank System")]
    [Tooltip("Tank capacity in seconds of spray")]
    public float tankCapacity = 30f;
    public float currentTank;
    public bool infiniteTank = false;

    [Header("Type")]
    public bool isWaterType = true; // false = nitrogen

    [Header("Debug")]
    public bool showDebugRay = true;

    // Private
    private XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;
    private bool isSpraying = false;

    public bool IsSpraying => isSpraying;
    public float TankPercent => currentTank / tankCapacity;
    public bool IsEmpty => currentTank <= 0 && !infiniteTank;

    // Get spray direction based on selected axis
    Vector3 GetSprayDirection()
    {
        return sprayDirection switch
        {
            SprayDirection.Forward => sprayPoint.forward,
            SprayDirection.Back => -sprayPoint.forward,
            SprayDirection.Up => sprayPoint.up,
            SprayDirection.Down => -sprayPoint.up,
            SprayDirection.Right => sprayPoint.right,
            SprayDirection.Left => -sprayPoint.right,
            _ => sprayPoint.forward
        };
    }

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Default spray point to this transform if not set
        if (sprayPoint == null)
            sprayPoint = transform;

        // Make sure spray is off at start
        if (sprayEffect != null)
            sprayEffect.Stop();

        // Fill tank
        currentTank = tankCapacity;
    }

    void Update()
    {
        if (isSpraying)
        {
            DamageFiresInRange();
            ConsumeTank();
        }

        // Debug visualization
        if (showDebugRay && sprayPoint != null)
        {
            Debug.DrawRay(sprayPoint.position, GetSprayDirection() * sprayRange, 
                isSpraying ? Color.cyan : Color.gray);
        }
    }

    void ConsumeTank()
    {
        if (infiniteTank) return;

        currentTank -= Time.deltaTime;

        if (currentTank <= 0)
        {
            currentTank = 0;
            StopSpray();
            
            // Play empty sound
            if (emptyTankSound != null)
            {
                AudioSource.PlayClipAtPoint(emptyTankSound, transform.position, 0.7f);
            }
            
            Debug.Log("[Extinguisher] Tank empty!");
        }
    }

    /// <summary>
    /// Refill the tank completely
    /// </summary>
    public void Refill()
    {
        currentTank = tankCapacity;
        Debug.Log("[Extinguisher] Tank refilled!");
    }

    /// <summary>
    /// Refill by a specific amount
    /// </summary>
    public void Refill(float amount)
    {
        currentTank = Mathf.Min(currentTank + amount, tankCapacity);
    }

    void DamageFiresInRange()
    {
        Vector3 direction = GetSprayDirection();

        // SphereCast from spray point in spray direction
        RaycastHit[] hits = Physics.SphereCastAll(
            sprayPoint.position,
            sprayRadius,
            direction,
            sprayRange
        );

        foreach (var hit in hits)
        {
            // Try to find Fire component on hit object or parent
            Fire fire = hit.collider.GetComponentInParent<Fire>();
            
            if (fire != null && fire.IsAlive)
            {
                float damage = damagePerSecond * Time.deltaTime;
                fire.TakeDamage(damage, isWaterType);
            }
        }
    }

    void OnEnable()
    {
        // Subscribe to XR events
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
        grabInteractable.activated.AddListener(OnTriggerPressed);
        grabInteractable.deactivated.AddListener(OnTriggerReleased);
    }

    void OnDisable()
    {
        // Unsubscribe
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
        grabInteractable.activated.RemoveListener(OnTriggerPressed);
        grabInteractable.deactivated.RemoveListener(OnTriggerReleased);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        Debug.Log("[Extinguisher] Grabbed!");
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        StopSpray();
        Debug.Log("[Extinguisher] Released!");
    }

    void OnTriggerPressed(ActivateEventArgs args)
    {
        if (IsEmpty)
        {
            // Play empty sound
            if (emptyTankSound != null)
            {
                AudioSource.PlayClipAtPoint(emptyTankSound, transform.position, 0.7f);
            }
            Debug.Log("[Extinguisher] Tank is empty!");
            return;
        }
        StartSpray();
    }

    void OnTriggerReleased(DeactivateEventArgs args)
    {
        StopSpray();
    }

    void StartSpray()
    {
        if (isSpraying) return;

        isSpraying = true;

        if (sprayEffect != null)
            sprayEffect.Play();

        if (sprayAudio != null)
        {
            sprayAudio.loop = true;
            sprayAudio.Play();
        }

        Debug.Log("[Extinguisher] Spraying!");
    }

    void StopSpray()
    {
        if (!isSpraying) return;

        isSpraying = false;

        if (sprayEffect != null)
            sprayEffect.Stop();

        if (sprayAudio != null)
            sprayAudio.Stop();

        Debug.Log("[Extinguisher] Stopped spraying");
    }
}

public enum SprayDirection
{
    Forward,
    Back,
    Up,
    Down,
    Right,
    Left
}
