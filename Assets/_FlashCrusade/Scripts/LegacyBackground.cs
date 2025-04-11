using UnityEngine;

public class LegacyBackground : MonoBehaviour
{
	private Material backgroundMaterial;
	private Vector2 offset;

	public float parallax = 2f;

	private void Awake()
	{
		backgroundMaterial = GetComponent<MeshRenderer>().material;
	}

	void FixedUpdate()
	{
		offset = backgroundMaterial.mainTextureOffset;

		offset.x = transform.position.x / transform.localScale.x / parallax;
		offset.y = transform.position.y / transform.localScale.y / parallax;

		backgroundMaterial.mainTextureOffset = offset;
	}
}
