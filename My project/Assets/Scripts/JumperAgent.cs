using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class JumperAgent : Agent
{
    [Header("Jump Settings")]
    public float jumpForce = 8f;
    public float jumpPenalty = 0.01f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundLayer;

    [Header("References")]
    public ObstacleSpawner spawner;

    private Rigidbody rb;
    private Vector3 startPos;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = startPos;
        transform.rotation = Quaternion.identity;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (spawner != null)
        {
            spawner.ResetSpawner();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(IsGrounded() ? 1f : 0f);
        sensor.AddObservation(rb.linearVelocity.y);
        sensor.AddObservation(transform.position.y);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];

        if (action == 1 && IsGrounded())
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                0f,
                rb.linearVelocity.z
            );

            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

            // Strafpunt bij elke sprong
            AddReward(-jumpPenalty);
        }

        // Kleine tijd penalty zodat hij efficiënt leert
        AddReward(-0.001f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        actionsOut.DiscreteActions.Array[0] =
            Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(
            groundCheck.position,
            Vector3.down,
            groundDistance,
            groundLayer
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("JumpObstacle"))
        {
            AddReward(-1f);
            EndEpisode();
        }

        if (other.CompareTag("BonusObstacle"))
        {
            AddReward(+1f);
            Destroy(other.gameObject);
        }
    }

    public void RewardPassedJumpObstacle()
    {
        AddReward(+0.6f);
    }

    public void PenaltyMissedBonus()
    {
        AddReward(-0.2f);
    }
}