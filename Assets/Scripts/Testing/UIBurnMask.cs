using UnityEngine;

public class UIBurnMask : MonoBehaviour
{
    public float burnDuration = 2f;

    RectTransform rect;
    float startHeight;
    float timer;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        startHeight = rect.rect.height;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / burnDuration);
        rect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            Mathf.Lerp(startHeight, 0f, t)
        );
    }
}
