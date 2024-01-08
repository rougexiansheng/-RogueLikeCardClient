using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIColorCostItem : MonoBehaviour
{
    [SerializeField]
    TMP_Text text;
    [SerializeField]
    ParticleItem gainParticle, depletionParticle, usageParticle, selectParticle;
    [SerializeField]
    Image img;
    public void SetValue(int value)
    {
        img.color = value <= 0 ? Color.gray : Color.white;
        text.text = value.ToString();
    }

    public void Select(bool isSelect)
    {
        if (isSelect) selectParticle.Play();
        else selectParticle.Stop();
    }

    public void ActiveParticle(PModifyColorData.PerformanceColorEffectEnum effectEnum)
    {
        switch (effectEnum)
        {
            case PModifyColorData.PerformanceColorEffectEnum.Gain:
                gainParticle.Play();
                break;
            case PModifyColorData.PerformanceColorEffectEnum.Depletion:
                depletionParticle.Play();
                break;
            case PModifyColorData.PerformanceColorEffectEnum.Usage:
                usageParticle.Play();
                break;
            default:
                break;
        }
    }
}
