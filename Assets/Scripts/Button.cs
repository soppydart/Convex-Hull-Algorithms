using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Texture2D hoverCursor;

    // Call this method when the mouse pointer enters the button
    public void OnPointerEnter(PointerEventData eventData)
    {
        Cursor.SetCursor(hoverCursor, Vector2.zero, CursorMode.Auto);
    }

    // Call this method when the mouse pointer exits the button
    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}