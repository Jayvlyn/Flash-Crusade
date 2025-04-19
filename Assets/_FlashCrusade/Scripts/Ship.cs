using System.Collections;
using UnityEditor;
using UnityEngine;

public class Ship : MonoBehaviour, IDamageable
{
    #region VARIABLES
    public ShipInputData inputData = new ShipInputData();

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
	private float currentMaxAcceleration;
	private float currentMaxSpeed;
	private bool boosting = false;
	private float boostFuel;

	private Vector2 velocity;
	public Vector2 Velocity
	{
		get { return velocity; }
		set
		{
			velocity = value;
			foreach(Weapon weapon in weapons)
			{
				weapon.SetShipVelocity(velocity);
			}
		}
	}

	[Header("Weapons")]

	[SerializeField] private Weapon[] weapons;

	[Header("Health")]
	[SerializeField] private int maxHealth = 100;
	private int health;

    #endregion

    #region UNITY METHODS

    private void Start()
	{
		health = maxHealth;
		currentMaxSpeed = maxSpeed;
		currentMaxAcceleration = maxAcceleration;
		boostFuel = maxBoostFuel;
		inputData.holdingFireWeapons = new bool[weapons.Length];
		if (objToMove == null) objToMove = this.gameObject;
	}

	private void Update()
	{
		Thrust(inputData.thrustInput);// Process thrust input. Returns truw when not Vector2.zero
		Turn(inputData.turnInput);
		for(int i = 0; i < inputData.holdingFireWeapons.Length; i++)
		{
			if(inputData.holdingFireWeapons[i]) weapons[i].Fire();
		}
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
			Velocity *= frameDrag;
		}

		float accelProgress = accelerationTime > 0 ? accelerateTimer / accelerationTime : 1f;
		float curveMultiplier = accelerationCurve.Evaluate(accelProgress);

		float extraTurnboost = 1;
		if (inputData.turnInput != 0)
		{
			extraTurnboost = 3;
		}

		Vector2 velocityDelta = targetVelocity - Velocity;
		Vector2 acceleration = Vector2.ClampMagnitude(velocityDelta / Time.deltaTime, currentMaxAcceleration * curveMultiplier * extraTurnboost);
		Velocity += acceleration * Time.deltaTime;

		objToMove.transform.position += (Vector3)(Velocity * Time.deltaTime);
		usingThrustersLastFrame = usingThrusters;

		return usingThrusters;
	}

	/// <summary>
	/// Double all thruster power, moving the ship faster.
	/// </summary>
	public void Boost(bool on) // default K
	{
        if (on && boostFuel > 1)
        { // turn on boost

			if (recoverBoostRoutine != null) RecoverEarlyStop();
			boosting = true;
			StartDepletingBoost();

			currentMaxAcceleration = maxBoostAcceleration;
			currentMaxSpeed = maxBoostSpeed;
        }
		else if(boosting)
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

    #region BOOST FUEL

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

			if (inputData.holdingBoost && inputData.thrustInput != Vector2.zero)
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
    #endregion

    public void TakeDamage(int damage)
    {
		health -= damage;
		if (health < 0)
		{
			health = 0;
			OnDeath();
		}
    }

	public virtual void OnDeath()
	{
		Destroy(gameObject);
	}
}
