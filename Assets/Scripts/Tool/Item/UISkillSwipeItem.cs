using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UISkillSwipeItem : MonoBehaviour
{
    [SerializeField]
    public ButtonLongPress ButtonLongPress;

    public Image Icon;
    public int SkillId;
    public int Index;
    public bool IsCenter = true;
    private float orignalYPosition;
    public void Init(int index, Sprite sprite)
    {
        this.Index = index;
        this.Icon.sprite = sprite;
        orignalYPosition = Icon.transform.position.y;
    }

    public void UpPerformace()
    {
        Icon.transform.DOLocalMoveY(100, 0.2f);
        IsCenter = false;
    }
    public void DownPerformace()
    {
        Icon.transform.DOLocalMoveY(-100, 0.2f);
        IsCenter = false;
    }
    public void ResetPerformace()
    {
        if (IsCenter == true) return;
        Icon.transform.DOLocalMoveY(0, 0.01f);
        IsCenter = true;
    }
}
