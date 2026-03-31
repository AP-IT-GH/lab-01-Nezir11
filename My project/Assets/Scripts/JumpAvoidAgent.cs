using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(Rigidbody))]
public class JumpAvoidAgent : Agent
{
    [Header("Movement")]
    public float moveForce = 18f;
    public float maxHorizontalSpeed = 6f;
    public float jumpImpulse = 7.5f;
    public LayerMask groundMask;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Episode")]
    public float fallY = -2f;
    public float survivalRewardPerStep = 0.001f;
    public float jumpCost = -0.0005f;
    public float wallPenalty = -0.002f;

    [Header("References")]
    public EnvironmentManager environmentManager;

    private Rigidbody rb;
    private Vector3 startLocalPos;

    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startLocalPos = transform.localPosition;
    }

    public override void Initialize()
    {
        if (environmentManager == null)
        {
            environmentManager = GetComponentInParent<EnvironmentManager>();
        }
    }

    public override void OnEpisodeBegin()
    {
        if (environmentManager != null)
        {
            environmentManager.ResetEnvironment();
        }

        transform.localPosition = startLocalPos;
        transform.localRotation = Quaternion.identity;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void FixedUpdate()
    {
        IsGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        if (transform.localPosition.y < fallY)
        {
            AddReward(-1.0f);
            EndEpisode();
            return;
        }

        AddReward(survivalRewardPerStep);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        sensor.AddObservation(localVel.x / maxHorizontalSpeed);
        sensor.AddObservation(localVel.y / jumpImpulse);
        sensor.AddObservation(localVel.z / maxHorizontalSpeed);
        sensor.AddObservation(IsGrounded ? 1f : 0f);

        Vector3 localPos = transform.localPosition;
        sensor.AddObservation(localPos.x / environmentManager.arenaHalfExtents.x);
        sensor.AddObservation(localPos.z / environmentManager.arenaHalfExtents.z);

        environmentManager.WriteNearestObjectObservations(this, sensor);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int moveXAction = actions.DiscreteActions[0]; // 0 stay, 1 left, 2 right
        int moveZAction = actions.DiscreteActions[1]; // 0 stay, 1 back, 2 forward
        int jumpAction = actions.DiscreteActions[2];  // 0 no, 1 jump

        float moveX = 0f;
        float moveZ = 0f;

        if (moveXAction == 1) moveX = -1f;
        else if (moveXAction == 2) moveX = 1f;

        if (moveZAction == 1) moveZ = -1f;
        else if (moveZAction == 2) moveZ = 1f;

        Vector3 force = new Vector3(moveX, 0f, moveZ) * moveForce;
        rb.AddForce(force, ForceMode.Acceleration);

        Vector3 horizontal = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (horizontal.magnitude > maxHorizontalSpeed)
        {
            horizontal = horizontal.normalized * maxHorizontalSpeed;
            rb.linearVelocity = new Vector3(horizontal.x, rb.linearVelocity.y, horizontal.z);
        }

        if (jumpAction == 1 && IsGrounded)
        {
            rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
            AddReward(jumpCost);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var d = actionsOut.DiscreteActions;

        d[0] = 0;
        d[1] = 0;
        d[2] = 0;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) d[0] = 1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) d[0] = 2;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) d[1] = 1;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) d[1] = 2;

        if (Input.GetKey(KeyCode.Space)) d[2] = 1;
    }

    public void HitHazard(float penalty)
    {
        AddReward(penalty);
        EndEpisode();
    }

    public void CollectBonus(float reward)
    {
        AddReward(reward);
    }

    public void TouchWall()
    {
        AddReward(wallPenalty);
    }
}