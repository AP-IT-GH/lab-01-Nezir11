using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ObelixAgent : Agent
{
    public float moveSpeed = 5f;
    public float rotateSpeed = 200f;

    private Rigidbody rb;

    private bool carryingMenhir = false;
    private GameObject carriedMenhir;

    public Transform carryPoint;

    public Transform[] menhirs;
    public Transform[] destinations;

    public Transform environmentRoot;

    public float spawnRange = 25f;

    private int remainingMenhirs;
    private bool[] destinationUsed;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();

        destinationUsed = new bool[destinations.Length];
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(0, 0.5f, 0);
        transform.rotation = Quaternion.identity;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        ResetMenhir();

        remainingMenhirs = menhirs.Length;

        // reset destination status
        for (int i = 0; i < destinationUsed.Length; i++)
        {
            destinationUsed[i] = false;
        }

        // spawn menhirs
        foreach (Transform m in menhirs)
        {
            m.position = environmentRoot.position + GetRandomPosition();
            m.rotation = Quaternion.identity;
            m.gameObject.SetActive(true);
        }

        // spawn destinations
        foreach (Transform d in destinations)
        {
            d.position = environmentRoot.position + GetRandomPosition();
            d.rotation = Quaternion.identity;
            d.gameObject.SetActive(true);
        }
    }

    void ResetMenhir()
    {
        if (carriedMenhir != null)
        {
            Rigidbody rb = carriedMenhir.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            Collider col = carriedMenhir.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = true;
            }

            carriedMenhir.transform.SetParent(null);
            carriedMenhir = null;
        }

        carryingMenhir = false;
    }

    Vector3 GetRandomPosition()
    {
        return new Vector3(
            Random.Range(-spawnRange, spawnRange),
            0.5f,
            Random.Range(-spawnRange, spawnRange)
        );
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(carryingMenhir ? 1 : 0);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float move = actions.ContinuousActions[0];
        float rotate = actions.ContinuousActions[1];

        transform.Rotate(Vector3.up, rotate * rotateSpeed * Time.deltaTime);
        rb.MovePosition(transform.position + transform.forward * move * moveSpeed * Time.deltaTime);

        AddReward(-0.001f);

        if (transform.localPosition.y < -1f)
        {
            AddReward(-1.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;

        actions[0] = Input.GetAxis("Vertical");
        actions[1] = Input.GetAxis("Horizontal");
    }

    private void OnCollisionEnter(Collision collision)
    {
        // PICKUP
        if (collision.gameObject.CompareTag("Menhir") && !carryingMenhir)
        {
            carryingMenhir = true;
            carriedMenhir = collision.gameObject;

            Rigidbody rb = carriedMenhir.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            Collider col = carriedMenhir.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            carriedMenhir.transform.SetParent(carryPoint);
            carriedMenhir.transform.localPosition = Vector3.zero;

            AddReward(0.3f);
        }

        // DELIVERY
        if (collision.gameObject.CompareTag("Destination") && carryingMenhir)
        {
            AddReward(0.7f);

            // disable menhir
            carriedMenhir.SetActive(false);

            // disable destination
            collision.gameObject.SetActive(false);

            ResetMenhir();

            remainingMenhirs--;

            if (remainingMenhirs <= 0)
            {
                EndEpisode();
            }
        }

        // WRONG BEHAVIOR
        if (collision.gameObject.CompareTag("Destination") && !carryingMenhir)
        {
            AddReward(-0.2f);
        }
    }
}