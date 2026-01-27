using UnityEngine;
using UnityEngine.UI;

public class BurnEdgeFollower : MonoBehaviour
{
    public Image cardImage;
    public RectTransform cardRect;
    public ParticleSystem fireParticles;

    void Update()
    {
        float fill = cardImage.fillAmount;

        float y = Mathf.Lerp(
            cardRect.rect.height * 0.5f,
            -cardRect.rect.height * 0.5f,
            1f - fill
        );

        transform.localPosition = new Vector3(0f, y, 0f);
    }
}
