using UnityEngine;

public class SimpleParticleSpawner : MonoBehaviour
{
    [Header("Particle Settings")]
    public GameObject particlePrefab;
    public int particleCount = 20;
    public float spawnRadius = 2f;
    public float particleLifetime = 2f;

    public void SpawnCelebrationParticles()
    {
        for (int i = 0; i < particleCount; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            GameObject particle = Instantiate(particlePrefab, randomPos, Quaternion.identity);

            // Add some random movement
            Rigidbody2D rb = particle.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(Random.insideUnitCircle * 5f, ForceMode2D.Impulse);
            }

            // Destroy after lifetime
            Destroy(particle, particleLifetime);
        }
    }
}