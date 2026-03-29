using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public class ObelixAgent2 : Agent
{
    [Header("Prefabs")]
    public GameObject menhirPrefab;
    public GameObject destinationPrefab;

    [Header("Settings")]
    public int spawnAmount = 6;
    public float moveSpeed = 5f;
    public float turnSpeed = 150f;

    [Header("Spawn Settings")]
    public float spawnRange = 2.5f;
    public float minDistance = 2.0f;
    public int maxSpawnAttempts = 20;

    private bool hasMenhir = false;
    private int deliveredCount = 0;

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private List<GameObject> menhirs = new List<GameObject>();
    private List<GameObject> destinations = new List<GameObject>();

    public override void OnEpisodeBegin()
    {
        hasMenhir = false;
        deliveredCount = 0;

        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        spawnedObjects.Clear();
        menhirs.Clear();
        destinations.Clear();

        List<Vector3> usedPositions = new List<Vector3>();

        // Spawn menhirs
        for (int i = 0; i < spawnAmount; i++)
        {
            Vector3 pos = GetValidSpawnPosition(usedPositions, 1f);
            usedPositions.Add(pos);

            GameObject m = Instantiate(
                menhirPrefab,
                pos + transform.parent.position,
                Quaternion.identity,
                transform.parent
            );

            m.SetActive(true);
            spawnedObjects.Add(m);
            menhirs.Add(m);
        }

        // Spawn destinations
        for (int i = 0; i < spawnAmount; i++)
        {
            Vector3 pos = GetValidSpawnPosition(usedPositions, 0.5f);
            usedPositions.Add(pos);

            GameObject d = Instantiate(
                destinationPrefab,
                pos + transform.parent.position,
                Quaternion.identity,
                transform.parent
            );

            d.SetActive(true);
            spawnedObjects.Add(d);
            destinations.Add(d);
        }

        transform.localPosition = new Vector3(0, 0.5f, 0);
        transform.localRotation = Quaternion.identity;
    }

    Vector3 GetValidSpawnPosition(List<Vector3> existingPositions, float height)
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 candidate = new Vector3(
                Random.Range(-spawnRange, spawnRange),
                height,
                Random.Range(-spawnRange, spawnRange)
            );

            bool valid = true;

            foreach (var pos in existingPositions)
            {
                if (Vector3.Distance(candidate, pos) < minDistance)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
                return candidate;
        }

        return new Vector3(
            Random.Range(-spawnRange, spawnRange),
            height,
            Random.Range(-spawnRange, spawnRange)
        );
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(hasMenhir ? 1f : 0f);
        sensor.AddObservation((float)deliveredCount / spawnAmount);

        // Direction to closest menhir
        Vector3 menhirDir = GetClosestDirection(menhirs);
        sensor.AddObservation(menhirDir.x);
        sensor.AddObservation(menhirDir.z);

        // Direction to closest destination
        Vector3 destDir = GetClosestDirection(destinations);
        sensor.AddObservation(destDir.x);
        sensor.AddObservation(destDir.z);
    }

    Vector3 GetClosestDirection(List<GameObject> objects)
    {
        float closestDist = float.MaxValue;
        Vector3 closestDir = Vector3.zero;

        foreach (var obj in objects)
        {
            if (obj != null && obj.activeSelf)
            {
                float dist = Vector3.Distance(transform.position, obj.transform.position);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestDir = (obj.transform.position - transform.position).normalized;
                }
            }
        }

        return closestDir;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int moveAction = actionBuffers.DiscreteActions[0];
        int rotateAction = actionBuffers.DiscreteActions[1];

        Vector3 move = Vector3.zero;
        if (moveAction == 1) move = transform.forward;
        else if (moveAction == 2) move = -transform.forward;

        transform.Translate(move * Time.deltaTime * moveSpeed, Space.World);

        float rotation = 0f;
        if (rotateAction == 1) rotation = -1f;
        else if (rotateAction == 2) rotation = 1f;

        transform.Rotate(Vector3.up, rotation * Time.deltaTime * turnSpeed);

        // Step penalty
        AddReward(-0.001f);

        // Distance reward (huge improvement)
        if (!hasMenhir)
        {
            AddReward(Vector3.Dot(transform.forward, GetClosestDirection(menhirs)) * 0.001f);
        }
        else
        {
            AddReward(Vector3.Dot(transform.forward, GetClosestDirection(destinations)) * 0.001f);
        }

        if (transform.localPosition.y < 0)
        {
            AddReward(-1.0f);
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Menhir") && !hasMenhir)
        {
            hasMenhir = true;
            AddReward(0.3f);
            collision.gameObject.SetActive(false);
        }

        if (collision.gameObject.CompareTag("Destination") && hasMenhir)
        {
            hasMenhir = false;
            deliveredCount++;

            AddReward(0.7f);
            collision.gameObject.SetActive(false);

            if (deliveredCount >= spawnAmount)
            {
                AddReward(1.0f);
                EndEpisode();
            }
        }

        if (collision.gameObject.CompareTag("Destination") && !hasMenhir)
        {
            AddReward(-0.2f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.W)) discreteActions[0] = 1;
        else if (Input.GetKey(KeyCode.S)) discreteActions[0] = 2;
        else discreteActions[0] = 0;

        if (Input.GetKey(KeyCode.A)) discreteActions[1] = 1;
        else if (Input.GetKey(KeyCode.D)) discreteActions[1] = 2;
        else discreteActions[1] = 0;
    }
}