using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 解析後技能資料
/// </summary>
public class SkillDataDefine
{
    public int id;
    public int groupId;
    public string skillName;
    /// <summary>忽略相機移動</summary>
    public bool ignoreCameraMove;
    public SpineAnimationEnum animationEnum;
    /// <summary>(ignoreCameraMove會被忽略)相機是否抬升</summary>
    public bool isMoveCameraUp;
    public SkillUnlockEnum unlock;
    public int unluckArg;
    public int level;
    public SkillTargetEnum targetEnum;
    public int targetAmount;
    public ActorProfessionEnum profession;
    public string comment;
    public EffectPosEnum costEffectPos;
    public SkillTypeEnum skillType;
    public ParticleItem costEffect;
    public AudioClip costEffectSound;
    public Sprite icon;
    /// <summary>裝備後能獲得的能量球數量</summary>
    public List<SkillCostColorEnum> poolColor = new List<SkillCostColorEnum>();
    /// <summary>此所有技能效果</summary>
    public List<SkillAbilityDataDefine> skillAbilities = new List<SkillAbilityDataDefine>();
    /// <summary>此技能花費</summary>
    public List<SkillCostColorData> costColors = new List<SkillCostColorData>();

}

[Serializable]
public class SkillCostColorData
{
    public SkillCostColorEnum colorEnum;
    public int count;
}

/// <summary>
/// 技能效果定義
/// </summary>
public class SkillAbilityDataDefine
{
    public SkillAbilityConditionEnum abilityCondition;
    public int conditionArg1;
    public int conditionArg2;
    public int conditionArg3;

    public SkillAbilityEnum skillAbility;
    public int abilityArg1;
    public int abilityArg2;
    public int abilityArg3;
    public ParticleItem hitEffect;
    public EffectPosEnum hitEffectPos;
    public AudioClip hitEffectSound;
    public SkillTypeEnum skillType;
}

/// <summary>
/// 最原始技能的資料
/// </summary>
public class SkillOriginDefine : OriginDefineBase<SkillDataDefine>
{
    public int groupId;
    public string skillName;
    public string action;
    public int unlock;
    public int unluckArg;
    public int level;
    public int profession;
    public int red;
    public int blue;
    public int green;
    public bool isMoveCameraUp;
    /// <summary>忽略相機移動</summary>
    public bool ignoreCameraMove;

    public int colorCost1;
    public int colorCost1Arg;
    public int colorCost2;
    public int colorCost2Arg;
    public int colorCost3;
    public int colorCost3Arg;
    public int colorCost4;
    public int colorCost4Arg;

    public int target;
    public int targetAmount;

    public string icon;
    public string comment;
    public string costEffectName;
    public int costEffectPos;
    public string castEffectSound;

    public int ability1Condition;
    public int ability1ConditionArg1;
    public int ability1ConditionArg2;
    public int ability1ConditionArg3;
    public int ability1Type;
    public int skillAbility1;
    public int ability1Arg1;
    public int ability1Arg2;
    public int ability1Arg3;
    public string ability1HitEffect;
    public int ability1HitEffectPos;
    public string ability1HitEffectSound;

    public int ability2Condition;
    public int ability2ConditionArg1;
    public int ability2ConditionArg2;
    public int ability2ConditionArg3;
    public int ability2Type;
    public int skillAbility2;
    public int ability2Arg1;
    public int ability2Arg2;
    public int ability2Arg3;
    public string ability2HitEffect;
    public int ability2HitEffectPos;
    public string ability2HitEffectSound;

    public int ability3Condition;
    public int ability3ConditionArg1;
    public int ability3ConditionArg2;
    public int ability3ConditionArg3;
    public int ability3Type;
    public int skillAbility3;
    public int ability3Arg1;
    public int ability3Arg2;
    public int ability3Arg3;
    public string ability3HitEffect;
    public int ability3HitEffectPos;
    public string ability3HitEffectSound;

