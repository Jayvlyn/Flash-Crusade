using UnityEngine;

public class InterpolatedFollowTarget : MonoBehaviour
{
    public Transform target;

    private Vector3 lastPos;
    private Vector3 currentPos;

    private float lastRot;
    private float currentRot;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (target != null)
        {
            lastPos = currentPos = target.position;
            lastRot = currentRot = target.eulerAngles.z;

            transform.position = currentPos;
            transform.rotation = Quaternion.Euler(0, 0, currentRot);
        }
    }

    private void FixedUpdate()
    {
        if (target == null) return;

        lastPos = currentPos;
        currentPos = target.position;

        lastRot = currentRot;
        currentRot = target.eulerAngles.z;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float t = Time.deltaTime / Time.fixedDeltaTime;
        t = Mathf.Clamp01(t);

        Vector3 pos = Vector3.Lerp(lastPos, currentPos, t);
        float rot = Mathf.LerpAngle(lastRot, currentRot, t);

        transform.position = pos;
        transform.rotation = Quaternion.Euler(0, 0, rot);
    }
}