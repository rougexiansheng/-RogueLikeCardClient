using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ManaItem : MonoBehaviour
{
    [SerializeField]
    private TMP_Text numText;


    public void SetText(string text)
    {
        numText.text = text;
    }
}
