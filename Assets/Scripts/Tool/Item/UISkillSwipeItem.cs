using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UISkillSwipeItem : MonoBehaviour
{
    [SerializeField]
    public ButtonLongPress ButtonLongPress;

    public Image icon;
    public int skillId;
    public void Init(int index, Sprite sprite)
    {
        this.skillId = index;
        this.icon.sprite = sprite;
    }
}
