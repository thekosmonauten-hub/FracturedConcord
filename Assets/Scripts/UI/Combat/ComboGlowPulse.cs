using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple pulsing effect for the ComboGlow image.
/// Attach to the ComboGlow GameObject. When enabled, it will pulse alpha between min and max.
/// Disabling the GameObject (or component) stops the tween and restores the max alpha.
/// </summary>
[RequireComponent(typeof(Image))]
public class ComboGlowPulse : MonoBehaviour
{
	[Header("Pulse Settings")]
	[SerializeField] private float minAlpha = 0.15f;
	[SerializeField] private float maxAlpha = 0.35f;
	[SerializeField] private float pulseDuration = 0.8f; // total cycle time up and down ~1.6s
	[SerializeField] private LeanTweenType easeType = LeanTweenType.easeInOutSine;
	
	private Image glowImage;
	private Color baseColor;
	private bool initialized = false;
	
	private void Awake()
	{
		glowImage = GetComponent<Image>();
		if (glowImage != null)
		{
			baseColor = glowImage.color;
			initialized = true;
		}
	}
	
	private void OnEnable()
	{
		if (!initialized)
		{
			Awake();
		}
		if (glowImage == null) return;
		
		// Ensure starting at max alpha (visible)
		SetAlpha(maxAlpha);
		
		// Start ping-pong alpha tween using value tween for precise alpha control
		// We'll use half-duration for one leg so full cycle ~pulseDuration
		LeanTween.value(gameObject, maxAlpha, minAlpha, pulseDuration * 0.5f)
			.setEase(easeType)
			.setOnUpdate((float a) => SetAlpha(a))
			.setLoopPingPong();
	}
	
	private void OnDisable()
	{
		// Cancel all tweens on this object and restore alpha
		LeanTween.cancel(gameObject);
		if (glowImage != null)
		{
			SetAlpha(maxAlpha);
		}
	}
	
	private void SetAlpha(float a)
	{
		var c = glowImage.color;
		c.a = a;
		glowImage.color = c;
	}
}


