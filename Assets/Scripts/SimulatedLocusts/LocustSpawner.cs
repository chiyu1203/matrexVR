using UnityEngine;

public class LocustSpawner : MonoBehaviour
{
    public GameObject locustPrefab;
    public int numberOfLocusts;
    public float spawnAreaSize;
    public float mu = 0.0f;  // mu value for Van Mises
    public float kappa = 10000f;  // kappa value for Van Mises
    public float locustSpeed = 0.1f;      // Default speed for locusts

    void Start()
    {
        SpawnLocusts();
    }

    void SpawnLocusts()
    {
        for (int i = 0; i < numberOfLocusts; i++)
        {
            Vector3 spawnPosition = new Vector3(
                Random.Range(-spawnAreaSize / 2, spawnAreaSize / 2),
                0,
                Random.Range(-spawnAreaSize / 2, spawnAreaSize / 2)
            );

            GameObject locust = Instantiate(locustPrefab, spawnPosition, Quaternion.identity);
            locust.transform.localRotation = GenerateVanMisesRotation(mu, kappa);  // Set the local rotation
            locust.GetComponent<LocustMover>().speed = locustSpeed;  // Set the speed of the locust
            // Get the Animator component
            Animator locustAnimator = locust.GetComponent<Animator>();

            if (locustAnimator)
            {
                // Assuming the animation state you want to desynchronize is the default one at layer 0
                AnimatorStateInfo state = locustAnimator.GetCurrentAnimatorStateInfo(0);
                locustAnimator.Play(state.fullPathHash, -1, Random.value);  // Random normalized time between 0 and 1
            }
        }
    }

    Quaternion GenerateVanMisesRotation(float mu, float kappa)
    {
        float angle = VanMisesDistribution.Generate(mu, kappa);
        Debug.Log("Generated Angle (in degrees): " + angle * Mathf.Rad2Deg);
        return Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0);  // Convert radians to degrees for the Quaternion rotation
    }
}

public static class VanMisesDistribution
{
    public static float Generate(float mu, float kappa)
    {
        float s = 0.5f / kappa;
        float r = s + Mathf.Sqrt(1 + s * s);
        
        while (true)
        {
            float u1 = Random.value;
            float z = Mathf.Cos(Mathf.PI * u1);
            float f = (1 + r * z) / (r + z);
            float c = kappa * (r - f);

            float u2 = Random.value;

            if (u2 < c)
            {
                float u3 = Random.value;
                float sign = (u3 > 0.5f) ? 1.0f : -1.0f;
                return mu + sign * Mathf.Acos(f);
            }
            else if (u2 <= c + Mathf.Exp(-kappa))
            {
                float u3 = Random.value;
                return mu + Mathf.PI * (2 * u3 - 1);
            }
        }
    }
}
