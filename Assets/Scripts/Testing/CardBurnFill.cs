using UnityEngine;
using UnityEngine.UI;

public class CardBurnFill : MonoBehaviour
{
    public float burnDuration = 2f;

    Image image;
    float timer;

    void Awake()
    {
        image = GetComponent<Image>();
        image.fillAmount = 1f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / burnDuration);
        image.fillAmount = Mathf.Lerp(1f, 0f, t);
    }
}
