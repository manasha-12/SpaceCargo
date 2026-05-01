using UnityEngine;

public class ProceduralLevelGenerator : MonoBehaviour
{
    [Header("Prefabs to Spawn")]
    [SerializeField] private GameObject terrainPrefab;
    [SerializeField] private GameObject landingPadPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject fuelPickupPrefab;
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private GameObject windZonePrefab;
    [SerializeField] private GameObject slowFallPowerUpPrefab;
    [SerializeField] private GameObject dronePrefab;

    [Header("Level Size")]
    [SerializeField] private float levelWidth = 80f;
    [SerializeField] private float levelHeight = 50f;
    [SerializeField] private float terrainHeight = -25f; // Y position of terrain top

    private int levelNumber;
    private Vector2 randomLanderStartPosition;

    public void GenerateLevel(int levelNum)
    {
        levelNumber = levelNum;

        Debug.Log($"Generating procedural level {levelNumber}");

        int difficulty = (levelNumber - 4) / 3;

        // Generate random lander position FIRST
        GenerateRandomLanderPosition();

        // Generate terrain
        GenerateTerrain();

        // Generate level content
        GenerateLandingPads(difficulty);
        GenerateCoins(difficulty);
        GenerateFuelPickups(difficulty);
        GenerateAsteroids(difficulty);
        GenerateWindZones(difficulty);
        GenerateSlowFallPowerUps(difficulty);
        GenerateDrones(difficulty);

        // Update lander's actual position
        UpdateLanderPosition();
    }

    private void GenerateRandomLanderPosition()
    {
        // Random X position within level bounds
        float randomX = Random.Range(-levelWidth / 3, levelWidth / 3);

        // Spawn near top of level (well above terrain)
        float spawnY = levelHeight / 2 - 10;

        randomLanderStartPosition = new Vector2(randomX, spawnY);

        Debug.Log($"Random lander start position: {randomLanderStartPosition}");
    }

    private void UpdateLanderPosition()
    {
        // Move the lander to the random position
        if (Lander.Instance != null)
        {
            Lander.Instance.transform.position = randomLanderStartPosition;
            Debug.Log($"Moved lander to: {randomLanderStartPosition}");
        }
    }

    private void GenerateTerrain()
    {
        if (terrainPrefab != null)
        {
            Vector2 position = new Vector2(0, terrainHeight);
            Instantiate(terrainPrefab, position, Quaternion.identity, transform);
            Debug.Log($"Spawned terrain at {position}");
        }
        else
        {
            Debug.LogWarning("Terrain prefab not assigned!");
        }
    }

    private void GenerateLandingPads(int difficulty)
    {
        // ALWAYS place one landing pad directly below the lander's RANDOM position
        // Make sure it's ABOVE the terrain
        Vector2 easyPadPosition = new Vector2(
            randomLanderStartPosition.x,
            randomLanderStartPosition.y - 10
        );

        // Ensure it's above terrain
        if (easyPadPosition.y < terrainHeight + 5)
        {
            easyPadPosition.y = terrainHeight + 5;
        }

        GameObject easyPad = Instantiate(landingPadPrefab, easyPadPosition, Quaternion.identity, transform);
        Debug.Log($"Spawned EASY landing pad below lander at {easyPadPosition}");

        // Add 1-3 additional random landing pads ABOVE terrain
        int additionalPads = Random.Range(1, 4);

        for (int i = 0; i < additionalPads; i++)
        {
            Vector2 position = GetRandomPositionAboveTerrain();
            GameObject pad = Instantiate(landingPadPrefab, position, Quaternion.identity, transform);
            Debug.Log($"Spawned additional landing pad at {position}");
        }
    }

    private void GenerateCoins(int difficulty)
    {
        int coinCount = Mathf.Max(5, 10 - difficulty);

        for (int i = 0; i < coinCount; i++)
        {
            Vector2 position = GetRandomPositionAboveTerrain();
            Instantiate(coinPrefab, position, Quaternion.identity, transform);
        }

        Debug.Log($"Spawned {coinCount} coins");
    }

