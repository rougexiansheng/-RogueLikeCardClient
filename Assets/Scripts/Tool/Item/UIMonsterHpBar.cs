using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System;

public class UIMonsterHpBar : MonoBehaviour
{
    [SerializeField]
    Image hpBarImg;
    [SerializeField]
    CanvasGroup canvasGroup;
    [SerializeField]
    TMPro.TMP_Text hpText, shieldText;
    [SerializeField]
    Image shieldImg, edgeImg, skillImg;
    [SerializeField]
    Sprite normalMonsterEdge, eliteMonsterEdge, normalShield, eliteShield;
    [SerializeField]
    ParticleItem normalShieldGainParticle, normalShieldBreakParticle, eliteShieldGainParticle, eliteShieldBreakParticle;
    ParticleItem shieldGainParticle, shieldBreakParticle;
    float time = 0.5f;
    public List<UIPassiveIconItem> passiveIconItems;
    private void Awake()
    {
        Active(false);
    }

    public void SetEdge(bool isElite)
    {
        if (isElite)
        {
            shieldGainParticle = eliteShieldGainParticle;
            shieldBreakParticle = eliteShieldBreakParticle;
            shieldImg.sprite = eliteShield;
            edgeImg.sprite = eliteMonsterEdge;
        }
        else
        {
            shieldGainParticle = normalShieldGainParticle;
            shieldBreakParticle = normalShieldBreakParticle;
            shieldImg.sprite = normalShield;
            edgeImg.sprite = normalMonsterEdge;
        }
    }

    public void UpdateMonsterShield(PModifyShieldData data)
    {
        var t = 0.5f;
        //數值沒有異動 什麼都不做
        shieldText.text = data.shieldValue.ToString();
        if (data.beforeValue == data.shieldValue) return;
        // 特效數值增加表演
        if (data.shieldValue > 0)
        {
            //初次獲得護盾
            if (data.beforeValue == 0)
            {
                shieldImg.DOFade(1, t);
                shieldText.DOFade(1, t);
            }
            shieldGainParticle.Play();
        }

        // 數值修改就放大縮小
        shieldText.transform.localScale = Vector3.one * 1.5f;
        shieldText.transform.DOScale(1, t);
        // 減少 紅變白
        if (data.beforeValue > data.shieldValue)
        {
            shieldText.color = Color.red;
            // 減少變紅轉白
            var color = Color.white;
            // 結果為0 淡出
            if (data.shieldValue == 0)
            {
                color.a = 0;
                shieldImg.DOFade(0, t);
                shieldBreakParticle.Play();
            }
            shieldText.DOColor(color, t);
        }
    }

    public UniTask OnAttack()
    {
        skillImg.DOFade(0, time);
        return skillImg.transform.DOScale(Vector3.one * 3f, time).AsyncWaitForCompletion().AsUniTask();
    }

    public UniTask SetSkillIcon(Sprite icon)
    {
        skillImg.sprite = icon;
        skillImg.color = new Color(1, 1, 1, 0);
        skillImg.DOFade(1, time);
        skillImg.transform.localScale = Vector3.one * 3f;
        skillImg.transform.DOScale(1, time);
        return UniTask.Delay((int)(time * 1000) + 100);
    }

    public void SetHp(int current, int max)
    {
        hpText.text = $"{current} / {max}";
        DOTween.To(() => hpBarImg.fillAmount, x => hpBarImg.fillAmount = x, current / (float)max, 0.2f);
    }

    public void Active(bool torf)
    {
        canvasGroup.alpha = torf ? 1 : 0;
        canvasGroup.blocksRaycasts = torf;
        canvasGroup.interactable = torf;
    }

    public void Clear()
    {
        SetHp(1, 1);
        skillImg.sprite = null;
        skillImg.color = Color.clear;
        for (int i = 0; i < passiveIconItems.Count; i++)
        {
            passiveIconItems[i].Clear();
        }
        shieldText.color = new Color(1, 1, 1, 0);
        shieldImg.color = new Color(1, 1, 1, 0);
    }
}
