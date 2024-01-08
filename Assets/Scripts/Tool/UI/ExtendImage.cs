using Crystal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExtendImage : Image
{
    [HideInInspector]
    public bool extendTop;

    [HideInInspector]
    public bool extendBottom;

    private SafeArea m_safeArea;

    protected override void Start()
    {
        base.Start();

        Refresh();
    }

    public void SetSafeArea(SafeArea safeArea)
    {
        m_safeArea = safeArea;
    }

    public void Refresh()
    {
        if (m_safeArea == null) return;

        if (extendTop)
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, m_safeArea.GetTopExtendArea());

        if (extendBottom)
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, -m_safeArea.GetBottomExtendArea());
    }
}
