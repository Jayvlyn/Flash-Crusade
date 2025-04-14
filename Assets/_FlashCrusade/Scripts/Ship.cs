using System.Collections;
using UnityEngine;

public class Ship : MonoBehaviour
{
	public ShipInputData currentInputData = new ShipInputData();

	[Header("Movement")]
	[SerializeField, Tooltip("Gameobject in heirarchy that ship will translate position of with movement (aka, parent of ship body)")]
	private GameObject objToMove;

	[SerializeField, Tooltip("Limits how fast ship can change velocity")]
	private float maxAcceleration = 10f;

	[SerializeField, Tooltip("Cap for linear velocity magnitude for regular thrust.")]
	private float maxSpeed = 5f;

	[SerializeField, Tooltip("Limits how fast ship can change velocity while boosting.")]
	private float maxBoostAcceleration = 20f;

	[SerializeField, Tooltip("Cap for linear velocity magnitude for boost.")]
	private float maxBoostSpeed = 10f;

	[SerializeField, Tooltip("How long the ship can boost in seconds.")]
	private float maxBoostFuel = 5f;

	[SerializeField, Tooltip("How long it takes to start recovering boost after boosting stops.")]
	private float recoverBoostDelay;

	[SerializeField, Tooltip("How long it takes to fully accelerate.")]
	private float accelerationTime = 0.5f;

	[SerializeField, Tooltip("Controls ramp-up behaviour.")]
	private AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

	[SerializeField, Tooltip("Angle per second for rotation.")]
	private float turnSpeed = 90f;

	[SerializeField, Tooltip("How many seconds it takes to fully stop from current speed when not accelerating.")]
	private float stopDuration = 2f;

	private bool usingThrustersLastFrame = false;
	private float accelerateTimer;
	private Vector2 velocity;
	private float currentMaxAcceleration;
	private float currentMaxSpeed;
	private bool boosting = false;
	private float boostFuel;


	[Header("Weapons")]

	[SerializeField] private Weapon weapon1;
	[SerializeField] private Weapon weapon2;
	[SerializeField] private Weapon weapon3;

	#region UNITY METHODS

	private void Start()
	{
		currentMaxSpeed = maxSpeed;
		currentMaxAcceleration = maxAcceleration;
		boostFuel = maxBoostFuel;

		if (objToMove == null)
		{
			objToMove = this.gameObject;
		}
	}

