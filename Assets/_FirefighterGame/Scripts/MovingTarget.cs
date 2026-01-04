using UnityEngine;

/// <summary>
/// Target that moves left and right (like arcade shooting gallery).
/// Can be hit by water projectiles.
/// </summary>
public class MovingTarget : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Leftmost position (local or world)")]
    public Vector3 leftPosition;
    [Tooltip("Rightmost position (local or world)")]
    public Vector3 rightPosition;
    [Tooltip("Movement speed")]
    public float moveSpeed = 2f;
    [Tooltip("Use local positions (relative to start position)")]
    public bool useLocalPositions = true;

    [Header("Behavior")]
    [Tooltip("Start moving left or right")]
    public bool startMovingRight = true;
    [Tooltip("Pause at edges")]
    public bool pauseAtEdges = false;
    [Tooltip("Pause duration at edges")]
    public float pauseDuration = 0.5f;

    [Header("Scoring")]
    [Tooltip("Points awarded when hit")]
    public int pointsOnHit = 100;
    [Tooltip("Points for destroying target")]
    public int pointsOnDestroy = 500;

    [Header("Health")]
    [Tooltip("How many hits to destroy")]
    public int health = 1;
    [Tooltip("Destroy on hit")]
    public bool destroyOnHit = true;

    [Header("Effects")]
    public GameObject hitEffect;
    public GameObject destroyEffect;
    public AudioClip hitSound;
    public AudioClip destroySound;

    // Runtime
    private Vector3 startPosition;
    private bool movingRight;
    private float pauseTimer = 0f;
    private int currentHealth;
    private bool isDestroyed = false;

    void Start()
    {
        startPosition = transform.position;
        currentHealth = health;
        movingRight = startMovingRight;

        // Convert local to world positions if needed
        if (useLocalPositions)
        {
            leftPosition = startPosition + leftPosition;
            rightPosition = startPosition + rightPosition;
        }
    }

    void Update()
    {
        if (isDestroyed) return;

        // Handle pause at edges
        if (pauseAtEdges && pauseTimer > 0)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }

        // Move
        Vector3 targetPos = movingRight ? rightPosition : leftPosition;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // Check if reached edge
        float distanceToTarget = Vector3.Distance(transform.position, targetPos);
        if (distanceToTarget < 0.1f)
        {
            // Reverse direction
            movingRight = !movingRight;

            if (pauseAtEdges)
                pauseTimer = pauseDuration;
        }
    }

    /// <summary>
    /// Called when hit by water projectile.
    /// </summary>
    public void OnHit()
    {
        if (isDestroyed) return;

        currentHealth--;

        // Play hit effect
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, 0.7f);
        }

        // Award points
        ShootingGalleryGame game = FindFirstObjectByType<ShootingGalleryGame>();
        if (game != null)
        {
            game.AddScore(pointsOnHit);
        }

        // Check if destroyed
        if (currentHealth <= 0 || destroyOnHit)
        {
            DestroyTarget();
        }
    }

    void DestroyTarget()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // Play destroy effect
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }

        if (destroySound != null)
        {
            AudioSource.PlayClipAtPoint(destroySound, transform.position, 0.7f);
        }

        // Award destroy points
        ShootingGalleryGame game = FindFirstObjectByType<ShootingGalleryGame>();
        if (game != null)
        {
            game.AddScore(pointsOnDestroy);
            game.OnTargetDestroyed();
        }

        // Destroy or hide
        Destroy(gameObject, 0.1f);
    }

    void OnDrawGizmosSelected()
    {
        // Draw movement path
        Vector3 left = useLocalPositions ? transform.position + leftPosition : leftPosition;
        Vector3 right = useLocalPositions ? transform.position + rightPosition : rightPosition;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(left, right);
        Gizmos.DrawWireSphere(left, 0.2f);
        Gizmos.DrawWireSphere(right, 0.2f);
    }
}


