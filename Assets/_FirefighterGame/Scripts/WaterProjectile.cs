using UnityEngine;

/// <summary>
/// Water projectile that can hit targets.
/// Attach to water prefab.
/// </summary>
public class WaterProjectile : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Lifetime in seconds")]
    public float lifetime = 5f;
    [Tooltip("Damage on hit")]
    public int damage = 1;

    [Header("Effects")]
    public GameObject splashEffect;
    public AudioClip splashSound;

    void Start()
    {
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if hit a target
        MovingTarget target = other.GetComponent<MovingTarget>();
        if (target != null)
        {
            target.OnHit();
            CreateSplash();
            Destroy(gameObject);
            return;
        }

        // Hit ground or wall
        if (other.gameObject.layer == LayerMask.NameToLayer("Default") || 
            other.CompareTag("Ground") || 
            other.CompareTag("Wall"))
        {
            CreateSplash();
            Destroy(gameObject);
        }
    }

    void CreateSplash()
    {
        if (splashEffect != null)
        {
            Instantiate(splashEffect, transform.position, Quaternion.identity);
        }

        if (splashSound != null)
        {
            AudioSource.PlayClipAtPoint(splashSound, transform.position, 0.5f);
        }
    }
}


