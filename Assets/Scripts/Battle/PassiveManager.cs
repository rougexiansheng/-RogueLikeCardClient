using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// 被動管理
/// </summary>
public class PassiveManager : IInitializable
{
    [Inject]
    GameFlowController gameFlow;
    [Inject]
    AssetManager assetManager;
    [Inject]
    PassiveAbilityMethods passiveMethods;
    [Inject]
    DataTableManager dataTableManager;
    Dictionary<PassiveAbilityConditionEnum, Func<BattleData, PassiveAbilityDataDefine, bool>> passiveAbilityConditionMethods = new Dictionary<PassiveAbilityConditionEnum, Func<BattleData, PassiveAbilityDataDefine, bool>>();
    Dictionary<PassiveAbilityEnum, Action<BattleData, PassiveAbilityDataDefine>> passiveAbilityMethods = new Dictionary<PassiveAbilityEnum, Action<BattleData, PassiveAbilityDataDefine>>();

    public void Initialize()
    {
        AddEffect(PassiveAbilityEnum.None, (a, b) => { });
        AddEffect(PassiveAbilityEnum.ActorAttribute, passiveMethods.ActorAttribute);
        AddEffect(PassiveAbilityEnum.Stun, passiveMethods.Stun);
        AddEffect(PassiveAbilityEnum.ReduceDamageMaxStack, passiveMethods.ReduceDamageMaxStack);
        AddEffect(PassiveAbilityEnum.SkillIncreaseDamage, passiveMethods.SkillIncreaseDamage);
        AddEffect(PassiveAbilityEnum.FightBack, passiveMethods.FightBack);
        AddEffect(PassiveAbilityEnum.ModifyPassiveStack, passiveMethods.ModifyPassiveStack);
        AddEffect(PassiveAbilityEnum.RepeatFirstSkill, passiveMethods.RepeatFirstSkill);
        AddEffect(PassiveAbilityEnum.DoSkill, passiveMethods.DoSkill);
        AddEffect(PassiveAbilityEnum.GetColor, passiveMethods.GetColor);
        AddEffect(PassiveAbilityEnum.GetRandomColor, passiveMethods.GetRandomColor);
        AddEffect(PassiveAbilityEnum.Retaliation, passiveMethods.Retaliation);
        AddEffect(PassiveAbilityEnum.Heal, passiveMethods.Heal);
        AddEffect(PassiveAbilityEnum.ModifyDamage, passiveMethods.ModifyDamage);
        AddEffect(PassiveAbilityEnum.ReduceDamageLimit, passiveMethods.ReduceDamageLimit);
        AddEffect(PassiveAbilityEnum.IgnoreAffrodPassive, passiveMethods.IgnoreAffrodPassive);
        AddEffect(PassiveAbilityEnum.SkillMove, passiveMethods.SkillMove);
        AddEffect(PassiveAbilityEnum.AffrodPassive, passiveMethods.AffrodPassive);
        AddEffect(PassiveAbilityEnum.CrossDamage, passiveMethods.CrossDamage);
        AddEffect(PassiveAbilityEnum.ReduceOnceDamageLimit, passiveMethods.ReduceOnceDamageLimit);
        AddEffect(PassiveAbilityEnum.BannedSkill, passiveMethods.BannedSkill);
        AddEffect(PassiveAbilityEnum.SpecialFightBack, passiveMethods.SpecialFightBack);
        AddEffect(PassiveAbilityEnum.AttributeHealOrDamage, passiveMethods.AttributeHealOrDamage);
        AddEffect(PassiveAbilityEnum.Shield, passiveMethods.Shield);
        AddEffect(PassiveAbilityEnum.ModifyHeal, passiveMethods.ModifyHeal);
        AddEffect(PassiveAbilityEnum.ModifyShield, passiveMethods.ModifyShield);
        AddEffect(PassiveAbilityEnum.ModifyColorCount, passiveMethods.ModifyColorCount);
        AddEffect(PassiveAbilityEnum.AffrodPassiveFromSkillTarget, passiveMethods.AffrodPassiveFromSkillTarget);
        AddEffect(PassiveAbilityEnum.RandomDoSkill, passiveMethods.RandomDoSkill);

        AddCondtion(PassiveAbilityConditionEnum.None, (a, b) => true);
        AddCondtion(PassiveAbilityConditionEnum.HpPercentage, passiveMethods.HpPercentage);
        AddCondtion(PassiveAbilityConditionEnum.DamageSource, passiveMethods.DamageSource);
        AddCondtion(PassiveAbilityConditionEnum.StackCheck, passiveMethods.StackCheck);
        AddCondtion(PassiveAbilityConditionEnum.CheckDamageValue, passiveMethods.CheckDamageValue);
        AddCondtion(PassiveAbilityConditionEnum.SkillGroupIdCheck, passiveMethods.SkillGroupIdCheck);
    }

