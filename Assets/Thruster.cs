using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thruster : MonoBehaviour
{
    public List<ThrusterPose> thrusterPositions = new();
    private int activatedIndex;

    public float activateSpeed = 0.2f;

    private bool active;

    [Button("Active1")]
    public void Activate1()
    {
        StartCoroutine(ActivateCoroutine(activateSpeed, 0));

        active = true;
    }

    [Button("Active2")]
    public void Activate2()
    {
        StartCoroutine(ActivateCoroutine(activateSpeed, 1));

        active = true;
    }

    public void Activate(int posIndex)
    {        
        StartCoroutine(ActivateCoroutine(activateSpeed, posIndex));

        active = true;
    }

    [Button("Deactive")]
    public void Deactivate()
    {
        StartCoroutine(DeactivateCoroutine(activateSpeed));
        active = false;
    }

    private IEnumerator ActivateCoroutine(float activateTime, int index)
    {
        // if was already active, deactivate prev state before
        if (active)
        {
            yield return StartCoroutine(DeactivateCoroutine(activateTime));
        }
        activatedIndex = index;

        float t = 0;
        transform.localRotation = Quaternion.Euler(0, 0, thrusterPositions[activatedIndex].zRotation);
        transform.localPosition = thrusterPositions[activatedIndex].inactivePos;
        while(t < activateTime)
        {
            t += Time.deltaTime;

            transform.localPosition = Vector2.Lerp(thrusterPositions[activatedIndex].inactivePos, thrusterPositions[activatedIndex].activePos, t / activateTime);

            yield return null;
        }
        transform.localPosition = thrusterPositions[activatedIndex].activePos;
    }

    private IEnumerator DeactivateCoroutine(float deactivateTime)
    {
        float t = 0;
        transform.localRotation = Quaternion.Euler(0, 0, thrusterPositions[activatedIndex].zRotation);
        transform.localPosition = thrusterPositions[activatedIndex].activePos;
        while (t < deactivateTime)
        {
            t += Time.deltaTime;

            transform.localPosition = Vector2.Lerp(thrusterPositions[activatedIndex].activePos, thrusterPositions[activatedIndex].inactivePos, t / deactivateTime);

            yield return null;
        }
        transform.localPosition = thrusterPositions[activatedIndex].inactivePos;
    }

    [Button("Set Inactive Position")]
    private void SetInactive()
    {
        var thisPos = thrusterPositions[^1];
        thrusterPositions[^1] = new ThrusterPose(transform.localPosition, thisPos.activePos, thisPos.zRotation);
    }

    [Button("Set Active Position")]
    private void SetActive()
    {
        var thisPos = thrusterPositions[^1];
        thrusterPositions[^1] = new ThrusterPose(thisPos.inactivePos, transform.localPosition, thisPos.zRotation);
    }

    [Button("Set Z Rotation")]
    private void SetRotation()
    {
        var thisPos = thrusterPositions[^1];
        float z = transform.localEulerAngles.z;
        thrusterPositions[^1] = new ThrusterPose(thisPos.inactivePos, thisPos.activePos, z);
    }

    [Button("Add Empty Thruster Slot")]
    private void AddNewThruster()
    {
        thrusterPositions.Add(new ThrusterPose(Vector2.zero, Vector2.zero, 0f));
    }
}