    private void GenerateFuelPickups(int difficulty)
    {
        int fuelCount = Mathf.Max(2, 4 - difficulty / 2);

        for (int i = 0; i < fuelCount; i++)
        {
            Vector2 position = GetRandomPositionAboveTerrain();
            Instantiate(fuelPickupPrefab, position, Quaternion.identity, transform);
        }

        Debug.Log($"Spawned {fuelCount} fuel pickups");
    }

    private void GenerateAsteroids(int difficulty)
    {
        int asteroidCount = 5 + (difficulty * 2);

        for (int i = 0; i < asteroidCount; i++)
        {
            Vector2 position = GetRandomPositionAboveTerrain();

            GameObject asteroid = Instantiate(asteroidPrefab, position, Quaternion.identity, transform);

            float scale = Random.Range(1f, 2.5f);
            asteroid.transform.localScale = Vector3.one * scale;
            asteroid.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        }

        Debug.Log($"Spawned {asteroidCount} asteroids");
    }

    private void GenerateWindZones(int difficulty)
    {
        if (windZonePrefab == null)
        {
            Debug.LogWarning("WindZone prefab not assigned!");
            return;
        }

        // Spawn 2-4 wind zones
        int windZoneCount = Random.Range(2, 5);

        for (int i = 0; i < windZoneCount; i++)
        {
            Vector2 position = GetRandomPositionAboveTerrain();
            Instantiate(windZonePrefab, position, Quaternion.identity, transform);
        }

        Debug.Log($"Spawned {windZoneCount} wind zones");
    }

    private void GenerateSlowFallPowerUps(int difficulty)
    {
        if (slowFallPowerUpPrefab == null)
        {
            Debug.LogWarning("SlowFallPowerUp prefab not assigned!");
            return;
        }

        // Spawn 1-2 slow fall power-ups
        int powerUpCount = Random.Range(1, 3);

        for (int i = 0; i < powerUpCount; i++)
        {
            Vector2 position = GetRandomPositionAboveTerrain();
            Instantiate(slowFallPowerUpPrefab, position, Quaternion.identity, transform);
        }

        Debug.Log($"Spawned {powerUpCount} slow fall power-ups");
    }

    private Vector2 GetRandomPositionAboveTerrain()
    {
        float x = Random.Range(-levelWidth / 2, levelWidth / 2);

        // Only spawn ABOVE the terrain (at least 5 units above terrain top)
        float minY = terrainHeight + 5;
        float maxY = levelHeight / 2;
        float y = Random.Range(minY, maxY);

        return new Vector2(x, y);
    }

    // Old method - kept for compatibility but not used
    private Vector2 GetRandomPosition()
    {
        return GetRandomPositionAboveTerrain();
    }

    private void GenerateDrones(int difficulty)
    {
        if (dronePrefab == null)
        {
            Debug.LogWarning("Drone prefab not assigned!");
            return;
        }

        // Spawn more drones as difficulty increases
        int droneCount = 1 + difficulty; // Level 4 = 1 drone, Level 7 = 2 drones, etc.
        droneCount = Mathf.Min(droneCount, 5); // Cap at 5 drones max

        for (int i = 0; i < droneCount; i++)
        {
            Vector2 position = GetRandomPositionAboveTerrain();

            // Make sure drone doesn't spawn too close to lander
            while (Vector2.Distance(position, randomLanderStartPosition) < 10f)
            {
                position = GetRandomPositionAboveTerrain();
            }

            GameObject drone = Instantiate(dronePrefab, position, Quaternion.identity, transform);

            // Optional: Make drones faster on higher levels
            Drone droneScript = drone.GetComponent<Drone>();
            if (droneScript != null)
            {
                // Increase speed based on difficulty
                // You can adjust this in the inspector per-drone if you want
            }

            Debug.Log($"Spawned drone at {position}");
        }

        Debug.Log($"Spawned {droneCount} drones for difficulty {difficulty}");
    }
}