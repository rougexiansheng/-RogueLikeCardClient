using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveDataDefine
{
    public int id;
    public string passiveName;
    public int level;
    public int groupId;
    public PassiveTypeEnum passiveType;
    public PassiveTriggerEnum trigger;
    /// <summary>是否不可見</summary>
    public bool isInvisible;
    public int maxStack;
    public PassiveStackEnum stackType;
    public PassiveTriggerEnum clearTrigger;

    public PassiveTriggerEnum keepTrigger;
    public int keepCount;
    /// <summary>觸發時機清除層數</summary>
    public int clearCount;
    public ParticleItem effect;
    public EffectPosEnum effectPos;
    public AudioClip effectSound;
    public Sprite icon;
    public string comment;
    public PassivePropertyEnum passivePropertyEnum;
    public List<PassiveAbilityDataDefine> passiveAbilitys = new List<PassiveAbilityDataDefine>();
}

public class PassiveAbilityDataDefine
{
    public PassiveAbilityConditionEnum abilityCondition;
    public int conditionArg1;
    public int conditionArg2;
    public int conditionArg3;

    public PassiveAbilityEnum passiveAbility;
    public int abilityArg1;
    public int abilityArg2;
    public int abilityArg3;
    /// <summary>執行效果清除層數</summary>
    public int clearCount;
}

public class PassiveOriginDefine : OriginDefineBase<PassiveDataDefine>
{
    public string passiveName;
    public int level;
    public int groupId;
    public int passiveType;
    public int trigger;
    public int maxStack;
    public int stackType;
    public int clearTrigger;
    /// <summary>是否不可見</summary>
    public bool isInvisible;
    public int passiveProperty;
    public int keepTrigger;
    public int keepCount;
    /// <summary>觸發時機清除層數</summary>
    public int clearCount;
    public string effectName;
    public int effectPos;
    public string effectSound;
    public string icon;
    public string comment;

    public int ability1Condition;
    public int ability1ConditionArg1;
    public int ability1ConditionArg2;
    public int ability1ConditionArg3;
    public int passiveAbility1;
    public int ability1Arg1;
    public int ability1Arg2;
    public int ability1Arg3;
    /// <summary>執行效果清除層數</summary>
    public int clearCount1;

    public int ability2Condition;
    public int ability2ConditionArg1;
    public int ability2ConditionArg2;
    public int ability2ConditionArg3;
    public int passiveAbility2;
    public int ability2Arg1;
    public int ability2Arg2;
    public int ability2Arg3;
    /// <summary>執行效果清除層數</summary>
    public int clearCount2;

    public override PassiveDataDefine ParseData()
    {
        var d = new PassiveDataDefine();
        d.id = id;
        d.passiveName = passiveName;
        d.level = level;
        d.groupId = groupId;
        d.passiveType = (PassiveTypeEnum)passiveType;
        d.trigger = (PassiveTriggerEnum)trigger;
        d.maxStack = maxStack;
        d.stackType = (PassiveStackEnum)stackType;
        d.clearTrigger = (PassiveTriggerEnum)clearTrigger;
        d.clearCount = clearCount;
        d.effectPos = (EffectPosEnum)effectPos;
        d.comment = comment;
        d.passivePropertyEnum = (PassivePropertyEnum)passiveProperty;
        d.keepTrigger = (PassiveTriggerEnum)keepTrigger;
        d.keepCount = keepCount;
        d.isInvisible = isInvisible;
        if (passiveAbility1 > 0)
        {
            var ability = new PassiveAbilityDataDefine();
            ability.passiveAbility = (PassiveAbilityEnum)passiveAbility1;
            ability.abilityArg1 = ability1Arg1;
            ability.abilityArg2 = ability1Arg2;
            ability.abilityArg3 = ability1Arg3;
            ability.abilityCondition = (PassiveAbilityConditionEnum)ability1Condition;
            ability.conditionArg1 = ability1ConditionArg1;
            ability.conditionArg2 = ability1ConditionArg2;
            ability.conditionArg3 = ability1ConditionArg3;
            ability.clearCount = clearCount1;
            d.passiveAbilitys.Add(ability);
        }
        if (passiveAbility2 > 0)
        {
            var ability = new PassiveAbilityDataDefine();
            ability.passiveAbility = (PassiveAbilityEnum)passiveAbility2;
            ability.abilityArg1 = ability2Arg1;
            ability.abilityArg2 = ability2Arg2;
            ability.abilityArg3 = ability2Arg3;
            ability.abilityCondition = (PassiveAbilityConditionEnum)ability2Condition;
            ability.conditionArg1 = ability2ConditionArg1;
            ability.conditionArg2 = ability2ConditionArg2;
            ability.conditionArg3 = ability2ConditionArg3;
            ability.clearCount = clearCount2;
            d.passiveAbilitys.Add(ability);
        }
        return d;
    }
}
