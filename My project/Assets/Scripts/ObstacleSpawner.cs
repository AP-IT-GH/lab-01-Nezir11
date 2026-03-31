using System.Collections;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public enum SpawnMode
    {
        LaneForward,
        CrossroadX,
        CrossroadZ,
        FlyingBonus
    }

    [Header("General")]
    public SpawnMode spawnMode = SpawnMode.LaneForward;
    public EnvironmentManager environmentManager;

    [Header("Prefabs")]
    public TrackableObject hazardPrefab;
    public TrackableObject bonusCollisionPrefab;
    public TrackableObject flyingBonusPrefab;

    [Header("Timing")]
    public float minSpawnInterval = 0.8f;
    public float maxSpawnInterval = 1.8f;

    [Header("Spawn positions")]
    public Transform[] spawnPoints;

    [Header("Feature usage")]
    public bool spawnHazards = true;
    public bool spawnBonusCollisionObjects = false;
    public bool spawnFlyingBonuses = false;

    [Range(0f, 1f)]
    public float bonusCollisionChance = 0.25f;

    private Coroutine spawnRoutine;

    public void ResetSpawner(EnvironmentManager manager)
    {
        environmentManager = manager;

        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));

        while (true)
        {
            if (ShouldSpawnForCurrentMode())
            {
                SpawnOne();
            }

            float wait = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(wait);
        }
    }

    private bool ShouldSpawnForCurrentMode()
    {
        if (environmentManager == null) return false;

        switch (spawnMode)
        {
            case SpawnMode.LaneForward:
                return environmentManager.useLaneObstacles;

            case SpawnMode.CrossroadX:
            case SpawnMode.CrossroadZ:
                return environmentManager.useCrossroadMode;

            case SpawnMode.FlyingBonus:
                return environmentManager.useFlyingBonusObjects;

            default:
                return true;
        }
    }

    private void SpawnOne()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        TrackableObject prefabToSpawn = ChoosePrefab();
        if (prefabToSpawn == null) return;

        TrackableObject obj = Instantiate(prefabToSpawn, point.position, point.rotation, transform);
        obj.environmentManager = environmentManager;
        obj.speed = environmentManager.CurrentEpisodeSpeed;

        SetupMovement(obj, point);
    }

    private TrackableObject ChoosePrefab()
    {
        if (spawnMode == SpawnMode.FlyingBonus && spawnFlyingBonuses)
        {
            return flyingBonusPrefab;
        }

        if (spawnBonusCollisionObjects &&
            environmentManager.useBonusCollisionObstacles &&
            Random.value < bonusCollisionChance)
        {
            return bonusCollisionPrefab;
        }

        if (spawnHazards)
            return hazardPrefab;

        return null;
    }

    private void SetupMovement(TrackableObject obj, Transform point)
    {
        switch (spawnMode)
        {
            case SpawnMode.LaneForward:
                obj.moveDirection = -point.forward;
                break;

            case SpawnMode.CrossroadX:
                obj.moveDirection = (Vector3.zero - point.localPosition).normalized;
                break;

            case SpawnMode.CrossroadZ:
                obj.moveDirection = (Vector3.zero - point.localPosition).normalized;
                break;

            case SpawnMode.FlyingBonus:
                obj.moveDirection = -point.forward;
                obj.speed *= 1.1f;
                break;
        }
    }
}