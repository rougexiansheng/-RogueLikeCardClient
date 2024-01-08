using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PModifyColorData : PerformanceData
{
    public Dictionary<SkillCostColorEnum, int> costColorCount;
    public List<ColorEffectData> effectDatas = new List<ColorEffectData>();
    /// <summary>特效</summary>
    public enum PerformanceColorEffectEnum
    {
        /// <summary>獲得時</summary>
        Gain,
        /// <summary>減少時</summary>
        Depletion,
        /// <summary>消耗</summary>
        Usage,
    }
    public class ColorEffectData
    {
        public PerformanceColorEffectEnum effectEnum;
        public SkillCostColorEnum color;
    }
    public void Init(BattleActor actor)
    {
        costColorCount = new Dictionary<SkillCostColorEnum, int>(actor.colors);
    }

    public void SetColorEffectEnum(SkillCostColorEnum colorEnum, PerformanceColorEffectEnum effectEnum)
    {
        effectDatas.Add(new ColorEffectData() { effectEnum = effectEnum, color = colorEnum });
    }
    public void SetColorEffectEnum(SkillCostColorEnum colorEnum, int value)
    {
        if (value == 0) return;
        var effectEnum = value > 0 ? PerformanceColorEffectEnum.Gain : PerformanceColorEffectEnum.Depletion;
        effectDatas.Add(new ColorEffectData() { effectEnum = effectEnum, color = colorEnum });
    }

}
