using System;
using System.Linq;
using UnityEngine;
using Zenject;

public class PassiveAbilityMethods
{
    [Inject]
    GameFlowController gameFlow;
    [Inject]
    PassiveManager passiveManager;
    [Inject]
    SkillManager skillManager;
    [Inject]
    BattleManager battleManager;
    [Inject]
    DataTableManager dataTableManager;
    #region 被動效果方法
    enum ActorAttributeEnum
    {
        Def = 1 << 0,
        Atk = 1 << 1,
        MaxHp = 1 << 2,
        MoveCount = 1 << 3,
        CostCount = 1 << 4,
        SkillRange = 1 << 5,
    }
    public void ActorAttribute(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var count = abilityDataDefine.abilityArg2 * battleData.actorPassive.currentStack;
        var lsEnum = Enum.GetValues(typeof(ActorAttributeEnum)).Cast<ActorAttributeEnum>().ToList();
        for (int i = 0; i < lsEnum.Count; i++)
        {
            if (((ActorAttributeEnum)abilityDataDefine.abilityArg1).HasFlag(lsEnum[i]))
            {
                switch (lsEnum[i])
                {
                    case ActorAttributeEnum.Def:
                        battleData.actorPassive.owner.currentActorBaseAttribute.defense.staticValue.Add(count);
                        break;
                    case ActorAttributeEnum.Atk:
                        battleData.actorPassive.owner.currentActorBaseAttribute.attackPower.staticValue.Add(count);
                        break;
                    case ActorAttributeEnum.MaxHp:
                        battleData.actorPassive.owner.currentActorBaseAttribute.maxHp.staticValue.Add(count);
                        break;
                    case ActorAttributeEnum.MoveCount:
                        battleData.actorPassive.owner.currentActorBaseAttribute.currentMove += count;
                        break;
                    case ActorAttributeEnum.CostCount:
                        battleData.actorPassive.owner.currentActorBaseAttribute.currentColorCount += count;
                        break;
                    case ActorAttributeEnum.SkillRange:
                        battleData.actorPassive.owner.currentActorBaseAttribute.currentSkillRange += count;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void Stun(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        battleData.actorPassive.owner.canAttack = Convert.ToBoolean(abilityDataDefine.abilityArg1);
    }
    public void ReduceDamageMaxStack(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var passive = battleData.actorPassive.owner.passives.Find(p => dataTableManager.GetPassiveDefine(p.passiveId).groupId == abilityDataDefine.abilityArg1);
        if (passive != null)
        {
            var i = 0;
            var count = passive.currentStack / abilityDataDefine.abilityArg3;
            for (i = 0; i < count; i++)
            {
                if (battleData.currentDmg.GetValue() <= 0)
                    break;
                battleData.currentDmg.staticValue.Add(-abilityDataDefine.abilityArg2);
            }
            var clearPassive = passive.Clone();
            clearPassive.currentStack = i * abilityDataDefine.abilityArg3;
            battleData.modifyActorPassive = clearPassive;
        }
    }
    public void SkillIncreaseDamage(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var skillDefine = dataTableManager.GetSkillDefine(battleData.skillId);
        if (skillDefine.groupId == abilityDataDefine.abilityArg1)
        {
            battleData.currentDmg.staticValue.Add(battleData.actorPassive.currentStack * abilityDataDefine.abilityArg2);
        }
    }

    public void FightBack(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var newBattleData = battleData.Clone();
        newBattleData.isSkill = false;
        newBattleData.currentTarget = battleData.sender;
        newBattleData.sender = battleData.currentTarget;
        newBattleData.currentDmg.RestValue(abilityDataDefine.abilityArg1 * battleData.actorPassive.currentStack);
        var result = battleManager.OnDamage(newBattleData.currentTarget, newBattleData.currentDmg.GetValue());
        var p = new POnDamageData();
        p.Init(newBattleData.currentTarget, result);
        gameFlow.AddPerformanceData(p);
    }

    public void ModifyPassiveStack(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var self = battleData.actorPassive.owner;
        var passives = self.passives.FindAll(p => dataTableManager.GetPassiveDefine(p.passiveId).groupId == abilityDataDefine.abilityArg1);
        var newBattleData = battleData.Clone();
        for (int i = 0; i < passives.Count; i++)
        {
            var p = passives[i];
            var addP = p.Clone();
            switch (abilityDataDefine.abilityArg3)
            {
                case 1:
                    addP.currentStack = abilityDataDefine.abilityArg2;
                    break;
                case 2:
                    addP.currentStack = -abilityDataDefine.abilityArg2;
                    break;
                case 3:
                    addP.currentStack = p.currentStack * abilityDataDefine.abilityArg2 - p.currentStack;
                    break;
                case 4:
                    addP.currentStack = (int)MathF.Ceiling(p.currentStack / (float)abilityDataDefine.abilityArg2) - p.currentStack;
                    break;
                default:
                    break;
            }
            newBattleData.modifyActorPassive = addP;
            if (addP.currentStack > 0)
                passiveManager.AddPassive(newBattleData);
            else if (addP.currentStack < 0)
            {
                addP.currentStack = Mathf.Abs(addP.currentStack);
                passiveManager.ClearPassive(newBattleData);
            }

        }
    }

    public void RepeatFirstSkill(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var skillDefine = dataTableManager.GetSkillDefine(battleData.originSkill.skillId);
        if (skillDefine.groupId != abilityDataDefine.abilityArg1)
        {
            // 清除一層
            var newBattleData = battleData.Clone();
            newBattleData.modifyActorPassive = battleData.actorPassive.Clone();
            newBattleData.modifyActorPassive.currentStack = 1;
            passiveManager.ClearPassive(newBattleData);
            //執行技能
            skillManager.OnSkill(newBattleData, false);
        }
    }

    public void DoSkill(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var newbattleData = battleData.Clone();
        var actor = newbattleData.actorPassive.owner;
        newbattleData.isSkill = true;
        newbattleData.originSkill = new ActorSkill() { skillId = abilityDataDefine.abilityArg1, originIndex = -1 };
        newbattleData.skillId = abilityDataDefine.abilityArg1;
        if (newbattleData.sender == null)
            newbattleData.sender = actor;
        if (newbattleData.selectTarget == null)
        {
            if (actor.lastBattleData != null)
            {
                newbattleData.selectTarget = actor.lastBattleData.selectTarget;
            }
            else
            {
                if (actor.isPlayer)
                    newbattleData.selectTarget = battleManager.monsters[0];
                else
                    newbattleData.selectTarget = battleManager.player;
            }
        }
        skillManager.OnSkill(newbattleData, false);
    }
    public void GetColor(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var self = battleData.actorPassive.owner;
        var colors = skillManager.GetColorsList((SkillCostColorEnum)abilityDataDefine.abilityArg1);
        battleData.currentColorCount.RestValue(abilityDataDefine.abilityArg2 * battleData.actorPassive.currentStack);
        passiveManager.OnActorPassive(self, PassiveTriggerEnum.PlayerGetCostColorBefore, battleData);
        var value = battleData.currentColorCount.GetValue();
        var p = new PModifyColorData();
        for (int i = 0; i < colors.Count; i++)
        {
            p.SetColorEffectEnum(colors[i], value);
            battleManager.AddColor(self, colors[i], value);
        }
        // 表演獲得能量球
        p.Init(self);
        gameFlow.AddPerformanceData(p);

        passiveManager.OnActorPassive(self, PassiveTriggerEnum.PlayerGetCostColorAfter, battleData);
    }

    public void GetRandomColor(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var self = battleData.actorPassive.owner;
        var colors = skillManager.GetColorsList((SkillCostColorEnum)abilityDataDefine.abilityArg1);
        battleData.currentColorCount.RestValue(abilityDataDefine.abilityArg2 * battleData.actorPassive.currentStack);
        passiveManager.OnActorPassive(self, PassiveTriggerEnum.PlayerGetCostColorBefore, battleData);
        var count = battleData.currentColorCount.GetValue();
        var p = new PModifyColorData();
        for (int i = 0; i < count; i++)
        {
            var r = UnityEngine.Random.Range(0, colors.Count);
            battleManager.AddColor(self, colors[r], 1);
            p.SetColorEffectEnum(colors[r], 1);
        }
        p.Init(self);
        gameFlow.AddPerformanceData(p);
        passiveManager.OnActorPassive(self, PassiveTriggerEnum.PlayerGetCostColorBefore, battleData);
    }

    public void Retaliation(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        if (battleData.currentDmg.GetValue() > 0)
        {
            var newBattleData = battleData.Clone();
            newBattleData.isSkill = false;
            newBattleData.currentTarget = battleData.sender;
            newBattleData.sender = battleData.currentTarget;
            var max = abilityDataDefine.abilityArg1 * battleData.actorPassive.currentStack;
            var dmg = battleData.currentDmg.GetValue();
            if (dmg > max) dmg = max;
            newBattleData.currentDmg.RestValue(dmg);
            var result = battleManager.OnDamage(newBattleData.currentTarget, newBattleData.currentDmg.GetValue());
            var pDmg = new POnDamageData();
            pDmg.Init(newBattleData.currentTarget, result);
            gameFlow.AddPerformanceData(pDmg);
            newBattleData.currentDmg.RestValue(dmg);
        }
    }

    public void Heal(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var target = battleData.actorPassive.owner;
        var passives = target.passives.FindAll(p => dataTableManager.GetPassiveDefine(p.passiveId).groupId == abilityDataDefine.abilityArg1);
        var totalCount = 0;
        for (int i = 0; i < passives.Count; i++)
        {
            totalCount += passives[i].currentStack;
        }
        var heal = totalCount * abilityDataDefine.abilityArg2;
        heal = heal < abilityDataDefine.abilityArg3 ? heal : abilityDataDefine.abilityArg3;
        var newBattleData = battleData.Clone();
        newBattleData.currentHeal.RestValue(heal);
        passiveManager.OnActorPassive(target, PassiveTriggerEnum.OnHealBefore, newBattleData);
        passiveManager.OnActorPassive(target, PassiveTriggerEnum.BeHealBefore, newBattleData);
        var value = battleManager.OnHeal(target, newBattleData.currentHeal.GetValue());
        var p = new POnHealData();
        p.Init(target, value);
        gameFlow.AddPerformanceData(p);
        newBattleData.currentHeal.RestValue(value);

        passiveManager.OnActorPassive(target, PassiveTriggerEnum.BeHealAfter, newBattleData);
        passiveManager.OnActorPassive(target, PassiveTriggerEnum.OnHealAfter, newBattleData);
    }

    public void ModifyDamage(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        //if (battleData.isSkill == false) return;
        var skillDefine = dataTableManager.GetSkillDefine(battleData.skillId);
        if (skillDefine.skillType.HasFlag(SkillTypeEnum.Attack))
        {
            battleData.currentDmg.staticValue.Add(battleData.actorPassive.currentStack * abilityDataDefine.abilityArg1);
        }
    }

    public void ReduceDamageLimit(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var target = battleData.sender;
        var passives = target.passives.FindAll(p => dataTableManager.GetPassiveDefine(p.passiveId).groupId == abilityDataDefine.abilityArg1);
        var totalCount = 0;
        for (int i = 0; i < passives.Count; i++)
        {
            totalCount += passives[i].currentStack;
        }
        var discount = totalCount * abilityDataDefine.abilityArg2;
        discount = discount < abilityDataDefine.abilityArg3 ? discount : abilityDataDefine.abilityArg3;
        battleData.currentDmg.staticValue.Add(-discount);
    }

    public void IgnoreAffrodPassive(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        if (dataTableManager.GetPassiveDefine(battleData.modifyActorPassive.passiveId).groupId == abilityDataDefine.abilityArg1 && battleData.modifyActorPassive.isAdd)
        {
            battleData.modifyActorPassive.currentStack = 0;
        }
    }

    public void SkillMove(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        // 表演轉動技能
        var target = battleData.actorPassive.owner;
        battleData.currentMoveCount = abilityDataDefine.abilityArg1;
        passiveManager.OnActorPassive(target, PassiveTriggerEnum.MoveSkillBySkillBefore, battleData);
        battleManager.PlayerPushSkills(battleData.currentMoveCount, target);
        passiveManager.OnActorPassive(target, PassiveTriggerEnum.MoveSkillBySkillAfter, battleData);
    }

    public void AffrodPassive(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        // 擊中特效and音效
        var stackCount = abilityDataDefine.abilityArg2 <= 0 ? 1 : abilityDataDefine.abilityArg2;
        var actorPassive = passiveManager.GainActorPassive(abilityDataDefine.abilityArg1, stackCount);
        actorPassive.owner = battleData.actorPassive.owner;
        actorPassive.sender = battleData.actorPassive.owner;
        var newBattleData = battleData.Clone();
        newBattleData.modifyActorPassive = actorPassive;
        passiveManager.AddPassive(newBattleData);
    }
    [Flags]
    enum DamageIgnoreEnum
    {
        None,
        Shield,
        Def
    }
    public void CrossDamage(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var target = battleData.actorPassive.owner;
        var dmg = battleData.actorPassive.currentStack * abilityDataDefine.abilityArg1;
        var ignoreEnum = (DamageIgnoreEnum)abilityDataDefine.abilityArg2;
        // TODO受傷表演
        var result = battleManager.OnDamage(target, dmg, ignoreEnum.HasFlag(DamageIgnoreEnum.Shield), ignoreEnum.HasFlag(DamageIgnoreEnum.Def));
        var pDmg = new POnDamageData();
        pDmg.Init(target, result);
        gameFlow.AddPerformanceData(pDmg);
    }

    public void ReduceOnceDamageLimit(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        if (battleData.actorPassive.currentStack > 0)
        {
            battleData.currentDmg.staticValue.Add(-abilityDataDefine.abilityArg1);
        }
    }

    public void BannedSkill(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        Debug.LogError("BannedSkill 被動封印技能 已廢棄 勿使用");
    }

    public void SpecialFightBack(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var newBattleData = battleData.Clone();
        newBattleData.sender = battleData.currentTarget;
        newBattleData.selectTarget = battleData.sender;
        //技能
        if (abilityDataDefine.abilityArg1 == 1)
        {
            newBattleData.isSkill = true;
            newBattleData.skillId = abilityDataDefine.abilityArg2;
            for (int i = 0; i < abilityDataDefine.abilityArg3; i++)
            {
                skillManager.OnSkill(newBattleData, false);
            }
        }
        //被動
        else if (abilityDataDefine.abilityArg1 == 2)
        {
            newBattleData.isSkill = false;
            var actorPassive = passiveManager.GainActorPassive(abilityDataDefine.abilityArg2, abilityDataDefine.abilityArg3);
            actorPassive.owner = battleData.sender;
            actorPassive.sender = battleData.currentTarget;
            newBattleData.modifyActorPassive = actorPassive;
            passiveManager.AddPassive(newBattleData);
        }
    }

    public void AttributeHealOrDamage(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var amount = 0;
        var newBattleData = battleData.Clone();
        passiveManager.GetCurrentActorAttribute(battleData.actorPassive.owner);
        switch (abilityDataDefine.abilityArg1)
        {
            case 1:
                amount = battleData.actorPassive.owner.currentActorBaseAttribute.defense.GetValue();
                break;
            case 2:
                amount = battleData.actorPassive.owner.currentActorBaseAttribute.attackPower.GetValue();
                break;
            case 3:
                amount = battleData.actorPassive.owner.currentActorBaseAttribute.maxHp.GetValue();
                break;
            case 4:
                amount = battleData.actorPassive.owner.currentActorBaseAttribute.currentMove;
                break;
            case 5:
                amount = battleData.actorPassive.owner.currentActorBaseAttribute.currentColorCount;
                break;
            case 6:
                amount = battleData.actorPassive.owner.shield;
                break;
        }
        newBattleData.isSkill = false;
        newBattleData.sender = battleData.actorPassive.sender;
        newBattleData.currentTarget = battleData.actorPassive.owner;
        if (abilityDataDefine.abilityArg2 == 1)
        {
            var dmg = abilityDataDefine.abilityArg3 * amount / 100;
            newBattleData.currentDmg.RestValue(battleManager.GetDamageFromAttckPower(newBattleData.sender, dmg));
            passiveManager.OnActorPassive(newBattleData.sender, PassiveTriggerEnum.OnAttackBefore, newBattleData);
            passiveManager.OnActorPassive(newBattleData.currentTarget, PassiveTriggerEnum.BeAttackBefore, newBattleData);
            passiveManager.OnActorPassive(newBattleData.currentTarget, PassiveTriggerEnum.OnDamage, newBattleData);

            var result = battleManager.OnDamage(battleData.currentTarget, battleData.currentDmg.GetValue());
            var p = new POnDamageData();
            p.Init(battleData.currentTarget, result);
            newBattleData.currentDmg.RestValue(result.Item1);
            gameFlow.AddPerformanceData(p);
            // 擊中特效and音效
            passiveManager.OnActorPassive(newBattleData.currentTarget, PassiveTriggerEnum.BeAttackAfter, newBattleData);
            passiveManager.OnActorPassive(newBattleData.sender, PassiveTriggerEnum.OnAttackAfter, newBattleData);

        }
        else if (abilityDataDefine.abilityArg2 == 2)
        {
            var heal = abilityDataDefine.abilityArg3 * amount / 100;
            newBattleData.currentHeal.RestValue(heal);
            passiveManager.OnActorPassive(newBattleData.sender, PassiveTriggerEnum.OnHealBefore, newBattleData);
            passiveManager.OnActorPassive(newBattleData.currentTarget, PassiveTriggerEnum.BeHealBefore, newBattleData);
            passiveManager.OnActorPassive(newBattleData.currentTarget, PassiveTriggerEnum.OnHeal, newBattleData);
            var value = battleManager.OnHeal(newBattleData.currentTarget, newBattleData.currentHeal.GetValue());
            var p = new POnHealData();
            p.Init(newBattleData.currentTarget, value);
            gameFlow.AddPerformanceData(p);
            newBattleData.currentHeal.RestValue(value);
            passiveManager.OnActorPassive(newBattleData.currentTarget, PassiveTriggerEnum.BeHealAfter, newBattleData);
            passiveManager.OnActorPassive(newBattleData.sender, PassiveTriggerEnum.OnHealAfter, newBattleData);
        }
    }

    public void Shield(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var actor = battleData.actorPassive.owner;
        var newBattleData = battleData.Clone();
        newBattleData.currentShield.RestValue(abilityDataDefine.abilityArg1 * newBattleData.actorPassive.currentStack);
        passiveManager.OnActorPassive(actor, PassiveTriggerEnum.OnShieldBefore, newBattleData);
        passiveManager.OnActorPassive(actor, PassiveTriggerEnum.BeShieldBefore, newBattleData);
        passiveManager.OnActorPassive(actor, PassiveTriggerEnum.OnShield, newBattleData);
        var p = new PModifyShieldData();
        p.beforeValue = actor.shield;

        actor.shield += newBattleData.currentShield.GetValue();

        p.isPlayer = actor.isPlayer;
        p.monsterPosition = actor.monsterPos;
        p.shieldValue = actor.shield;
        gameFlow.AddPerformanceData(p);
        passiveManager.OnActorPassive(actor, PassiveTriggerEnum.BeShieldAfter, newBattleData);
        passiveManager.OnActorPassive(actor, PassiveTriggerEnum.OnShieldAfter, newBattleData);
    }

    public void ModifyHeal(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        battleData.currentHeal.staticValue.Add(abilityDataDefine.abilityArg1 * battleData.actorPassive.currentStack);
    }

    public void ModifyShield(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        battleData.currentShield.staticValue.Add(abilityDataDefine.abilityArg1 * battleData.actorPassive.currentStack);
    }

    public void ModifyColorCount(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        battleData.currentColorCount.staticValue.Add(abilityDataDefine.abilityArg1 * battleData.actorPassive.currentStack);
    }

    public void AffrodPassiveFromSkillTarget(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        if (abilityDataDefine.abilityArg2 == 0) return;
        var actorPassive = passiveManager.GainActorPassive(abilityDataDefine.abilityArg1, Mathf.Abs(abilityDataDefine.abilityArg2));
        actorPassive.sender = battleData.sender;
        actorPassive.owner = battleData.currentTarget;
        var newBattleData = battleData.Clone();
        newBattleData.modifyActorPassive = actorPassive;
        if (abilityDataDefine.abilityArg2 > 0)
        {
            passiveManager.AddPassive(newBattleData);
        }
        else if (abilityDataDefine.abilityArg2 < 0)
        {
            passiveManager.ClearPassive(newBattleData);
        }
    }

    public void RandomDoSkill(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var r = UnityEngine.Random.Range(0, 100);
        if (r > abilityDataDefine.abilityArg2)
        {
            var newbattleData = battleData.Clone();
            var actor = newbattleData.actorPassive.owner;
            newbattleData.isSkill = true;
            newbattleData.skillId = abilityDataDefine.abilityArg1;
            if (newbattleData.sender == null)
                newbattleData.sender = actor;
            if (newbattleData.selectTarget == null)
            {
                if (actor.lastBattleData != null)
                {
                    newbattleData.selectTarget = actor.lastBattleData.selectTarget;
                }
                else
                {
                    if (actor.isPlayer)
                        newbattleData.selectTarget = battleManager.monsters[0];
                    else
                        newbattleData.selectTarget = battleManager.player;
                }
            }
            skillManager.OnSkill(newbattleData, false);
        }
    }
    #endregion

    #region 被動效果條件
    public bool HpPercentage(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        passiveManager.GetCurrentActorAttribute(battleData.actorPassive.owner);
        var percentage = battleData.actorPassive.owner.currentHp * 100 / battleData.actorPassive.owner.currentActorBaseAttribute.maxHp.GetValue();
        switch (abilityDataDefine.conditionArg1)
        {
            //大於
            case 1:
                return percentage > abilityDataDefine.conditionArg2;
            //小於
            case 2:
                return percentage < abilityDataDefine.conditionArg2;
            //等於
            case 3:
                return percentage == abilityDataDefine.conditionArg2;
            //大於等於
            case 4:
                return percentage >= abilityDataDefine.conditionArg2;
            //小於等於
            case 5:
                return percentage <= abilityDataDefine.conditionArg2;
            default:
                return false;
        }
    }
    public bool DamageSource(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var fistCheck = true;
        var targetIsSelf = true;
        if (abilityDataDefine.conditionArg1 == 1)
            fistCheck = battleData.isSkill;
        else if (abilityDataDefine.conditionArg1 == 2)
            fistCheck = !battleData.isSkill;
        if (abilityDataDefine.conditionArg2 == 1)
            targetIsSelf = battleData.sender != battleData.actorPassive.owner && battleData.sender != battleData.currentTarget;
        return fistCheck && targetIsSelf;
    }
    public bool StackCheck(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var passives = battleData.actorPassive.owner.passives.FindAll(p => dataTableManager.GetPassiveDefine(p.passiveId).groupId == abilityDataDefine.conditionArg1);
        var totalStack = 0;
        for (int i = 0; i < passives.Count; i++)
        {
            totalStack = +passives[i].currentStack;
        }
        switch (abilityDataDefine.conditionArg2)
        {
            case 1:
                return totalStack > abilityDataDefine.conditionArg3;
            case 2:
                return totalStack < abilityDataDefine.conditionArg3;
            case 3:
                return totalStack == abilityDataDefine.conditionArg3;
            default:
                return false;
        }
    }

    public bool CheckDamageValue(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        var firstCheck = false;
        if (abilityDataDefine.conditionArg1 == 1) firstCheck = true;
        else if (abilityDataDefine.conditionArg1 == 1)
        {
            firstCheck = battleData.isSkill;
        }
        else if (abilityDataDefine.conditionArg1 == 2)
        {
            firstCheck = !battleData.isSkill;
        }
        if (!firstCheck) return firstCheck;

        var value = battleData.currentDmg.GetValue();
        switch (abilityDataDefine.conditionArg2)
        {
            case 1:
                return value > abilityDataDefine.conditionArg3;
            case 2:
                return value < abilityDataDefine.conditionArg3;
            case 3:
                return value == abilityDataDefine.conditionArg3;
            default:
                return false;
        }
    }

    public bool SkillGroupIdCheck(BattleData battleData, PassiveAbilityDataDefine abilityDataDefine)
    {
        if (battleData.originSkill.skillId == battleData.skillId)
        {
            var skillDefine = dataTableManager.GetSkillDefine(battleData.skillId);
            return skillDefine.groupId == abilityDataDefine.conditionArg1 || skillDefine.groupId == abilityDataDefine.conditionArg2 || skillDefine.groupId == abilityDataDefine.conditionArg3;
        }
        else return false;
    }
    #endregion
}
