using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonImage : Image
{
#pragma warning disable CS0108
    private PolygonCollider2D collider2D;
#pragma warning restore CS0108
    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (collider2D == null) collider2D = GetComponent<PolygonCollider2D>();
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, screenPoint, eventCamera, out Vector3 point);
        return collider2D.OverlapPoint(point);
    }
}
