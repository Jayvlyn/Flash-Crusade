using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thruster : MonoBehaviour
{
    [SerializeField] private ThrusterType thrusterType;
    public enum ThrusterType
    {
        TOP,
        LEFT,
        RIGHT
    }

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

    public enum ThrusterDir
    {
        UP, DOWN, LEFT, RIGHT
    }

	public List<ThrusterPose> thrusterPositions = new();
    private int activatedIndex;

    public float activateSpeed = 0.2f;


	public void Activate(ThrusterDir thrusterDir) // Indexes in order of left, up, down, right // only 0-2
    {
		switch (thrusterDir)
		{
			case ThrusterDir.UP: // POINT UP
                if(thrusterType == ThrusterType.LEFT)
                {
                    StartActivateCoroutine(1);
                }
				else if (thrusterType == ThrusterType.RIGHT)
				{
                    StartActivateCoroutine(2);
				}
				else // top
				{
                    StartActivateCoroutine(0);
				}
				break;
			case ThrusterDir.DOWN: // POINT DOWN
				if (thrusterType == ThrusterType.LEFT)
				{
                    StartActivateCoroutine(2);
				}
				else if (thrusterType == ThrusterType.RIGHT)
				{
					StartActivateCoroutine(1);
				}
				else // top
				{
                    StartDeactivateCoroutine(); // invalid
				}
				break;
			case ThrusterDir.LEFT: // POINT LEFT
				if (thrusterType == ThrusterType.LEFT)
				{
					StartActivateCoroutine(0);
				}
				else if (thrusterType == ThrusterType.RIGHT)
				{
                    StartDeactivateCoroutine(); // invalid
				}
				else // top
				{
                    StartActivateCoroutine(0);
				}
				break;
			case ThrusterDir.RIGHT: // POINT RIGHT
				if (thrusterType == ThrusterType.LEFT)
				{
					StartDeactivateCoroutine();
				}
				else if (thrusterType == ThrusterType.RIGHT)
				{
					StartActivateCoroutine(2);
				}
				else // top
				{
					StartActivateCoroutine(2);
				}
				break;
		}
	}

	public void Deactivate()
    {
        StartDeactivateCoroutine();
    }

	#region COROUTINES

	private void StartActivateCoroutine(int index)
    {
        if(activateCoroutine != null) StopCoroutine(activateCoroutine);
        activateCoroutine = StartCoroutine(ActivateCoroutine(0.2f, index));
    }

    private Coroutine activateCoroutine;
    private IEnumerator ActivateCoroutine(float activateTime, int index)
    {


        activatedIndex = index;
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
		activateCoroutine = null;
    }

	private void StartDeactivateCoroutine()
	{
		if (deactivateCoroutine != null) StopCoroutine(deactivateCoroutine);
		deactivateCoroutine = StartCoroutine(DeactivateCoroutine(0.2f));
	}

	public Coroutine deactivateCoroutine;
    private IEnumerator DeactivateCoroutine(float deactivateTime)
    {
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
        activatedIndex = -1;
		deactivateCoroutine = null;
    }

	#endregion

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
