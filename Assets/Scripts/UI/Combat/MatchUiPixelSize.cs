using UnityEngine;

public sealed class MatchUiPixelSize : MonoBehaviour
{
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private Camera targetCamera;
	[SerializeField] private Vector2 desiredUiPixels = new Vector2(512, 512); // previous UI size
	[SerializeField] private bool matchHeightOnly = true;

	private void Reset()
	{
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		if (targetCamera == null) targetCamera = Camera.main;
	}

	private void Start()
	{
		if (spriteRenderer == null || targetCamera == null) return;

		var sprite = spriteRenderer.sprite;
		if (sprite == null) return;

		float worldPerPixel = (2f * targetCamera.orthographicSize) / Screen.height;

		// Sprite world size at scale 1
		Vector2 spriteWorldAt1 = sprite.rect.size / sprite.pixelsPerUnit;

		// Desired world size derived from UI pixels
		Vector2 desiredWorld = desiredUiPixels * worldPerPixel;

		Vector3 newScale;
		if (matchHeightOnly)
		{
			float s = desiredWorld.y / spriteWorldAt1.y;
			newScale = new Vector3(s, s, 1f);
		}
		else
		{
			newScale = new Vector3(
				desiredWorld.x / spriteWorldAt1.x,
				desiredWorld.y / spriteWorldAt1.y,
				1f
			);
		}

		transform.localScale = newScale;
	}
}