using UnityEngine;

public class ProceduralLevelGenerator : MonoBehaviour
{
    [Header("Prefabs to Spawn")]
    [SerializeField] private GameObject terrainPrefab;
    [SerializeField] private GameObject landingPadPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject fuelPickupPrefab;
    [SerializeField] private GameObject asteroidPrefab;

    [Header("Level Size")]
    [SerializeField] private float levelWidth = 80f;
    [SerializeField] private float levelHeight = 50f;

    private int levelNumber;

    public void GenerateLevel(int levelNum)
    {
        levelNumber = levelNum;

        Debug.Log($"Generating procedural level {levelNumber}");

        int difficulty = (levelNumber - 4) / 3;

        // Generate terrain first
        GenerateTerrain();

        // Generate level content
        GenerateLandingPads(difficulty);
        GenerateCoins(difficulty);
        GenerateFuelPickups(difficulty);
        GenerateAsteroids(difficulty);
    }

    private void GenerateTerrain()
    {
        if (terrainPrefab != null)
        {
            // Spawn terrain at the bottom
            Vector2 position = new Vector2(0, -levelHeight / 2 - 5);
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
        int padCount = Random.Range(2, 5);

        for (int i = 0; i < padCount; i++)
        {
            Vector2 position = GetRandomPosition();
            GameObject pad = Instantiate(landingPadPrefab, position, Quaternion.identity, transform);
            Debug.Log($"Spawned landing pad at {position}");
        }
    }

    private void GenerateCoins(int difficulty)
    {
        int coinCount = Mathf.Max(5, 10 - difficulty);

        for (int i = 0; i < coinCount; i++)
        {
            Vector2 position = GetRandomPosition();
            Instantiate(coinPrefab, position, Quaternion.identity, transform);
        }

        Debug.Log($"Spawned {coinCount} coins");
    }

    private void GenerateFuelPickups(int difficulty)
    {
        int fuelCount = Mathf.Max(2, 4 - difficulty / 2);

        for (int i = 0; i < fuelCount; i++)
        {
            Vector2 position = GetRandomPosition();
            Instantiate(fuelPickupPrefab, position, Quaternion.identity, transform);
        }

        Debug.Log($"Spawned {fuelCount} fuel pickups");
    }

    private void GenerateAsteroids(int difficulty)
    {
        int asteroidCount = 5 + (difficulty * 2);

        for (int i = 0; i < asteroidCount; i++)
        {
            Vector2 position = GetRandomPosition();

            GameObject asteroid = Instantiate(asteroidPrefab, position, Quaternion.identity, transform);

            float scale = Random.Range(1f, 2.5f);
            asteroid.transform.localScale = Vector3.one * scale;
            asteroid.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        }

        Debug.Log($"Spawned {asteroidCount} asteroids");
    }

    private Vector2 GetRandomPosition()
    {
        float x = Random.Range(-levelWidth / 2, levelWidth / 2);
        float y = Random.Range(-levelHeight / 2, levelHeight / 2);

        return new Vector2(x, y);
    }
}