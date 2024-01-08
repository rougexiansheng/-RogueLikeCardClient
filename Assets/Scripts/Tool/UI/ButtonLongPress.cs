using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonLongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public float durationThreshold = 1.0f;

    /// <summary>長按觸發一次</summary>
    public UnityEvent onLongPress = new UnityEvent();
    /// <summary>按下 觸發一次</summary>
    public UnityEvent onPressed = new UnityEvent();
    /// <summary>離開 觸發一次</summary>
    public UnityEvent onPressEnd = new UnityEvent();

    private bool isPointerDown = false;
    private bool longPressTriggered = false;
    /// <summary>是否以觸發長按</summary>
    public bool isTriggered { get { return longPressTriggered && isPointerDown; } }
    private float timePressStarted;


    private void Update()
    {
        if (isPointerDown && !longPressTriggered)
        {
            if (Time.time - timePressStarted > durationThreshold)
            {
                longPressTriggered = true;
                onLongPress.Invoke();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        timePressStarted = Time.time;
        isPointerDown = true;
        longPressTriggered = false;
        onPressed.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        onPressEnd.Invoke();
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerDown = false;
        onPressEnd.Invoke();
    }
}
