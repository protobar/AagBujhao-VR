using UnityEngine;

/// <summary>
/// Place these in your level to mark where fires can spawn.
/// </summary>
public class FireSpawner : MonoBehaviour
{
    [Header("Fire Prefabs")]
    [Tooltip("Drag your fire prefabs here (Fire1, Fire2, etc). One will be randomly chosen.")]
    public GameObject[] firePrefabs;

    [Header("Spawn Settings")]
    public bool spawnOnStart = true;
    public float spawnDelay = 0f;
    public float fireScale = 1f;

    [Header("Fire Settings")]
    public float fireHealth = 100f;

    [Header("Gizmo")]
    public float gizmoSize = 0.5f;

    // Runtime
    private Fire currentFire;
    private GameUI gameUI;

    /// <summary>
    /// Does this spawner have an active fire?
    /// </summary>
    public bool HasFire => currentFire != null && currentFire.IsAlive;

    /// <summary>
    /// Get the current fire (if any)
    /// </summary>
    public Fire CurrentFire => currentFire;

    void Start()
    {
        gameUI = FindFirstObjectByType<GameUI>();

        if (spawnOnStart)
        {
            if (spawnDelay > 0)
                Invoke(nameof(SpawnFire), spawnDelay);
            else
                SpawnFire();
        }
    }

    /// <summary>
    /// Spawn a fire at this point.
    /// </summary>
    public void SpawnFire()
    {
        if (HasFire)
        {
            Debug.Log($"[FireSpawner] {name} already has a fire!");
            return;
        }

        if (firePrefabs == null || firePrefabs.Length == 0)
        {
            Debug.LogError($"[FireSpawner] {name} has no fire prefabs assigned!");
            return;
        }

        // Pick random prefab
        GameObject prefab = firePrefabs[Random.Range(0, firePrefabs.Length)];

        // Spawn fire
        GameObject fireObj = Instantiate(prefab, transform.position, transform.rotation);
        fireObj.transform.localScale = Vector3.one * fireScale;
        fireObj.name = $"Fire_{name}";

        // Get or add Fire component
        currentFire = fireObj.GetComponent<Fire>();
        if (currentFire == null)
        {
            currentFire = fireObj.AddComponent<Fire>();
        }

        // Apply settings
        currentFire.maxHealth = fireHealth;
        currentFire.currentHealth = fireHealth;

        // Register with GameUI
        if (gameUI != null)
            gameUI.RegisterFire(currentFire);

        Debug.Log($"[FireSpawner] Spawned fire at {name}");
    }

    /// <summary>
    /// Force spawn even if fire exists (destroys old fire).
    /// </summary>
    public void ForceSpawn()
    {
        if (currentFire != null)
        {
            Destroy(currentFire.gameObject);
            currentFire = null;
        }
        SpawnFire();
    }

    // Visual helper in editor
    void OnDrawGizmos()
    {
        // Orange sphere to show spawn point
        Gizmos.color = HasFire ? Color.red : new Color(1f, 0.5f, 0f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, gizmoSize);

        // Flame shape
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.8f);
        Vector3 top = transform.position + Vector3.up * gizmoSize * 2;
        Gizmos.DrawLine(transform.position + Vector3.left * gizmoSize * 0.3f, top);
        Gizmos.DrawLine(transform.position + Vector3.right * gizmoSize * 0.3f, top);
        Gizmos.DrawLine(transform.position, top);
    }

    void OnDrawGizmosSelected()
    {
        // Show label when selected
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, gizmoSize * 1.2f);
    }
}
