using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public int MaxEnemies = 4;

    public List<Vector2> SpawnPoints = new List<Vector2>();

    private int m_totalEnemiesSpawned;
    private List<Vector2> m_availableSpawnPoints;

    private void Start()
    {
        m_availableSpawnPoints = new List<Vector2>(SpawnPoints);
        StartCoroutine(SpawnAllEnemiesWithDelay());
    }

    // Reset the state of the EnemySpawner
    public void ResetSpawner()
    {
        // Remove any existing child enemies
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        m_totalEnemiesSpawned = 0;

        // Clear the available spawn points list before re-initializing it
        m_availableSpawnPoints.Clear();
        m_availableSpawnPoints.AddRange(SpawnPoints);

        StartCoroutine(SpawnAllEnemiesWithDelay());
    }

    private IEnumerator SpawnAllEnemiesWithDelay()
    {
        yield return new WaitForSeconds(1f);
        SpawnAllEnemies();
    }

    private void SpawnAllEnemies()
    {
        while (m_totalEnemiesSpawned < MaxEnemies)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (m_availableSpawnPoints.Count == 0)
        {
            Debug.LogWarning("No available spawn points.");
            return;
        }

        // Select a random spawn point from the list of available spawn points
        Vector2 selectedSpawnPoint = m_availableSpawnPoints[Random.Range(0, m_availableSpawnPoints.Count)];
        Vector2 spawnPosition = (Vector2)transform.position + selectedSpawnPoint;

        // Instantiate the enemy at the chosen spawn point
        GameObject newEnemy = Instantiate(EnemyPrefab, spawnPosition, Quaternion.identity);

        // Set the parent of the new enemy to this EnemySpawner GameObject
        newEnemy.transform.parent = this.transform;

        ++m_totalEnemiesSpawned;
        m_availableSpawnPoints.Remove(selectedSpawnPoint); // Remove the used spawn point from the list
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        foreach (Vector2 spawnPoint in SpawnPoints)
        {
            Vector2 worldPosition = (Vector2)transform.position + spawnPoint;
            Gizmos.DrawWireSphere(worldPosition, 0.5f);
        }
    }
}