    bool AddEffect(PassiveAbilityEnum abilityEnum, Action<BattleData, PassiveAbilityDataDefine> func)
    {
        var have = passiveAbilityMethods.ContainsKey(abilityEnum);
        if (!have)
            passiveAbilityMethods.Add(abilityEnum, func);

        else
            Debug.LogWarning($"PassiveAbilityEnum:{abilityEnum} Repeated Addition");
        return have;
    }

    bool AddCondtion(PassiveAbilityConditionEnum conditionEnum, Func<BattleData, PassiveAbilityDataDefine, bool> func)
    {
        var have = passiveAbilityConditionMethods.ContainsKey(conditionEnum);
        if (!have)
            passiveAbilityConditionMethods.Add(conditionEnum, func);

        else
            Debug.LogWarning($"PassiveAbilityConditionEnum:{conditionEnum} Repeated Addition");
        return have;
    }

    int countt = 0;
    /// <summary>
    /// 觸發角色的狀態or清除狀態(檢查所有狀態是否可以執行)
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="triggerEnum"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public void OnActorPassive(BattleActor actor, PassiveTriggerEnum triggerEnum, BattleData battleData = null)
    {

        if (battleData == null)
        {
            battleData = new BattleData();
            battleData.isSkill = false;
        }
        if (countt > 1000)
        {
            Debug.LogError("被動 Loop 超過1000次");
            return;
        }
        // 執行觸發
        var triggerPassiveFirstLs = new List<ActorPassive>();
        var triggerPassiveSecondLs = new List<ActorPassive>();
        var triggerPassiveThirdLs = new List<ActorPassive>();
        for (int i = 0; i < actor.passives.Count; i++)
        {
            var passive = actor.passives[i];
            var passiveDefine = dataTableManager.GetPassiveDefine(passive.passiveId);
            if (passiveDefine.trigger == triggerEnum)
            {
                if (passiveDefine.passivePropertyEnum == PassivePropertyEnum.Heal)
                {
                    triggerPassiveFirstLs.Add(passive);
                }
                else if (passiveDefine.passivePropertyEnum == PassivePropertyEnum.Damage)
                {
                    triggerPassiveSecondLs.Add(passive);
                }
                else
                {
                    triggerPassiveThirdLs.Add(passive);
                }
                countt++;
            }
        }
        DoPassiveList(triggerPassiveFirstLs, actor, triggerEnum, battleData);
        DoPassiveList(triggerPassiveSecondLs, actor, triggerEnum, battleData);
        DoPassiveList(triggerPassiveThirdLs, actor, triggerEnum, battleData);

        var triggerPassiveClearLs = new List<ActorPassive>();
        // 執行清除
        for (int i = 0; i < actor.passives.Count; i++)
        {
            var passive = actor.passives[i];
            if (dataTableManager.GetPassiveDefine(passive.passiveId).clearTrigger == triggerEnum)
            {
                triggerPassiveClearLs.Add(passive);
            }
        }
        for (int i = 0; i < triggerPassiveClearLs.Count; i++)
        {
            var clearPassive = triggerPassiveClearLs[i];
            battleData.modifyActorPassive = clearPassive.Clone();
            battleData.modifyActorPassive.currentStack = dataTableManager.GetPassiveDefine(clearPassive.passiveId).clearCount;
            UtilityHelper.BattleLog($"Name:{actor.actorName} 清除觸發:{triggerEnum}-{(int)triggerEnum} 被動:{battleData.modifyActorPassive.passiveId}-{dataTableManager.GetPassiveDefine(battleData.modifyActorPassive.passiveId).passiveName}", UtilityHelper.BattleLogEnum.Passive);
            ClearPassive(battleData);
        }

        var triggerPassiveKeepLs = new List<ActorPassive>();
        // 執行次數清除
        for (int i = 0; i < actor.passives.Count; i++)
        {
            var passive = actor.passives[i];
            if (dataTableManager.GetPassiveDefine(passive.passiveId).keepTrigger == triggerEnum)
            {
                passive.keepCount++;
                triggerPassiveKeepLs.Add(passive);
            }
        }
        for (int i = 0; i < triggerPassiveKeepLs.Count; i++)
        {
            var clearPassive = triggerPassiveKeepLs[i];
            var passiveDefine = dataTableManager.GetPassiveDefine(clearPassive.passiveId);
            if (passiveDefine.keepCount > 0 && passiveDefine.keepCount <= clearPassive.keepCount)
            {
                battleData.modifyActorPassive = clearPassive.Clone();
                battleData.modifyActorPassive.currentStack = 999;
                UtilityHelper.BattleLog($"Name:{actor.actorName} 次數清除觸發:{triggerEnum}-{(int)triggerEnum} 次數達到:{passiveDefine.keepCount} 被動:{battleData.modifyActorPassive.passiveId}-{passiveDefine.passiveName}", UtilityHelper.BattleLogEnum.Passive);
                ClearPassive(battleData);
            }
        }
        if (triggerPassiveFirstLs.Count > 0 || triggerPassiveSecondLs.Count > 0 || triggerPassiveThirdLs.Count > 0)
            countt = 0;
    }

