using UnityEngine;

public class DestroyOutsideArena : MonoBehaviour
{
    public EnvironmentManager environmentManager;
    public float extraMargin = 3f;

    private void Start()
    {
        if (environmentManager == null)
        {
            environmentManager = GetComponentInParent<EnvironmentManager>();
        }
    }

    private void Update()
    {
        if (environmentManager == null) return;

        Vector3 p = transform.localPosition;
        Vector3 e = environmentManager.arenaHalfExtents;

        if (Mathf.Abs(p.x) > e.x + extraMargin ||
            p.y < -3f ||
            p.y > e.y + 8f ||
            Mathf.Abs(p.z) > e.z + extraMargin)
        {
            Destroy(gameObject);
        }
    }
}