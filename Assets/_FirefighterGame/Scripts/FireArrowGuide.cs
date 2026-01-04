using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Arrow that guides player to the nearest active fire.
/// Updates automatically when fires are extinguished.
/// </summary>
public class FireArrowGuide : MonoBehaviour
{
    [Header("Arrow Display")]
    [Tooltip("3D Arrow object (should be child of camera or positioned in front)")]
    public Transform arrowTransform;
    [Tooltip("Distance text (optional - can be 3D TextMesh or UI)")]
    public TextMeshProUGUI distanceText;
    [Tooltip("3D TextMesh for distance (alternative to UI text)")]
    public TextMesh distanceText3D;
    [Tooltip("Arrow color (for materials)")]
    public Color arrowColor = Color.red;

    [Header("Settings")]
    [Tooltip("How to choose which fire to point to")]
    public TargetMode targetMode = TargetMode.Nearest;
    [Tooltip("Minimum distance to show arrow")]
    public float minDistance = 2f;
    [Tooltip("Maximum distance to show arrow")]
    public float maxDistance = 100f;
    [Tooltip("Arrow rotation speed")]
    public float rotationSpeed = 5f;
    [Tooltip("Arrow position offset from camera (local space if child of camera)")]
    public Vector3 arrowOffset = new Vector3(0, -0.2f, 1f);
    [Tooltip("Is arrow a child of camera? (uses local offset)")]
    public bool isChildOfCamera = true;

    [Header("Animation")]
    public bool pulseArrow = true;
    public float pulseSpeed = 2f;
    public float pulseScale = 0.2f;
    public float baseScale = 1f;

    [Header("Distance Display")]
    public bool showDistance = true;
    public string distanceFormat = "{0:F1}m";

    // Runtime
    private Camera playerCamera;
    private Fire currentTarget;
    private Vector3 baseArrowScale;
    private float pulseTimer = 0f;

    public enum TargetMode
    {
        Nearest,        // Closest fire
        First,          // First fire found
        Largest         // Fire with most health
    }

    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindFirstObjectByType<Camera>();

        if (arrowTransform == null)
            arrowTransform = transform;

        baseArrowScale = arrowTransform.localScale;

        // Set arrow color for UI Image
        Image arrowImage = arrowTransform.GetComponent<Image>();
        if (arrowImage != null)
            arrowImage.color = arrowColor;

        // Set arrow color for 3D material
        Renderer arrowRenderer = arrowTransform.GetComponent<Renderer>();
        if (arrowRenderer != null && arrowRenderer.material != null)
        {
            arrowRenderer.material.color = arrowColor;
        }
    }

    void Update()
    {
        UpdateTarget();
        UpdateArrow();
        UpdateDistanceText();
    }

    void UpdateTarget()
    {
        // Find all active fires
        Fire[] allFires = FindObjectsByType<Fire>(FindObjectsSortMode.None)
            .Where(f => f.IsAlive)
            .ToArray();

        if (allFires.Length == 0)
        {
            currentTarget = null;
            arrowTransform.gameObject.SetActive(false);
            return;
        }

        // Choose target based on mode
        Fire newTarget = null;

        switch (targetMode)
        {
            case TargetMode.Nearest:
                newTarget = GetNearestFire(allFires);
                break;

            case TargetMode.First:
                newTarget = allFires[0];
                break;

            case TargetMode.Largest:
                newTarget = GetLargestFire(allFires);
                break;
        }

        // Update target if changed
        if (newTarget != currentTarget)
        {
            currentTarget = newTarget;
            arrowTransform.gameObject.SetActive(true);
        }
    }

    Fire GetNearestFire(Fire[] fires)
    {
        if (fires.Length == 0 || playerCamera == null) return null;

        Fire nearest = null;
        float nearestDist = float.MaxValue;
        Vector3 cameraPos = playerCamera.transform.position;

        foreach (var fire in fires)
        {
            float dist = Vector3.Distance(cameraPos, fire.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = fire;
            }
        }

        return nearest;
    }

    Fire GetLargestFire(Fire[] fires)
    {
        if (fires.Length == 0) return null;

        Fire largest = null;
        float maxHealth = 0f;

        foreach (var fire in fires)
        {
            if (fire.currentHealth > maxHealth)
            {
                maxHealth = fire.currentHealth;
                largest = fire;
            }
        }

        return largest;
    }

    void UpdateArrow()
    {
        if (currentTarget == null || playerCamera == null)
        {
            arrowTransform.gameObject.SetActive(false);
            return;
        }

        Vector3 cameraPos = playerCamera.transform.position;
        Vector3 targetPos = currentTarget.transform.position;
        float distance = Vector3.Distance(cameraPos, targetPos);

        // Hide if too close or too far
        if (distance < minDistance || distance > maxDistance)
        {
            arrowTransform.gameObject.SetActive(false);
            return;
        }

        arrowTransform.gameObject.SetActive(true);

        // Position arrow relative to camera
        if (isChildOfCamera && arrowTransform.parent == playerCamera.transform)
        {
            // Use local offset if child of camera
            arrowTransform.localPosition = arrowOffset;
        }
        else
        {
            // World space positioning
            arrowTransform.position = playerCamera.transform.position + 
                playerCamera.transform.TransformDirection(arrowOffset);
        }

        // Rotate arrow to point at target
        Vector3 direction = (targetPos - arrowTransform.position).normalized;
        direction.y = 0; // Keep arrow horizontal

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            arrowTransform.rotation = Quaternion.Slerp(
                arrowTransform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Pulse animation
        if (pulseArrow)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float scale = baseScale + Mathf.Sin(pulseTimer) * pulseScale;
            arrowTransform.localScale = baseArrowScale * scale;
        }
    }

    void UpdateDistanceText()
    {
        if (!showDistance) return;
        if (currentTarget == null || playerCamera == null)
        {
            if (distanceText != null) distanceText.gameObject.SetActive(false);
            if (distanceText3D != null) distanceText3D.gameObject.SetActive(false);
            return;
        }

        float distance = Vector3.Distance(
            playerCamera.transform.position,
            currentTarget.transform.position
        );

        if (distance < minDistance || distance > maxDistance)
        {
            if (distanceText != null) distanceText.gameObject.SetActive(false);
            if (distanceText3D != null) distanceText3D.gameObject.SetActive(false);
            return;
        }

        string distanceStr = string.Format(distanceFormat, distance);

        // Update UI text
        if (distanceText != null)
        {
            distanceText.gameObject.SetActive(true);
            distanceText.text = distanceStr;
        }

        // Update 3D text
        if (distanceText3D != null)
        {
            distanceText3D.gameObject.SetActive(true);
            distanceText3D.text = distanceStr;
            
            // Make 3D text face camera
            if (playerCamera != null)
            {
                distanceText3D.transform.LookAt(playerCamera.transform);
                distanceText3D.transform.Rotate(0, 180, 0);
            }
        }
    }

    /// <summary>
    /// Force update target (call when fire is extinguished)
    /// </summary>
    public void RefreshTarget()
    {
        currentTarget = null;
        UpdateTarget();
    }

    /// <summary>
    /// Set arrow visibility
    /// </summary>
    public void SetVisible(bool visible)
    {
        arrowTransform.gameObject.SetActive(visible);
        if (distanceText != null)
            distanceText.gameObject.SetActive(visible && showDistance);
    }
}