    void DoPassiveList(List<ActorPassive> ls, BattleActor actor, PassiveTriggerEnum triggerEnum, BattleData battleData)
    {

        for (int i = 0; i < ls.Count; i++)
        {
            battleData.actorPassive = ls[i];

            UtilityHelper.BattleLog($"Name:{actor.actorName} 執行觸發:{triggerEnum}-{(int)triggerEnum} 被動:{battleData.actorPassive.passiveId}-{dataTableManager.    GetPassiveDefine(battleData.actorPassive.passiveId).passiveName}", UtilityHelper.BattleLogEnum.Passive);
            DoPassive(battleData);
        }
    }
    int count = 0;
    /// <summary>
    /// 觸發角色身上指定被動
    /// </summary>
    /// <param name="actorPassive"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    void DoPassive(BattleData battleData)
    {
        count++;
        if (count > 1000)
        {
            Debug.LogError("被動 Loop 超過1000次");
            return;
        }

        var define = dataTableManager.GetPassiveDefine(battleData.actorPassive.passiveId);
        var doEffectAndSound = false;
        var p = new PShowParticleData();
        p.isIgnore = true;
        p.Init(define, battleData.actorPassive.owner);
        gameFlow.AddPerformanceData(p);

        for (int i = 0; i < define.passiveAbilitys.Count; i++)
        {
            var abilityData = define.passiveAbilitys[i];
            if (passiveAbilityConditionMethods.TryGetValue(abilityData.abilityCondition, out Func<BattleData, PassiveAbilityDataDefine, bool> conditionFunc))
            {
                var check = conditionFunc(battleData, abilityData);
                var have = passiveAbilityMethods.TryGetValue(abilityData.passiveAbility, out Action<BattleData, PassiveAbilityDataDefine> abilityFunc);
                if (check && have)
                {
                    UtilityHelper.BattleLog($"Name:{battleData.actorPassive.owner.actorName} 被動:{battleData.actorPassive.passiveId}-{dataTableManager.GetPassiveDefine(battleData.actorPassive.passiveId).passiveName} 執行效果:{abilityData.passiveAbility}", UtilityHelper.BattleLogEnum.Passive);
                    doEffectAndSound = true;
                    abilityFunc(battleData, abilityData);
                    if (abilityData.clearCount > 0)
                    {
                        var newbattleData = battleData.Clone();
                        newbattleData.modifyActorPassive = newbattleData.actorPassive.Clone();
                        newbattleData.modifyActorPassive.currentStack = abilityData.clearCount;
                        ClearPassive(newbattleData);
                    }
                }
                else
                {
                    Debug.LogWarning($"Passive ability id:{battleData.actorPassive.passiveId}-{dataTableManager.GetPassiveDefine(battleData.actorPassive.passiveId).passiveName} not work abitity:{abilityData.passiveAbility} have is:{have}\n" +
                        $"condition is:{check} check:{abilityData.abilityCondition} arg1:{abilityData.conditionArg1} arg2:{abilityData.conditionArg2} arg3:{abilityData.conditionArg3}");

                }
            }
            else
            {
                Debug.LogWarning($"Passive ability id:{battleData.actorPassive.passiveId}-{dataTableManager.GetPassiveDefine(battleData.actorPassive.passiveId).passiveName} dont have:{abilityData.abilityCondition}");
            }
        }
        if (doEffectAndSound)
        {
            p.isIgnore = false;
            UtilityHelper.BattleLog($"name : {define.effect?.name} Pos : {define.effectPos}", UtilityHelper.BattleLogEnum.PassiveEffect);
            UtilityHelper.BattleLog($"name : {define.effectSound?.name}", UtilityHelper.BattleLogEnum.PassiveSound);
        }
        count = 0;
    }

