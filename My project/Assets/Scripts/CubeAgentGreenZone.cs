using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CubeAgentGreenZone : Agent
{
    public Transform Target;
    public Transform GoalZone;

    public float moveSpeed = 6f;
    public float turnSpeed = 200f;

    private bool touchedBlock = false;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(0, 0.5f, 0);
        transform.localRotation = Quaternion.identity;

        Target.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);

        Target.gameObject.SetActive(true);

        touchedBlock = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Rays geven de observaties
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float move = actionBuffers.ContinuousActions[0];
        float turn = actionBuffers.ContinuousActions[1];

        transform.Translate(Vector3.forward * move * moveSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up * turn * turnSpeed * Time.deltaTime);

        // kleine straf per stap
        AddReward(-0.001f);

        // gevallen
        if (transform.localPosition.y < 0)
        {
            AddReward(-1f);
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // blok geraakt
        if (other.transform == Target && !touchedBlock)
        {
            touchedBlock = true;

            AddReward(0.5f);

            Target.gameObject.SetActive(false);
        }

        // goal geraakt
        if (other.transform == GoalZone && touchedBlock)
        {
            AddReward(1f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;

        actions[0] = Input.GetAxis("Vertical");
        actions[1] = Input.GetAxis("Horizontal");
    }
}