    public override SkillDataDefine ParseData()
    {
        var data = new SkillDataDefine();
        data.id = id;
        data.groupId = groupId;
        data.level = level;
        data.isMoveCameraUp = isMoveCameraUp;
        data.profession = (ActorProfessionEnum)profession;
        data.unlock = (SkillUnlockEnum)unlock;
        data.unluckArg = unluckArg;
        data.skillName = skillName;
        data.targetEnum = (SkillTargetEnum)target;
        data.targetAmount = targetAmount;
        data.comment = comment;
        data.costEffectPos = (EffectPosEnum)costEffectPos;
        if (!string.IsNullOrEmpty(action))
            data.animationEnum = (SpineAnimationEnum)Enum.Parse(typeof(SpineAnimationEnum), action, true);
        data.ignoreCameraMove = ignoreCameraMove;
        for (int i = 0; i < green; i++)
        {
            data.poolColor.Add(SkillCostColorEnum.Green);
        }
        for (int i = 0; i < red; i++)
        {
            data.poolColor.Add(SkillCostColorEnum.Red);
        }
        for (int i = 0; i < blue; i++)
        {
            data.poolColor.Add(SkillCostColorEnum.Blue);
        }

        #region 技能效果 最多3個
        if (skillAbility1 > 0)
        {
            var abilityData = new SkillAbilityDataDefine();
            abilityData.skillAbility = (SkillAbilityEnum)skillAbility1;
            abilityData.abilityCondition = (SkillAbilityConditionEnum)ability1Condition;
            abilityData.conditionArg1 = ability1ConditionArg1;
            abilityData.conditionArg2 = ability1ConditionArg2;
            abilityData.conditionArg3 = ability1ConditionArg3;
            abilityData.abilityArg1 = ability1Arg1;
            abilityData.abilityArg2 = ability1Arg2;
            abilityData.abilityArg3 = ability1Arg3;
            abilityData.hitEffectPos = (EffectPosEnum)ability1HitEffectPos;
            data.skillAbilities.Add(abilityData);
            data.skillType = data.skillType | (SkillTypeEnum)ability1Type;
        }
        if (skillAbility2 > 0)
        {
            var abilityData = new SkillAbilityDataDefine();
            abilityData.skillAbility = (SkillAbilityEnum)skillAbility2;
            abilityData.abilityCondition = (SkillAbilityConditionEnum)ability2Condition;
            abilityData.conditionArg1 = ability2ConditionArg1;
            abilityData.conditionArg2 = ability2ConditionArg2;
            abilityData.conditionArg3 = ability2ConditionArg3;
            abilityData.abilityArg1 = ability2Arg1;
            abilityData.abilityArg2 = ability2Arg2;
            abilityData.abilityArg3 = ability2Arg3;
            abilityData.hitEffectPos = (EffectPosEnum)ability2HitEffectPos;
            data.skillAbilities.Add(abilityData);
            data.skillType = data.skillType | (SkillTypeEnum)ability2Type;
        }
        if (skillAbility3 > 0)
        {
            var abilityData = new SkillAbilityDataDefine();
            abilityData.skillAbility = (SkillAbilityEnum)skillAbility3;
            abilityData.abilityCondition = (SkillAbilityConditionEnum)ability3Condition;
            abilityData.conditionArg1 = ability3ConditionArg1;
            abilityData.conditionArg2 = ability3ConditionArg2;
            abilityData.conditionArg3 = ability3ConditionArg3;
            abilityData.abilityArg1 = ability3Arg1;
            abilityData.abilityArg2 = ability3Arg2;
            abilityData.abilityArg3 = ability3Arg3;
            abilityData.hitEffectPos = (EffectPosEnum)ability3HitEffectPos;
            data.skillAbilities.Add(abilityData);
            data.skillType = data.skillType | (SkillTypeEnum)ability3Type;
        }
        #endregion
        #region 花費條件
        if (colorCost1 > 0)
        {
            var cost = new SkillCostColorData();
            cost.colorEnum = (SkillCostColorEnum)colorCost1;
            cost.count = colorCost1Arg;
            data.costColors.Add(cost);
        }
        if (colorCost2 > 0)
        {
            var cost = new SkillCostColorData();
            cost.colorEnum = (SkillCostColorEnum)colorCost2;
            cost.count = colorCost2Arg;
            data.costColors.Add(cost);
        }
        if (colorCost3 > 0)
        {
            var cost = new SkillCostColorData();
            cost.colorEnum = (SkillCostColorEnum)colorCost3;
            cost.count = colorCost3Arg;
            data.costColors.Add(cost);
        }
        if (colorCost4 > 0)
        {
            var cost = new SkillCostColorData();
            cost.colorEnum = (SkillCostColorEnum)colorCost4;
            cost.count = colorCost4Arg;
            data.costColors.Add(cost);
        }
        #endregion
        return data;
    }

    public override int GetGroupId()
    {
        return groupId;
    }
}