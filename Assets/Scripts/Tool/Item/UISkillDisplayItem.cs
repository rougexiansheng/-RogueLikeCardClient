using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkillDisplayItem : MonoBehaviour
{

    public Image icon;
    public int skillId;
    public void Init(int index, Sprite sprite)
    {
        this.skillId = index;
        this.icon.sprite = sprite;
    }

}
