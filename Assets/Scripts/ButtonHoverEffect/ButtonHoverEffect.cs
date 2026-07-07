using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    private Image image;
    private Color originalColor;

    public float scaleMultiplier = 1.1f;


    [SerializeField]
    private Color hoverColor = new Color(0.0f, 0.8f, 1.0f, 1.0f);

    void Start()
    {
        originalScale = transform.localScale;

        image = GetComponent<Image>();

        if (image != null)
        {
            originalColor = image.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * scaleMultiplier;

        if (image != null)
            image.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;

        if (image != null)
            image.color = originalColor;
    }
}