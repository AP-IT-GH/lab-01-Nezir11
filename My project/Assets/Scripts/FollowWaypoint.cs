using UnityEngine;

public class FollowWaypoint : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;
    public float waypointReachDistance = 1.5f;

    [Header("Movement")]
    public float speed = 5f;
    public float rotSpeed = 3f;

    private int currentWaypoint = 0;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Als je physics gebruikt (Rigidbody), stuur dan liever in FixedUpdate.
        if (rb != null) return;

        MoveWithoutPhysics();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        MoveWithPhysics();
    }

    private void MoveWithoutPhysics()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypoint];

        // Richting + rotatie (smooth)
        Vector3 direction = target.position - transform.position;
        direction.y = 0f; // voorkomt kantelen op hellingen/verschillen in hoogte

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotSpeed);
        }

        // Vooruit bewegen in lokale Z-as
        transform.Translate(0f, 0f, speed * Time.deltaTime);

        // Check of we dicht genoeg zijn om naar volgende waypoint te gaan
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= waypointReachDistance)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }

    private void MoveWithPhysics()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypoint];

        // Richting + rotatie (smooth)
        Vector3 direction = target.position - rb.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Quaternion newRotation = Quaternion.Slerp(rb.rotation, lookRotation, Time.fixedDeltaTime * rotSpeed);
            rb.MoveRotation(newRotation);
        }

        // Vooruit bewegen (physics)
        Vector3 forwardMove = transform.forward * (speed * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + forwardMove);

        // Waypoint switch
        float distance = Vector3.Distance(rb.position, target.position);
        if (distance <= waypointReachDistance)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }
}