using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMiniMonsterHpInfo : MonoBehaviour
{
    [SerializeField]
    Image deadMark, hp;

    public void SetHp(float f)
    {
        hp.fillAmount = f;
    }

    public void SetDeadMark(bool torf)
    {
        deadMark.enabled = torf;
    }
}
