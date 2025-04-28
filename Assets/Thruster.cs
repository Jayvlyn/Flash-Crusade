using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thruster : MonoBehaviour
{
    private enum State
    {
        INACTIVE,
        ACTIVATING,
        ACTIVE,
        DEACTIVATING
    }
    private State currentState;

    private void ChangeThrusterState(State state)
    {
        currentState = state;
	}

	public List<ThrusterPose> thrusterPositions = new();
    private int activatedIndex;

    public float activateSpeed = 0.2f;

	/// <summary>
	/// Activate thruster in position. If already active in that position will do nothing.
	/// <para />Top thruster: Left - Right
	/// <para />Left thruster: Left - Top - Down
	/// <para />Right thruster: Top - Down - Right
	/// </summary>
	/// <param name="posIndex">Index in this order: Left, Top, Down, Right</param>
	public void Activate(int posIndex)
    {
        if ((currentState == State.ACTIVE || currentState == State.ACTIVATING) && activatedIndex == posIndex) return; // tryig to activate what is already active/activating
		if(activateCoroutine != null) StopCoroutine(activateCoroutine); // shouldn't be running but just in case
        activateCoroutine = StartCoroutine(ActivateCoroutine(activateSpeed, posIndex));
    }

    public void Deactivate()
    {
        if(currentState == State.INACTIVE || currentState == State.DEACTIVATING) return; // trying to do what is already doing/done
		if (deactivateCoroutine != null) StopCoroutine(deactivateCoroutine); // shouldn't be running but just in case
		deactivateCoroutine = StartCoroutine(DeactivateCoroutine(activateSpeed));
    }

    private Coroutine activateCoroutine;
    private IEnumerator ActivateCoroutine(float activateTime, int index)
    {
        if(currentState == State.ACTIVE || currentState == State.ACTIVATING)
        {
            if(activateCoroutine != null)
            {
                yield return activateCoroutine;
            }
            deactivateCoroutine = StartCoroutine(DeactivateCoroutine(activateSpeed));
        }
        if (deactivateCoroutine != null)
        {
            yield return deactivateCoroutine;
        }

        activatedIndex = index;
        ChangeThrusterState(State.ACTIVATING);
        float t = 0;
        transform.localRotation = Quaternion.Euler(0, 0, thrusterPositions[activatedIndex].zRotation);
        Vector2 initialPosition = transform.localPosition;
        while(t < activateTime)
        {
            t += Time.deltaTime;

            transform.localPosition = Vector2.Lerp(initialPosition, thrusterPositions[activatedIndex].activePos, t / activateTime);

            yield return null;
        }
        transform.localPosition = thrusterPositions[activatedIndex].activePos;
        ChangeThrusterState(State.ACTIVE);
		activateCoroutine = null;
    }

    public Coroutine deactivateCoroutine;
    private IEnumerator DeactivateCoroutine(float deactivateTime)
    {
        if(currentState == State.ACTIVATING)
        {
            yield return activateCoroutine;
        }

        ChangeThrusterState(State.DEACTIVATING);
        float t = 0;
        transform.localRotation = Quaternion.Euler(0, 0, thrusterPositions[activatedIndex].zRotation);
		Vector2 initialPosition = transform.localPosition;
		while (t < deactivateTime)
        {
            t += Time.deltaTime;

            transform.localPosition = Vector2.Lerp(initialPosition, thrusterPositions[activatedIndex].inactivePos, t / deactivateTime);

            yield return null;
        }
        transform.localPosition = thrusterPositions[activatedIndex].inactivePos;
        ChangeThrusterState(State.INACTIVE);
        activatedIndex = -1;
		deactivateCoroutine = null;
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
