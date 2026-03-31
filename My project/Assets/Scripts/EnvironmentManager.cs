using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;

public class EnvironmentManager : MonoBehaviour
{
    [Header("Agent")]
    public JumpAvoidAgent agent;

    [Header("Arena")]
    public Vector3 arenaHalfExtents = new Vector3(8f, 2f, 8f);

    [Header("Episode difficulty")]
    public float minObstacleSpeed = 2f;
    public float maxObstacleSpeed = 8f;

    [Header("Observation")]
    [Range(1, 12)]
    public int maxObservedObjects = 8;
    public float maxObservationDistance = 20f;

    [Header("Feature toggles")]
    public bool useLaneObstacles = true;
    public bool useBonusCollisionObstacles = true;
    public bool useFlyingBonusObjects = true;
    public bool useCrossroadMode = true;

    [Header("Spawners")]
    public ObstacleSpawner[] spawners;

    private readonly List<TrackableObject> activeObjects = new List<TrackableObject>();

    public float CurrentEpisodeSpeed { get; private set; }

    private void Start()
    {
        ResetEnvironment();
    }

    public void ResetEnvironment()
    {
        CurrentEpisodeSpeed = Random.Range(minObstacleSpeed, maxObstacleSpeed);

        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            if (activeObjects[i] != null)
            {
                Destroy(activeObjects[i].gameObject);
            }
        }

        activeObjects.Clear();

        foreach (var spawner in spawners)
        {
            if (spawner != null)
            {
                spawner.ResetSpawner(this);
            }
        }
    }

    public void RegisterObject(TrackableObject obj)
    {
        if (!activeObjects.Contains(obj))
            activeObjects.Add(obj);
    }

    public void UnregisterObject(TrackableObject obj)
    {
        activeObjects.Remove(obj);
    }

    public void WriteNearestObjectObservations(JumpAvoidAgent observingAgent, VectorSensor sensor)
    {
        List<TrackableObject> valid = new List<TrackableObject>();

        foreach (var obj in activeObjects)
        {
            if (obj == null) continue;

            float dist = Vector3.Distance(observingAgent.transform.position, obj.transform.position);
            if (dist <= maxObservationDistance)
            {
                valid.Add(obj);
            }
        }

        valid.Sort((a, b) =>
        {
            float da = Vector3.SqrMagnitude(a.transform.position - observingAgent.transform.position);
            float db = Vector3.SqrMagnitude(b.transform.position - observingAgent.transform.position);
            return da.CompareTo(db);
        });

        int count = Mathf.Min(maxObservedObjects, valid.Count);

        for (int i = 0; i < count; i++)
        {
            WriteObjectObservation(observingAgent, sensor, valid[i]);
        }

        for (int i = count; i < maxObservedObjects; i++)
        {
            WriteEmptyObservation(sensor);
        }
    }

    private void WriteObjectObservation(JumpAvoidAgent observingAgent, VectorSensor sensor, TrackableObject obj)
    {
        Vector3 rel = observingAgent.transform.InverseTransformPoint(obj.transform.position);
        Vector3 relVel = observingAgent.transform.InverseTransformDirection(obj.CurrentVelocity);

        sensor.AddObservation(rel.x / arenaHalfExtents.x);
        sensor.AddObservation(rel.y / 5f);
        sensor.AddObservation(rel.z / arenaHalfExtents.z);

        sensor.AddObservation(relVel.x / maxObstacleSpeed);
        sensor.AddObservation(relVel.y / maxObstacleSpeed);
        sensor.AddObservation(relVel.z / maxObstacleSpeed);

        sensor.AddObservation(obj.objectType == TrackableObject.ObjectType.Hazard ? 1f : 0f);
        sensor.AddObservation(obj.objectType == TrackableObject.ObjectType.BonusCollision ? 1f : 0f);
        sensor.AddObservation(obj.objectType == TrackableObject.ObjectType.FlyingBonus ? 1f : 0f);
        sensor.AddObservation(obj.requiresJump ? 1f : 0f);
    }

    private void WriteEmptyObservation(VectorSensor sensor)
    {
        for (int i = 0; i < 10; i++)
            sensor.AddObservation(0f);
    }
}