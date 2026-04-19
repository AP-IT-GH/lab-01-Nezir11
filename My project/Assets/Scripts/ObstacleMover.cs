using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float destroyZ = -6f;

    private JumperAgent agent;
    private bool rewardGiven = false;

    public void Setup(JumperAgent targetAgent, float speed)
    {
        agent = targetAgent;
        moveSpeed = speed;
    }

    private void Update()
    {
        transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.World);

        if (!rewardGiven && agent != null && CompareTag("JumpObstacle"))
        {
            if (transform.position.z < agent.transform.position.z - 0.3f)
            {
                rewardGiven = true;
                agent.RewardPassedJumpObstacle();
            }
        }

        if (transform.position.z < destroyZ)
        {
            Destroy(gameObject);
        }
    }
}