    /// <summary>
    /// 直接指定清除狀態
    /// </summary>
    /// <param name="actorPassive"></param>
    /// <returns></returns>
    public void ClearPassive(BattleData battleData)
    {
        if (battleData.modifyActorPassive.currentStack == 0) return;
        battleData.modifyActorPassive.isAdd = false;
        UtilityHelper.BattleLog($"Passive Name:{dataTableManager.GetPassiveDefine(battleData.modifyActorPassive.passiveId).passiveName} 清除層數:{battleData.modifyActorPassive.currentStack}", UtilityHelper.BattleLogEnum.Passive);
        var target = battleData.modifyActorPassive.owner;
        var passive = target.passives.Find(p => p.passiveId == battleData.modifyActorPassive.passiveId);
        if (passive == null) return;
        OnActorPassive(target, PassiveTriggerEnum.OnPassiveModifyBefore, battleData);
        //被動減少 表演
        var p = new PPassiveData();
        p.Init(target);
        passive.currentStack -= battleData.modifyActorPassive.currentStack;
        if (passive.currentStack <= 0)
        {
            // 是否要表演移除
            p.isRemove = true;
            target.passives.Remove(passive);
            battleData.modifyActorPassive.currentStack = 0;
        }
        else
            battleData.modifyActorPassive = passive;
        UtilityHelper.BattleLog($"Passive Name:{dataTableManager.GetPassiveDefine(battleData.modifyActorPassive.passiveId).passiveName} 剩餘層數:{battleData.modifyActorPassive.currentStack}", UtilityHelper.BattleLogEnum.Passive);
        // 被動修改
        p.passiveId = battleData.modifyActorPassive.passiveId;
        gameFlow.AddPerformanceData(p);
        OnActorPassive(target, PassiveTriggerEnum.OnPassiveModifyAfter, battleData);
    }

    public ActorPassive GainActorPassive(int passiveId, int stackCount)
    {
        var actorPassive = new ActorPassive();
        actorPassive.passiveId = passiveId;
        actorPassive.currentStack = stackCount;
        return actorPassive;
    }

