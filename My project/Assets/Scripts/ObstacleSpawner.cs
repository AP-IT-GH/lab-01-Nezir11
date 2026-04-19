using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject jumpObstaclePrefab;
    public GameObject bonusObstaclePrefab;
    public JumperAgent agent;

    [Header("Spawn Position")]
    public float spawnZ = 12f;
    public float laneX = 1f;
    public float spawnY = 0.5f;

    [Header("Spawn Timing")]
    public float minSpawnDelay = 8.0f;
    public float maxSpawnDelay = 10.0f;

    [Header("Extra Distance Check")]
    public float minDistanceBetweenObstacles = 8f;

    [Header("Speed Per Episode")]
    public float minEpisodeSpeed = 2.0f;
    public float maxEpisodeSpeed = 3.0f;

    private float currentEpisodeSpeed;
    private Coroutine spawnRoutine;
    private List<GameObject> spawnedObjects = new List<GameObject>();

    public void ResetSpawner()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }

        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i] != null)
            {
                Destroy(spawnedObjects[i]);
            }
        }

        spawnedObjects.Clear();

        currentEpisodeSpeed = Random.Range(minEpisodeSpeed, maxEpisodeSpeed);
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnObstacle();

            float waitTime = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void SpawnObstacle()
    {
        if (LastObstacleTooClose())
        {
            return;
        }

        GameObject prefab = Random.value < 0.5f ? jumpObstaclePrefab : bonusObstaclePrefab;

        Vector3 spawnPos = new Vector3(laneX, spawnY, spawnZ);
        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

        ObstacleMover mover = obj.GetComponent<ObstacleMover>();
        if (mover != null)
        {
            mover.Setup(agent, currentEpisodeSpeed);
        }

        spawnedObjects.Add(obj);
    }

    private bool LastObstacleTooClose()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] == null)
            {
                spawnedObjects.RemoveAt(i);
                continue;
            }

            float distance = Mathf.Abs(spawnZ - spawnedObjects[i].transform.position.z);
            if (distance < minDistanceBetweenObstacles)
            {
                return true;
            }

            return false;
        }

        return false;
    }
}