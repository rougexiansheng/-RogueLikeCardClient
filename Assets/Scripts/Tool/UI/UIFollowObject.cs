using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFollowObject : MonoBehaviour
{
    Camera uiCamera;
    Transform target;

    public void SetFollowTarget(Transform target)
    {
        if (uiCamera == null) uiCamera = Camera.main;
        this.target = target;
    }

    private void Update()
    {
        if (target == null) return;
        if (uiCamera == null) uiCamera = Camera.main;
        transform.position = uiCamera.WorldToScreenPoint(target.position);
    }
}