    /// <summary>
    /// 上狀態
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="passiveId"></param>
    /// <param name="stackCount"></param>
    /// <returns></returns>
    public void AddPassive(BattleData battleData)
    {
        battleData.modifyActorPassive.isAdd = true;
        var actor = battleData.modifyActorPassive.owner;
        var passiveDefine = dataTableManager.GetPassiveDefine(battleData.modifyActorPassive.passiveId);
        if (passiveDefine.stackType == PassiveStackEnum.None) return;
        UtilityHelper.BattleLog($"Passive ability ActorName:{actor.actorName} Passive:{battleData.modifyActorPassive.passiveId}-{dataTableManager.GetPassiveDefine(battleData.modifyActorPassive.passiveId).passiveName} 新增層數:{battleData.modifyActorPassive.currentStack}", UtilityHelper.BattleLogEnum.Passive);
        //var passive = actor.passives.Find(p => passiveDefine.groupId == p.define.groupId);
        OnActorPassive(actor, PassiveTriggerEnum.OnPassiveModifyBefore, battleData);
        // 是否有相同的技能(不同等級)
        // 如果層數=0 後面都不執行
        if (battleData.modifyActorPassive.currentStack == 0 || passiveDefine.maxStack <= 0) return;
        var passive = actor.passives.Find(p => passiveDefine.groupId == dataTableManager.GetPassiveDefine(p.passiveId).groupId);
        var p = new PPassiveData();
        p.Init(actor);
        // 疊加層數 
        if (passiveDefine.stackType == PassiveStackEnum.Stack)
        {
            if (passive == null)
            {
                passive = battleData.modifyActorPassive;
                actor.passives.Add(passive);
            }
            else
            {
                // 不同等級/不疊加
                if (passive.passiveId != battleData.modifyActorPassive.passiveId)
                {
                    actor.passives.Add(battleData.modifyActorPassive);
                }
                else
                {
                    // 計算疊層
                    if (passive.currentStack + battleData.modifyActorPassive.currentStack > passiveDefine.maxStack)
                    {
                        passive.currentStack = passiveDefine.maxStack;
                    }
                    else
                    {
                        passive.currentStack = passive.currentStack + battleData.modifyActorPassive.currentStack;
                    }
                }
            }
        }
        else if (passiveDefine.stackType == PassiveStackEnum.Replace)
        {
            if (passive != null) actor.passives.Remove(passive);
            else passive = battleData.modifyActorPassive;
            actor.passives.Add(battleData.modifyActorPassive);
        }
        else if (passiveDefine.stackType == PassiveStackEnum.LevelStack)
        {
            if (passive == null)
            {
                actor.passives.Add(battleData.modifyActorPassive);
                passive = battleData.modifyActorPassive;
            }
            else
            {
                if (dataTableManager.GetPassiveDefine(passive.passiveId).level > passiveDefine.level)
                    return;
                if (dataTableManager.GetPassiveDefine(passive.passiveId).level < passiveDefine.level)
                {
                    var pp = new PPassiveData();
                    pp.Init(passive.owner);
                    pp.isRemove = true;
                    pp.passiveId = passive.passiveId;
                    gameFlow.AddPerformanceData(pp);

                    actor.passives.Remove(passive);
                    actor.passives.Add(battleData.modifyActorPassive);
                }
                if (passive.currentStack + battleData.modifyActorPassive.currentStack > passiveDefine.maxStack)
                {
                    passive.currentStack = passiveDefine.maxStack;
                }
                else
                {
                    passive.currentStack = passive.currentStack + battleData.modifyActorPassive.currentStack;
                }
            }
        }
        passive.keepCount = 0;
        battleData.modifyActorPassive = passive;
        p.passiveId = battleData.modifyActorPassive.passiveId;
        gameFlow.AddPerformanceData(p);
        UtilityHelper.BattleLog($"Passive ability ActorName:{actor.actorName} Passive:{battleData.modifyActorPassive.passiveId}-{dataTableManager.GetPassiveDefine(battleData.modifyActorPassive.passiveId).passiveName} 新增後層數:{battleData.modifyActorPassive.currentStack}", UtilityHelper.BattleLogEnum.Passive);
        OnActorPassive(actor, PassiveTriggerEnum.OnPassiveModifyAfter, battleData);
    }

    public void GetCurrentActorAttribute(BattleActor actor)
    {
        var attribute = actor.currentActorBaseAttribute;

        if (attribute.isDone)
        {
            attribute.isDone = false;
            attribute.attackPower.RestValue(actor.attackPower);
            attribute.currentMove = actor.skillMoveCount;
            attribute.currentColorCount = actor.getColorCount;
            attribute.maxHp.RestValue(actor.maxHp);
            attribute.defense.RestValue(actor.defense);
            attribute.currentSkillRange = actor.skillRange;
            var battleData = new BattleData();
            battleData.sender = actor;
            battleData.currentTarget = actor;
            OnActorPassive(actor, PassiveTriggerEnum.Permanent, battleData);
            attribute.isDone = true;
        }
        else Debug.LogError($"常駐:{actor.actorName} 遞迴了!!!!!!!!!!");
    }
}
