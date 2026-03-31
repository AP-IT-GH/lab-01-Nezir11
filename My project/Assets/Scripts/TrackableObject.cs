using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TrackableObject : MonoBehaviour
{
    public enum ObjectType
    {
        Hazard,
        BonusCollision,
        FlyingBonus
    }

    [Header("Type")]
    public ObjectType objectType = ObjectType.Hazard;
    public bool requiresJump = true;

    [Header("Rewards")]
    public float hazardPenalty = -1.0f;
    public float bonusReward = 0.6f;

    [Header("Movement")]
    public Vector3 moveDirection = Vector3.left;
    public float speed = 4f;

    [HideInInspector] public EnvironmentManager environmentManager;

    private Vector3 lastPosition;
    public Vector3 CurrentVelocity { get; private set; }

    protected virtual void Start()
    {
        lastPosition = transform.position;

        if (environmentManager == null)
            environmentManager = GetComponentInParent<EnvironmentManager>();

        if (environmentManager != null)
            environmentManager.RegisterObject(this);
    }

    protected virtual void OnDestroy()
    {
        if (environmentManager != null)
            environmentManager.UnregisterObject(this);
    }

    protected virtual void Update()
    {
        transform.position += moveDirection.normalized * speed * Time.deltaTime;
        CurrentVelocity = (transform.position - lastPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPosition = transform.position;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        JumpAvoidAgent agent = other.GetComponent<JumpAvoidAgent>();
        if (agent == null) return;

        switch (objectType)
        {
            case ObjectType.Hazard:
                agent.HitHazard(hazardPenalty);
                break;

            case ObjectType.BonusCollision:
                agent.CollectBonus(bonusReward);
                Destroy(gameObject);
                break;

            case ObjectType.FlyingBonus:
                agent.CollectBonus(bonusReward);
                Destroy(gameObject);
                break;
        }
    }
}