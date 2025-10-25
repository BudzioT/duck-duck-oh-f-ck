using UnityEngine;
using UnityEngine.UI;

public class FireEmojiMover : MonoBehaviour
{
    private RectTransform rectTransform;
    private RectTransform canvasRect;
    private Vector2 direction;
    private float speed;

    public void Initialize(RectTransform parentCanvas)
    {
        canvasRect = parentCanvas;
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
            Debug.LogError("FireEmojiMover: No RectTransform found on this GameObject!");
        if (canvasRect == null)
            Debug.LogError("FireEmojiMover: No Canvas assigned!");

        direction = Random.insideUnitCircle.normalized;
        float sizeFactor = rectTransform.localScale.x;
        speed = Random.Range(500f, 1000f) / sizeFactor;

        Debug.Log($"FireEmojiMover Initialized. Speed={speed}, Direction={direction}, rectTransform={rectTransform}, canvasRect={canvasRect}");
    }


    void Update()
    {
        if (rectTransform == null || canvasRect == null)
        {
            // Uncomment to debug if Update is running but refs are null
            // Debug.Log("Update called but rectTransform or canvasRect is null");
            return;
        }

        rectTransform.anchoredPosition += direction * speed * Time.deltaTime;

        Vector2 pos = rectTransform.anchoredPosition;
        Vector2 size = canvasRect.sizeDelta;

        float halfWidth = rectTransform.sizeDelta.x / 2;
        float halfHeight = rectTransform.sizeDelta.y / 2;

        bool bounced = false;

        if (pos.x + halfWidth > size.x / 2 || pos.x - halfWidth < -size.x / 2)
        {
            direction.x *= -1;
            pos.x = Mathf.Clamp(pos.x, -size.x / 2 + halfWidth, size.x / 2 - halfWidth);
            bounced = true;
        }

        if (pos.y + halfHeight > size.y / 2 || pos.y - halfHeight < -size.y / 2)
        {
            direction.y *= -1;
            pos.y = Mathf.Clamp(pos.y, -size.y / 2 + halfHeight, size.y / 2 - halfHeight);
            bounced = true;
        }

        rectTransform.anchoredPosition = pos;

        if (bounced)
        {
            Debug.Log($"FireEmoji bounced! New direction={direction}");
        }
    }
}
