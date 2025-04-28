using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Ship : MonoBehaviour, IDamageable
{
    #region VARIABLES
    private ShipInputData inputData = new ShipInputData();
	public ShipInputData InputData { 
		get {
			return inputData; 
		} 
		set { 
			inputData = value;
		}	
	}

	[Header("Movement")]
	[SerializeField, Tooltip("Gameobject in heirarchy that ship will translate position of with movement (aka, parent of ship body)")]
	private GameObject objToMove;

	[SerializeField, Tooltip("Limits how fast ship can change velocity")]
	private float maxAcceleration = 1000f;

	[SerializeField, Tooltip("Cap for linear velocity magnitude for regular thrust.")]
	private float maxSpeed = 500f;

	[SerializeField, Tooltip("Limits how fast ship can change velocity while boosting.")]
	private float maxBoostAcceleration = 2000f;

	[SerializeField, Tooltip("Cap for linear velocity magnitude for boost.")]
	private float maxBoostSpeed = 10000f;

	[SerializeField, Tooltip("How long the ship can boost in seconds.")]
	private float maxBoostFuel = 7f;

	[SerializeField, Tooltip("How long it takes to start recovering boost after boosting stops.")]
	private float recoverBoostDelay;

	[SerializeField, Tooltip("How long it takes to fully accelerate.")]
	private float accelerationTime = 0.5f;

	[SerializeField, Tooltip("Controls ramp-up behaviour.")]
	private AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

	[SerializeField, Tooltip("Angle per second for rotation.")]
	private float turnSpeed = 90f;

	[SerializeField, Tooltip("Angle per second for rotation while boosting.")]
	private float boostTurnSpeed = 180f;

	[SerializeField, Tooltip("How many seconds it takes to fully stop from current speed when not accelerating.")]
	public float stopDuration = 2f;

	private bool usingThrustersLastFrame;
	private float accelerateTimer;
	[HideInInspector] public float currentMaxAcceleration;
	[HideInInspector] public Vector2 acceleration;
	private float currentMaxSpeed;
	private bool boosting;
	private float boostFuel;
	public bool isTurning { get { return InputData.turnInput != 0; } }

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

	[SerializeField] private Weapon[] weapons;

	[Header("Health")]
	[SerializeField] private int maxHealth = 100;
	private int health;
	
    [InfoBox("Order thrusters in list as the following: Left, Top, Right")]
    [SerializeField] private List<Thruster> thrusters = new();
	private Thruster LeftThruster { get {  return thrusters[0]; } }
	private Thruster TopThruster { get {  return thrusters[1]; } }
	private Thruster RightThruster { get {  return thrusters[2]; } }

    #endregion

    #region UNITY METHODS

    private void Start()
	{
		health = maxHealth;
		currentMaxSpeed = maxSpeed;
		currentMaxAcceleration = maxAcceleration;
		boostFuel = maxBoostFuel;
		InputData.holdingFireWeapons = new bool[weapons.Length];
		if (objToMove == null) objToMove = this.gameObject;
	}

	private void Update()
	{
		Thrust(inputData.thrustInput);
		//Debug.Log(gameObject.GetInstanceID() + " " + inputData.thrustInput);

		if (isTurning)
		{
			Turn(InputData.turnInput);
		}

		for(int i = 0; i < InputData.holdingFireWeapons.Length; i++)
		{
			if(InputData.holdingFireWeapons[i]) weapons[i].Fire();
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
		if (InputData.turnInput != 0)
		{
			extraTurnboost = 3;
		}

		Vector2 velocityDelta = targetVelocity - Velocity;
		acceleration = Vector2.ClampMagnitude(velocityDelta / Time.deltaTime, currentMaxAcceleration * curveMultiplier * extraTurnboost);
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
		float speed = boosting ? boostTurnSpeed : turnSpeed;
		float angle = direction * speed * Time.deltaTime;

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

			if (InputData.holdingBoost && InputData.thrustInput != Vector2.zero)
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

    #region THRUSTER VISUALS
    [Button("Find Thrusters")]
    private void FindThrusters()
    {
        thrusters.Clear();

        foreach (Transform child in GetComponentsInChildren<Transform>(true)) // true = include inactive
        {
            if (child.name.ToLower().Contains("thruster"))
            {
                Thruster found = child.GetComponent<Thruster>();
                if (found != null && !thrusters.Contains(found))
                {
                    thrusters.Add(found);
                    Debug.Log($"Added thruster: {found.name}");
                }
            }
        }
    }

	public void UpdateActiveThrusters()
	{
		
		if(inputData.isMovingOrTurning)
		{
			if (inputData.thrustInput == Vector2.left) // on left input, point right
			{
				TopThruster.Activate(Thruster.ThrusterDir.RIGHT);
				RightThruster.Activate(Thruster.ThrusterDir.RIGHT);
				LeftThruster.Deactivate();
			}
			else if (inputData.thrustInput == Vector2.right) // on right input, point left
			{
				TopThruster.Activate(Thruster.ThrusterDir.LEFT);
				LeftThruster.Activate(Thruster.ThrusterDir.LEFT);
				RightThruster.Deactivate();
			}
			else if (inputData.thrustInput == Vector2.up)
			{
				TopThruster.Deactivate();
				RightThruster.Activate(Thruster.ThrusterDir.DOWN);
				LeftThruster.Activate(Thruster.ThrusterDir.DOWN);
			}
			else if (inputData.thrustInput == Vector2.down)
			{
				LeftThruster.Activate(Thruster.ThrusterDir.UP);
				RightThruster.Activate(Thruster.ThrusterDir.UP);
				TopThruster.Deactivate();
			}
			else if (inputData.thrustInput == new Vector2(-1, -1))
			{
                LeftThruster.Activate(Thruster.ThrusterDir.UP);
                RightThruster.Activate(Thruster.ThrusterDir.RIGHT);
                TopThruster.Deactivate();
            }
            else if (inputData.thrustInput == new Vector2(1, 1))
            {
                LeftThruster.Activate(Thruster.ThrusterDir.LEFT);
                RightThruster.Activate(Thruster.ThrusterDir.DOWN);
                TopThruster.Deactivate();
            }
            else if (inputData.thrustInput == new Vector2(1, -1))
            {
                LeftThruster.Activate(Thruster.ThrusterDir.LEFT);
                RightThruster.Activate(Thruster.ThrusterDir.UP);
                TopThruster.Deactivate();
            }
            else if (inputData.thrustInput == new Vector2(-1, 1))
            {
                LeftThruster.Activate(Thruster.ThrusterDir.DOWN);
                RightThruster.Activate(Thruster.ThrusterDir.RIGHT);
                TopThruster.Deactivate();
            }
        }
		else
		{
			DeactivateAllThrusters();
		}
	}

	private void DeactivateAllThrusters()
	{
        foreach (Thruster thruster in thrusters)
        {
            thruster.Deactivate();
        }
    }
    #endregion
}
