using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSetItem : MonoBehaviour
{

    [SerializeField]
    public TMP_Text numText;

    [SerializeField]
    public Button button;

    [SerializeField]
    public Image SelectImage;
    public void Rest()
    {
        SelectImage.gameObject.SetActive(false);
        button.interactable = false;
        button.onClick.RemoveAllListeners();
        numText.text = "";
    }


}
