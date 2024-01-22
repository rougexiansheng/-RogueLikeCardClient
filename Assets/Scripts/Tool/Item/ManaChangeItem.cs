using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ManaChangeItem : MonoBehaviour
{
    [SerializeField]
    private TMP_Text numText;

    [SerializeField]
    private Image state;

    [SerializeField]
    private Image upImage;
    [SerializeField]
    private Image downUpImage;


    public void SetText(int num)
    {
        numText.text = "";
        upImage.gameObject.SetActive(false);
        downUpImage.gameObject.SetActive(false);

        if (num > 0)
        {
            numText.text = "+";
            upImage.gameObject.SetActive(true);
        }
        else if (num < 0)
        {
            downUpImage.gameObject.SetActive(true);
        }
        numText.text += num.ToString();
    }

}
