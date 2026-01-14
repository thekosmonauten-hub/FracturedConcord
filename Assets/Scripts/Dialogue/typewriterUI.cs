using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class typewriterUI : MonoBehaviour
{
	Text _text;
	TMP_Text _tmpProText;
	string writer;

	[Header("Typewriter Settings")]
	[Tooltip("Delay before typewriter starts (in seconds)")]
	[SerializeField] float delayBeforeStart = 0f;
	
	[Tooltip("Time between each character (in seconds). Lower = faster. Try 0.02 for fast, 0.05 for normal, 0.1 for slow.")]
	[SerializeField] float timeBtwChars = 0.05f;
	
	[Tooltip("Character shown at the end while typing (e.g., '|' for cursor). Leave empty for none.")]
	[SerializeField] string leadingChar = "";
	
	[Tooltip("Show leading character before delay starts?")]
	[SerializeField] bool leadingCharBeforeDelay = false;

	// Coroutine references for stopping
	private Coroutine textCoroutine;
	private Coroutine tmpCoroutine;
	
	// Track if Start() has been called
	private bool hasStarted = false;
	
	// Track if typewriter was manually started (prevents OnEnable from interfering)
	private bool manuallyStarted = false;

	// Use this for initialization
	void Start()
	{
		InitializeComponents();
		StartTypewriterEffect();
		hasStarted = true;
	}
	
	/// <summary>
	/// Initialize component references
	/// </summary>
	private void InitializeComponents()
	{
		if (_text == null)
			_text = GetComponent<Text>();
		if (_tmpProText == null)
			_tmpProText = GetComponent<TMP_Text>();
	}
	
	/// <summary>
	/// Start the typewriter effect with the current text in the component.
	/// This can be called multiple times to retrigger the effect when text changes.
	/// </summary>
	public void StartTypewriterEffect()
	{
		// Stop any existing coroutines first
		StopTypewriterEffect();
		
		InitializeComponents();

		if(_text != null && !string.IsNullOrEmpty(_text.text))
        {
			writer = _text.text;
			_text.text = "";

			// Use method reference instead of string (more reliable)
			textCoroutine = StartCoroutine(TypeWriterText());
		}

		if (_tmpProText != null && !string.IsNullOrEmpty(_tmpProText.text))
		{
			writer = _tmpProText.text;
			_tmpProText.text = "";

			// Use method reference instead of string (more reliable)
			tmpCoroutine = StartCoroutine(TypeWriterTMP());
		}
	}
	
	/// <summary>
	/// Start the typewriter effect with the provided text directly.
	/// This avoids the need to set text in the component first, preventing it from being visible before typing starts.
	/// </summary>
	public void StartTypewriterEffect(string textToType)
	{
		// Stop any existing coroutines first
		StopTypewriterEffect();
		
		InitializeComponents();
		
		if (string.IsNullOrEmpty(textToType))
		{
			Debug.LogWarning("[typewriterUI] StartTypewriterEffect: textToType is null or empty!");
			return;
		}
		
		writer = textToType;
		
		// Mark as manually started to prevent OnEnable from interfering
		manuallyStarted = true;
		
		// Clear text immediately to prevent it from being visible
		bool startedCoroutine = false;
		
		if (_text != null)
		{
			_text.text = "";
			// Only start coroutine if GameObject is active (coroutines need active GameObject)
			if (gameObject.activeInHierarchy)
			{
				textCoroutine = StartCoroutine(TypeWriterText());
				startedCoroutine = true;
				Debug.Log($"[typewriterUI] Started typewriter coroutine for Text component with {textToType.Length} characters");
			}
			else
			{
				// GameObject not active yet - coroutine will start when it becomes active
				// But we need to ensure text stays empty
				_text.text = "";
				Debug.Log($"[typewriterUI] GameObject not active yet - will start typewriter when activated. Text cleared.");
			}
		}
		
		if (_tmpProText != null)
		{
			_tmpProText.text = "";
			// Only start coroutine if GameObject is active (coroutines need active GameObject)
			if (gameObject.activeInHierarchy)
			{
				tmpCoroutine = StartCoroutine(TypeWriterTMP());
				startedCoroutine = true;
				Debug.Log($"[typewriterUI] Started typewriter coroutine for TextMeshPro component with {textToType.Length} characters");
			}
			else
			{
				// GameObject not active yet - coroutine will start when it becomes active
				// But we need to ensure text stays empty
				_tmpProText.text = "";
				Debug.Log($"[typewriterUI] GameObject not active yet - will start typewriter when activated. Text cleared.");
			}
		}
		
		// If GameObject wasn't active, start coroutine in OnEnable
		// But we've already set manuallyStarted = true, so OnEnable won't interfere
		if (!startedCoroutine)
		{
			if (!gameObject.activeInHierarchy)
			{
				// GameObject will become active soon - OnEnable will handle it, but we've marked it as manually started
				// So we need to start the coroutine when it becomes active
				// We'll do this by checking in OnEnable if manuallyStarted and starting coroutine
				Debug.Log($"[typewriterUI] GameObject not active - typewriter will start when GameObject becomes active");
			}
			else
			{
				Debug.LogError("[typewriterUI] StartTypewriterEffect: Neither Text nor TextMeshPro component found! Cannot start typewriter effect.");
			}
		}
	}
	
	/// <summary>
	/// Stop any currently running typewriter effects
	/// </summary>
	public void StopTypewriterEffect()
	{
		if (textCoroutine != null)
		{
			StopCoroutine(textCoroutine);
			textCoroutine = null;
		}
		if (tmpCoroutine != null)
		{
			StopCoroutine(tmpCoroutine);
			tmpCoroutine = null;
		}
	}
	
	/// <summary>
	/// Called when the GameObject is enabled - retrigger typewriter if text has changed
	/// This ensures typewriter effect works when paragraphs are reused
	/// </summary>
	void OnEnable()
	{
		// If typewriter was manually started, start the coroutine now that GameObject is active
		if (manuallyStarted)
		{
			manuallyStarted = false; // Reset flag
			
			// Ensure text is still empty
			if (_text != null)
			{
				_text.text = "";
			}
			if (_tmpProText != null)
			{
				_tmpProText.text = "";
			}
			
			// Now start the coroutine since GameObject is active
			if (!string.IsNullOrEmpty(writer))
			{
				if (_text != null && textCoroutine == null)
				{
					textCoroutine = StartCoroutine(TypeWriterText());
					Debug.Log($"[typewriterUI] OnEnable: Started typewriter coroutine for Text component (was waiting for GameObject activation)");
				}
				if (_tmpProText != null && tmpCoroutine == null)
				{
					tmpCoroutine = StartCoroutine(TypeWriterTMP());
					Debug.Log($"[typewriterUI] OnEnable: Started typewriter coroutine for TextMeshPro component (was waiting for GameObject activation)");
				}
			}
			return;
		}
		
		// If Start() has already run and the component is active, check if we should retrigger
		// Wait a frame to ensure text is set before checking (text might be set before activation)
		if (hasStarted && gameObject.activeInHierarchy)
		{
			StartCoroutine(CheckAndRetriggerOnEnable());
		}
	}
	
	/// <summary>
	/// Check if text has changed and retrigger typewriter if needed
	/// </summary>
	private IEnumerator CheckAndRetriggerOnEnable()
	{
		// Wait a frame to ensure any text setting has completed
		yield return null;
		
		// Don't retrigger if we're already typing (avoid double-triggering)
		if (textCoroutine != null || tmpCoroutine != null)
		{
			yield break;
		}
		
		InitializeComponents();
		
		// Check if there's text to type that hasn't been typed yet
		bool shouldRetrigger = false;
		
		if (_tmpProText != null && !string.IsNullOrEmpty(_tmpProText.text))
		{
			// If text exists, it means it was set but typewriter hasn't started
			shouldRetrigger = true;
		}
		else if (_text != null && !string.IsNullOrEmpty(_text.text))
		{
			shouldRetrigger = true;
		}
		
		if (shouldRetrigger)
		{
			StartTypewriterEffect();
		}
	}

	/// <summary>
	/// Set the speed of the typewriter effect at runtime.
	/// Lower values = faster typing. Recommended: 0.01 (very fast) to 0.1 (slow).
	/// </summary>
	public void SetTypewriterSpeed(float newSpeed)
	{
		// Ensure speed is positive and reasonable
		timeBtwChars = Mathf.Max(0.001f, newSpeed);
		
		// If coroutine is running, we'd need to restart it to apply new speed
		// For now, this will apply to future typewriter effects
	}

	/// <summary>
	/// Get the current typewriter speed.
	/// </summary>
	public float GetTypewriterSpeed()
	{
		return timeBtwChars;
	}

	IEnumerator TypeWriterText()
	{
		_text.text = leadingCharBeforeDelay ? leadingChar : "";

		yield return new WaitForSeconds(delayBeforeStart);

		foreach (char c in writer)
		{
			if (_text == null) yield break; // Safety check
			
			if (_text.text.Length > 0)
			{
				_text.text = _text.text.Substring(0, _text.text.Length - leadingChar.Length);
			}
			_text.text += c;
			_text.text += leadingChar;
			yield return new WaitForSeconds(timeBtwChars);
		}

		if(leadingChar != "")
        {
			_text.text = _text.text.Substring(0, _text.text.Length - leadingChar.Length);
		}
	}

	IEnumerator TypeWriterTMP()
    {
        _tmpProText.text = leadingCharBeforeDelay ? leadingChar : "";

        yield return new WaitForSeconds(delayBeforeStart);

		foreach (char c in writer)
		{
			if (_tmpProText == null) yield break; // Safety check
			
			if (_tmpProText.text.Length > 0)
			{
				_tmpProText.text = _tmpProText.text.Substring(0, _tmpProText.text.Length - leadingChar.Length);
			}
			_tmpProText.text += c;
			_tmpProText.text += leadingChar;
			yield return new WaitForSeconds(timeBtwChars);
		}

		if (leadingChar != "")
		{
			_tmpProText.text = _tmpProText.text.Substring(0, _tmpProText.text.Length - leadingChar.Length);
		}
	}

	void OnDestroy()
	{
		// Clean up coroutines when object is destroyed
		StopTypewriterEffect();
	}
	
	void OnDisable()
	{
		// Stop typewriter when GameObject is disabled
		StopTypewriterEffect();
	}
}