using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Water gun that shoots water prefabs when trigger is pressed.
/// For shooting gallery game mode.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class WaterGun : MonoBehaviour
{
    [Header("Water Projectile")]
    [Tooltip("Water prefab to shoot")]
    public GameObject waterPrefab;

    [Header("Shooting")]
    [Tooltip("Where water spawns from (nozzle tip)")]
    public Transform shootPoint;
    [Tooltip("Shooting force")]
    public float shootForce = 10f;
    [Tooltip("Shooting rate (shots per second)")]
    public float fireRate = 3f;
    [Tooltip("Spread angle in degrees")]
    public float spreadAngle = 2f;

    [Header("Audio")]
    public AudioSource shootAudio;
    public AudioClip shootSound;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;

    // Runtime
    private XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;
    private float lastShotTime = 0f;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (shootPoint == null)
            shootPoint = transform;
    }

    void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
        grabInteractable.activated.AddListener(OnTriggerPressed);
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
        grabInteractable.activated.RemoveListener(OnTriggerPressed);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }

    void OnTriggerPressed(ActivateEventArgs args)
    {
        Shoot();
    }

    void Update()
    {
        // Auto-fire while trigger held (optional - remove if you want single shots only)
        // Uncomment if you want continuous fire
        /*
        if (isGrabbed && Input.GetButton("Fire1"))
        {
            Shoot();
        }
        */
    }

    void Shoot()
    {
        // Check fire rate
        if (Time.time - lastShotTime < 1f / fireRate)
            return;

        lastShotTime = Time.time;

        if (waterPrefab == null)
        {
            Debug.LogWarning("[WaterGun] No water prefab assigned!");
            return;
        }

        // Spawn water projectile
        Vector3 spawnPos = shootPoint.position;
        Quaternion spawnRot = shootPoint.rotation;

        // Add random spread
        Vector3 spread = Random.insideUnitSphere * spreadAngle;
        spawnRot = shootPoint.rotation * Quaternion.Euler(spread);

        GameObject water = Instantiate(waterPrefab, spawnPos, spawnRot);

        // Add force to water
        Rigidbody rb = water.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(shootPoint.forward * shootForce, ForceMode.VelocityChange);
        }

        // Play effects
        if (muzzleFlash != null)
            muzzleFlash.Play();

        if (shootAudio != null && shootSound != null)
            shootAudio.PlayOneShot(shootSound);
    }
}


