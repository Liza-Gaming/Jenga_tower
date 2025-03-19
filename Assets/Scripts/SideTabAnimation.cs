using UnityEngine;
using System.Collections;

public class SideTabAnimation : MonoBehaviour
{
    // Reference to the panel's RectTransform.
    public RectTransform panelRectTransform;
    
    // Duration for the sliding animation.
    public float slideTime = 0.5f;
    
    // The anchored X positions for the open and closed states.
    public float openPositionX = 0f;
    public float closedPositionX = -300f;
    
    // Track whether the panel is currently open.
    private bool isOpen = false;
    private Coroutine currentCoroutine;

    // Call this method on button click.
    public void TogglePanel()
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(SlidePanel());
    }

    private IEnumerator SlidePanel()
    {
        float elapsed = 0f;
        float startX = panelRectTransform.anchoredPosition.x;
        // Determine target position based on current state.
        float targetX = isOpen ? closedPositionX : openPositionX;

        while (elapsed < slideTime)
        {
            elapsed += Time.deltaTime;
            float newX = Mathf.Lerp(startX, targetX, elapsed / slideTime);
            panelRectTransform.anchoredPosition = new Vector2(newX, panelRectTransform.anchoredPosition.y);
            yield return null;
        }
        // Ensure panel reaches exact target.
        panelRectTransform.anchoredPosition = new Vector2(targetX, panelRectTransform.anchoredPosition.y);
        isOpen = !isOpen;
    }
}
