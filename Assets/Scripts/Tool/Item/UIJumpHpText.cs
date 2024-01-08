using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class UIJumpHpText : MonoBehaviour
{
    string priteFormat = "<sprite name={0} color={1}>";
    [SerializeField]
    TMPro.TMP_Text text;
    [Header("¶Ë®`")]
    [SerializeField]
    float radis;
    [Header("ªvÀø")]
    [SerializeField]
    float moveUp;
    async UniTask DoDamageJump()
    {
        var r = Random.Range(0, 2);
        var y = Random.Range(0.01f, 1);
        var x = r == 0 ? Random.Range(0.01f, 1) : Random.Range(-0.01f, -1);
        var moveV3 = new Vector3(x, y);
        moveV3.Normalize();
        moveV3 *= this.radis;
        var pos = ((RectTransform)transform).anchoredPosition3D + moveV3;
        ((RectTransform)transform).DOAnchorPos3DY(pos.y, 0.6f).SetEase(Ease.OutSine);
        await ((RectTransform)transform).DOAnchorPos3DX(pos.x, 0.6f).SetEase(Ease.OutSine).AsyncWaitForCompletion().AsUniTask();
        await text.DOFade(0, 0.2f).SetEase(Ease.InExpo).AsyncWaitForCompletion().AsUniTask();
    }
    UniTask DoHealJump()
    {
        var tY = ((RectTransform)transform).DOAnchorPosY(moveUp, 1f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion().AsUniTask();
        var tA = text.DOFade(0, 1.2f).SetEase(Ease.InExpo).AsyncWaitForCompletion().AsUniTask();
        return UniTask.WhenAll(tY, tA);
    }
    public UniTask JumpBlock()
    {
        var colorHex = "#FFFFFF";
        text.color = Color.white;
        text.text = "";
        text.text = string.Format(priteFormat, "\"block\"", colorHex);
        return DoDamageJump();
    }
    public UniTask Jump(int value, bool isNegative)
    {
        var str = Mathf.Abs(value).ToString();
        text.color = Color.white;
        text.text = "";
        if (isNegative)
        {
            var colorHex = "#FFFFFF";
            text.text += string.Format(priteFormat, $"\"-\"", colorHex);
            for (int i = 0; i < str.Length; i++)
            {
                text.text += string.Format(priteFormat,$"\"{str[i]}\"", colorHex);
            }
            return DoDamageJump();
        }
        else
        {
            var colorHex = "#00FF00";
            text.text += string.Format(priteFormat, $"\"+\"", colorHex);
            for (int i = 0; i < str.Length; i++)
            {
                text.text += string.Format(priteFormat, $"\"{str[i]}\"" , colorHex);
            }
            return DoHealJump();
        }
    }
}
