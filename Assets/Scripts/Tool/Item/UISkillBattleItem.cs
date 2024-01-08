using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Cysharp.Threading.Tasks;
using System;

public class UISkillBattleItem : MonoBehaviour
{
    [SerializeField]
    GameObject[] levelObjs;
    /// <summary>
    /// 灰 綠 藍 紅
    /// </summary>
    [SerializeField]
    Sprite[] ColorSprites;

    /// <summary>
    /// 特效物件
    /// </summary>
    [SerializeField]
    ParticleItem sealedEffect, sealingEffect, maliciousEffect;
    /// <summary>
    /// 特效物件
    /// </summary>
    [SerializeField]
    ParticleItem castEffect, readyCastEffect, addFailureEffect, removeEffect;
    [SerializeField]
    Image[] images;
    [SerializeField]
    Image skillImg;
    [SerializeField]
    Transform[] costPositions;
    public CanvasGroup canvasGroup;
    public Button button;
    public ButtonLongPress longPressBtn;
    public int skillId;
    public int skillIndex;
    bool isReady, isUsed, isBanned, isMalicious;
    public void ReadyCost(bool isReady)
    {
        this.isReady = isReady;
        if (isReady)
            readyCastEffect.Play();
        else
            readyCastEffect.Stop();
        canvasGroup.interactable = !isUsed && this.isReady;
    }

    public void Used(bool isUsed)
    {
        this.isUsed = isUsed;
        skillImg.color = isUsed ? Color.gray : Color.white;
        if (isUsed)
            readyCastEffect.Stop();
        canvasGroup.interactable = !this.isUsed && isReady;
    }

    public void PlayAddFailure(Action action)
    {
        addFailureEffect.ClearAction();
        addFailureEffect.AddStopAction(action);
        addFailureEffect.Play();
    }

    public void PlayRemoveEffect(Action action)
    {
        removeEffect.ClearAction();
        removeEffect.AddStopAction(action);
        removeEffect.Play();
    }

    public void StartSeal()
    {
        isBanned = true;
        sealingEffect.ClearAction();
        sealingEffect.AddStopAction(sealedEffect.Play);
        sealingEffect.Play();
    }

    public void EndSeal()
    {
        isBanned = false;
        sealedEffect.Stop();
    }

    public void DoSealed(bool torf)
    {
        if (torf)
        {
            sealedEffect.Play();
        }
        else
        {
            sealedEffect.Stop();
        }
    }

    private void Clear()
    {
        longPressBtn.onPressed.RemoveAllListeners();
        longPressBtn.onLongPress.RemoveAllListeners();
        longPressBtn.onPressEnd.RemoveAllListeners();
        button.onClick.RemoveAllListeners();
        readyCastEffect.Stop();
        sealedEffect.Stop();
        for (int i = 0; i < levelObjs.Length; i++)
        {
            levelObjs[i].SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (isMalicious) maliciousEffect.Play();
        else maliciousEffect.Stop();

        if (isBanned) sealedEffect.Play();
        else sealedEffect.Stop();
    }

    public void SetSkillItem(SkillDataDefine skillData)
    {
        Clear();
        longPressBtn.onPressed.RemoveAllListeners();
        longPressBtn.onPressed.AsObservable().Subscribe(_ => castEffect.Play());
        longPressBtn.onPressEnd.RemoveAllListeners();
        longPressBtn.onPressEnd.AsObservable().Subscribe(_ => castEffect.Stop());
        skillImg.sprite = skillData.icon;
        isMalicious = skillData.skillType.HasFlag(SkillTypeEnum.Malicious);
        if (isMalicious) maliciousEffect.Play();
        else maliciousEffect.Stop();
        var startCount = images.Length - skillData.costColors.Count;
        for (int i = 0; i < skillData.level - 1; i++)
        {
            levelObjs[i].SetActive(true);
        }
        for (int i = 0; i < images.Length; i++)
        {
            var img = images[i];
            img.gameObject.SetActive(skillData.costColors.Count > i);
            if (img.gameObject.activeSelf)
            {
                img.transform.position = costPositions[startCount + i * 2].position;
                var sprite = ColorSprites[0];
                switch (skillData.costColors[i].colorEnum)
                {
                    case SkillCostColorEnum.Red:
                        sprite = ColorSprites[3];
                        break;
                    case SkillCostColorEnum.Green:
                        sprite = ColorSprites[1];
                        break;
                    case SkillCostColorEnum.Blue:
                        sprite = ColorSprites[2];
                        break;
                    case SkillCostColorEnum.None:
                    default:
                        break;
                }
                img.sprite = sprite;
                if (skillData.costColors[i].count == -1)
                    img.GetComponentInChildren<TMPro.TMP_Text>().text = "X";
                else
                    img.GetComponentInChildren<TMPro.TMP_Text>().text = skillData.costColors[i].count.ToString();
            }
        }
    }
}