	private void Update()
	{
		Thrust(currentInputData.thrustInput)) // Process thrust input. Returns truw when not Vector2.zero
		Turn(currentInputData.turnInput);
	}

	#endregion

	#region MOVEMENT

	/// <summary>
	/// Thrust ship in any of the 8 directions
	/// </summary>
	/// <param name="direction">Direction of thrust</param>
	public bool Thrust(Vector2 direction) // default W-A-S-D
	{
		Vector2 worldDirection = (Vector2)objToMove.transform.up * direction.y + (Vector2)objToMove.transform.right * direction.x;

		worldDirection.Normalize();

		bool usingThrusters = worldDirection != Vector2.zero;
		Vector2 targetVelocity = worldDirection.normalized * currentMaxSpeed;

		if (usingThrusters)
		{
			if (!usingThrustersLastFrame) accelerateTimer = 0f;
			accelerateTimer += Time.deltaTime;
			accelerateTimer = Mathf.Min(accelerateTimer, accelerationTime);
		}
		else
		{
			accelerateTimer = 0f;

			float frameDrag = Mathf.Pow(0.01f, Time.deltaTime / stopDuration); // Drag calculated from duration to frame multiplier
			velocity *= frameDrag;
		}

		float accelProgress = accelerationTime > 0 ? accelerateTimer / accelerationTime : 1f;
		float curveMultiplier = accelerationCurve.Evaluate(accelProgress);

		float extraTurnboost = 1;
		if (currentInputData.turnInput != 0)
		{
			extraTurnboost = 3;
		}

		Vector2 velocityDelta = targetVelocity - velocity;
		Vector2 acceleration = Vector2.ClampMagnitude(velocityDelta / Time.deltaTime, currentMaxAcceleration * curveMultiplier * extraTurnboost);
		velocity += acceleration * Time.deltaTime;

		objToMove.transform.position += (Vector3)(velocity * Time.deltaTime);
		usingThrustersLastFrame = usingThrusters;

		return usingThrusters;
	}

	/// <summary>
	/// Double all thruster power, moving the ship faster.
	/// </summary>
	public void Boost(bool on) // default K
	{
        if (on)
        { // turn on boost

			if (recoverBoostRoutine != null) RecoverEarlyStop();
			boosting = true;
			StartDepletingBoost();

			currentMaxAcceleration = maxBoostAcceleration;
			currentMaxSpeed = maxBoostSpeed;
        }
		else if(boosting && boostFuel > 1)
		{ // turn off boost
			boosting = false;
			if (boostFuel < maxBoostFuel) StartRecoveringBoost();

			currentMaxAcceleration = maxAcceleration;
			currentMaxSpeed = maxSpeed;
		}
    }

	/// <summary>
	/// Rotate ship left or right depending on direction
	/// </summary>
	/// <param name="direction">-1=turn left | 1=turn right</param>
	private void Turn(float direction) // default J-L
	{
		float angle = direction * turnSpeed * Time.deltaTime;
		objToMove.transform.Rotate(0f, 0f, -angle);
	}

	#endregion

	#region WEAPONS

	public void FireWeapon1()
	{

	}

	public void FireWeapon2()
	{

	}

	public void FireWeapon3()
	{

	}

	#endregion

	private void StartDepletingBoost()
	{
		StopBothBoostRoutines();
		depleteBoostRoutine = StartCoroutine(DepleteBoostRoutine());
	}

	private void StartRecoveringBoost()
	{
		StopBothBoostRoutines();
		recoverBoostRoutine = StartCoroutine(RecoverBoostRoutine(2));
	}

	private Coroutine depleteBoostRoutine;
	private IEnumerator DepleteBoostRoutine()
	{
		while(boostFuel > 0)
		{
			boostFuel -= Time.deltaTime;
			yield return null;
		}
		boostFuel = 0;
		Boost(false);
		depleteBoostRoutine = null;
	}

	bool recoverJustStarted = true;
	private Coroutine recoverBoostRoutine;
	private IEnumerator RecoverBoostRoutine(float startDelay)
	{
		if(recoverJustStarted) yield return new WaitForSeconds(startDelay);

		recoverJustStarted = false;
		while(boostFuel < maxBoostFuel)
		{
			boostFuel += Time.deltaTime;

			if (currentInputData.holdingBoost && currentInputData.thrustInput != Vector2.zero)
			{
				Boost(true);
			}

			yield return null;
		}
		recoverJustStarted = true; // reset
		boostFuel = maxBoostFuel;
		recoverBoostRoutine = null;
	}

	private void RecoverEarlyStop()
	{
		recoverJustStarted = true;
		StopCoroutine(recoverBoostRoutine);
		recoverBoostRoutine = null;
	}

	private void StopBothBoostRoutines()
	{
		if (depleteBoostRoutine != null)
		{
			StopCoroutine(depleteBoostRoutine);
			depleteBoostRoutine = null;
		}
		if (recoverBoostRoutine != null)
		{
			RecoverEarlyStop();
		}
	}













	//if(boostToggleTimer!=null) StopCoroutine(boostToggleTimer);
	//boostToggleTimer = StartCoroutine(BoostToggleTimer(1f));

	//private Coroutine boostToggleTimer;
	//private IEnumerator BoostToggleTimer(float time)
	//{
	//	float startMaxAcceleration = currentMaxAcceleration;
	//	float startMaxSpeed = currentMaxSpeed;

	//	float elapsedTime = 0;
	//	while (elapsedTime < time)
	//	{
	//		elapsedTime += Time.deltaTime;
	//		if(boosting)
	//		{
	//			currentMaxAcceleration = Mathf.Lerp(startMaxAcceleration, maxBoostAcceleration, elapsedTime/time);
	//			currentMaxSpeed = Mathf.Lerp(startMaxSpeed, maxBoostSpeed, elapsedTime/time);
	//		}
	//		else
	//		{
	//			currentMaxAcceleration = Mathf.Lerp(startMaxAcceleration, maxAcceleration, elapsedTime / time);
	//			currentMaxSpeed = Mathf.Lerp(startMaxSpeed, maxSpeed, elapsedTime / time);
	//		}
	//		yield return null;
	//	}
	//}
}
