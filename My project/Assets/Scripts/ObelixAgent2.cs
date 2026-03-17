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

    private bool hasMenhir = false;
    private int deliveredCount = 0;
    private List<GameObject> spawnedObjects = new List<GameObject>();

    public override void OnEpisodeBegin()
    {
        // 1. Reset variabelen
        hasMenhir = false;
        deliveredCount = 0;

        // 2. Ruim alle oude objecten (menhirs en destinations) op
        foreach (var obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();

        // 3. Spawn nieuwe Menhirs
        for (int i = 0; i < spawnAmount; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(-4f, 4f), 1f, Random.Range(-4f, 4f));
            GameObject m = Instantiate(menhirPrefab, randomPos + transform.parent.position, Quaternion.identity, transform.parent);
            spawnedObjects.Add(m);
        }

        // 4. Spawn nieuwe Destinations
        for (int i = 0; i < spawnAmount; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));
            GameObject d = Instantiate(destinationPrefab, randomPos + transform.parent.position, Quaternion.identity, transform.parent);
            spawnedObjects.Add(d);
        }

        // 5. Reset Obelix positie
        this.transform.localPosition = new Vector3(0, 0.5f, 0);
        this.transform.localRotation = Quaternion.identity;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observatie 1: Heeft hij een menhir vast? (0 of 1)
        sensor.AddObservation(hasMenhir ? 1f : 0f);

        // Observatie 2: Hoeveel procent van de taak is voltooid? (0.0 tot 1.0)
        // Dit helpt de AI begrijpen dat de episode nog niet klaar is.
        sensor.AddObservation((float)deliveredCount / spawnAmount);

        // TOTAAL SPACE SIZE IN UNITY: 2
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // --- ACTIES ---
        int moveAction = actionBuffers.DiscreteActions[0]; // 0: niks, 1: voor, 2: achter
        int rotateAction = actionBuffers.DiscreteActions[1]; // 0: niks, 1: links, 2: rechts

        // Bewegen
        Vector3 move = Vector3.zero;
        if (moveAction == 1) move = transform.forward;
        else if (moveAction == 2) move = -transform.forward;
        transform.Translate(move * Time.deltaTime * moveSpeed, Space.World);

        // Draaien
        float rotation = 0f;
        if (rotateAction == 1) rotation = -1f;
        else if (rotateAction == 2) rotation = 1f;
        transform.Rotate(Vector3.up, rotation * Time.deltaTime * turnSpeed);

        // --- BELONINGEN ---
        // Kleine straf per stap om snelheid te stimuleren
        AddReward(-1f / MaxStep);

        // Straf als hij van het platform valt
        if (transform.localPosition.y < 0)
        {
            AddReward(-1.0f);
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1. Botsen met Menhir
        if (collision.gameObject.CompareTag("Menhir") && !hasMenhir)
        {
            hasMenhir = true;
            AddReward(0.3f); // Beloning voor oppakken
            collision.gameObject.SetActive(false); // Menhir verdwijnt ("op de rug")
        }

        // 2. Botsen met Destination
        if (collision.gameObject.CompareTag("Destination") && hasMenhir)
        {
            hasMenhir = false; // Obelix is weer vrij
            deliveredCount++;  // Teller ophogen

            AddReward(0.7f); // Beloning voor succesvolle aflevering
            collision.gameObject.SetActive(false); // Bestemming verdwijnt (is "gevuld")

            // Check of alle 6 menhirs zijn afgeleverd
            if (deliveredCount >= spawnAmount)
            {
                AddReward(1.0f); // Grote bonus voor volledige taak
                EndEpisode();    // NU pas de episode be indigen
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        // Vooruit/Achteruit
        if (Input.GetKey(KeyCode.W)) discreteActions[0] = 1;
        else if (Input.GetKey(KeyCode.S)) discreteActions[0] = 2;
        else discreteActions[0] = 0;

        // Draaien
        if (Input.GetKey(KeyCode.A)) discreteActions[1] = 1;
        else if (Input.GetKey(KeyCode.D)) discreteActions[1] = 2;
        else discreteActions[1] = 0;
    }
}