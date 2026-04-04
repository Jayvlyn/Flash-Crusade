using UnityEngine;

public class TransformFollower : MonoBehaviour
{
    public Transform target;
    public bool useFixedUpdate;

    void Update()
    {
        if (useFixedUpdate) return;

        transform.position = target.position;
    }

    private void FixedUpdate()
    {
        if (!useFixedUpdate) return;

        transform.position = target.position;
    }
}
