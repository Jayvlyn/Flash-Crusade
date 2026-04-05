using UnityEngine;

public class TransformFollower : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public bool useFixedUpdate;

    void Update()
    {
        if (useFixedUpdate) return;

        transform.position = target.position + offset;
    }

    private void FixedUpdate()
    {
        if (!useFixedUpdate) return;

        transform.position = target.position + offset;
    }
}
