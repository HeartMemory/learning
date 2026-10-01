using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float value = 0f;      // 左按钮填 -1，右按钮填 +1
    public bool IsHeld { get; private set; }

    public void OnPointerDown(PointerEventData e) => IsHeld = true;
    public void OnPointerUp(PointerEventData e) => IsHeld = false;
}
