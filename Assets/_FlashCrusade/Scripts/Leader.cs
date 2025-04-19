using UnityEngine;

public class Leader : MonoBehaviour
{
	private Fleet fleet;

	private bool freeFly = false;

	private void Start()
	{
		fleet = new Fleet(this.transform);
	}

	#region COMMANDS

	public void ToggleFreeFly()
	{
		freeFly = !freeFly;

		foreach(AIAgent ship in fleet.ships)
		{
			ship.FreeFly = freeFly;
		}
	}

	#endregion
}
