using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStateItem : MonoBehaviour
{
    [SerializeField]
    GameObject roundCountObject;
    [SerializeField]
    Image passiveIcon;
    [SerializeField]
    TMPro.TMP_Text stackCountText, rountCountText, passiveName, comment;
    public void SetState(ActorPassive passive, PassiveDataDefine define)
    {
        var rounds = define.keepCount - passive.keepCount;
        roundCountObject.SetActive(rounds > 0);
        if (rounds > 0)
        {
            rountCountText.text = $"{rounds} ¦^¦X";
        }
        passiveIcon.sprite = define.icon;
        stackCountText.text = passive.currentStack.ToString();
        passiveName.text = define.passiveName;
        comment.text = define.comment;
    }
    public void Clear()
    {
        roundCountObject.SetActive(false);
        passiveIcon.sprite = null;
        comment.text = "";
        passiveName.text = "";
    }
}
