using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SkillAbilityMethods
{
    [Inject]
    EnvironmentManager environmentManager;
    [Inject]
    GameFlowController gameFlow;
    [Inject]
    PassiveManager passiveManager;
    [Inject]
    SkillManager skillManager;
    [Inject]
    BattleManager battleManager;
    [Inject]
    MonsterManager monsterManager;
    [Inject]
    DataTableManager dataTableManager;

    #region 技能效果方法
    public void Damage(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        battleData.currentDmg.RestValue(battleManager.GetDamageFromAttckPower(battleData.sender, abilityData.abilityArg1));
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnAttackBefore, battleData);
        passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.BeAttackBefore, battleData);

        passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.OnDamage, battleData);

        var result = battleManager.OnDamage(battleData.currentTarget, battleData.currentDmg.GetValue());
        // 加入受傷表演and擊中特效
        var pDmg = new POnDamageData();
        pDmg.Init(battleData.currentTarget, result);
        battleData.pMultipleDatas.performanceDatas.Add(pDmg);
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);

        battleData.currentDmg.RestValue(result.Item1);
        passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.BeAttackAfter, battleData);
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnAttackAfter, battleData);
    }

    public void GetColor(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        // 加入擊中特效
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);
        //表演獲得能量球
        var p = new PModifyColorData();
        var colors = skillManager.GetColorsList((SkillCostColorEnum)abilityData.abilityArg1);
        for (int i = 0; i < colors.Count; i++)
        {
            battleData.currentColorCount.RestValue(abilityData.abilityArg2);
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.PlayerGetCostColorBefore, battleData);
            var value = battleData.currentColorCount.GetValue();
            p.SetColorEffectEnum(colors[i], value);
            battleManager.AddColor(battleData.sender, colors[i], value);
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.PlayerGetCostColorAfter, battleData);
        }
        p.Init(battleData.sender);
        gameFlow.AddPerformanceData(p);
    }

    public void ActiveSkill(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);

        for (int i = 0; i < abilityData.abilityArg2; i++)
        {
            var newBattleData = battleData.Clone();
            newBattleData.isSkill = true;
            newBattleData.skillId = abilityData.abilityArg1;
            skillManager.OnSkill(newBattleData, false);
        }
    }

    public void AffrodPassive(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);
        if (abilityData.abilityArg2 == 0) return;
        var actorPassive = passiveManager.GainActorPassive(abilityData.abilityArg1, Mathf.Abs(abilityData.abilityArg2));
        actorPassive.sender = battleData.sender;
        actorPassive.owner = battleData.currentTarget;
        var newBattleData = battleData.Clone();
        newBattleData.modifyActorPassive = actorPassive;
        if (abilityData.abilityArg2 > 0)
        {
            passiveManager.AddPassive(newBattleData);
        }
        else if (abilityData.abilityArg2 < 0)
        {
            passiveManager.ClearPassive(newBattleData);
        }
    }

    public void SkillMove(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        // 擊中特效and音效
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);

        var target = battleData.currentTarget;
        battleData.currentMoveCount = abilityData.abilityArg1;
        passiveManager.OnActorPassive(target, PassiveTriggerEnum.MoveSkillBySkillBefore, battleData);
        battleManager.PlayerPushSkills(battleData.currentMoveCount, target);
        passiveManager.OnActorPassive(target, PassiveTriggerEnum.MoveSkillBySkillAfter, battleData);
    }

    public void KilledActiveSkill(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        battleData.currentDmg.RestValue(battleManager.GetDamageFromAttckPower(battleData.sender, abilityData.abilityArg1));
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnAttackBefore, battleData);
        passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.BeAttackBefore, battleData);
        // 傷害前死亡 不執行額外技能
        passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.OnDamage, battleData);
        bool alreadyDead = battleData.currentTarget.isDead;

        var result = battleManager.OnDamage(battleData.currentTarget, battleData.currentDmg.GetValue());
        // 加入同時執行特效表演
        var pDmg = new POnDamageData();
        pDmg.Init(battleData.currentTarget, result);
        battleData.pMultipleDatas.performanceDatas.Add(pDmg);
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);
        battleData.pMultipleDatas.AddPShowParticle();
        battleData.currentDmg.RestValue(result.Item1);
        passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.BeAttackAfter, battleData);
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnAttackAfter, battleData);

        // 確定是在傷害後死亡 執行
        if (battleData.currentTarget.isDead && !alreadyDead)
        {
            var newBattleData = battleData.Clone();
            newBattleData.isSkill = true;
            newBattleData.selectTarget = battleData.currentTarget;
            newBattleData.skillId = abilityData.abilityArg2;
            skillManager.OnSkill(newBattleData, false);
        }
    }

    public void TargetBuffStackAffrodPassiveForTarget(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        // 擊中特效and音效
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);

        var passives = battleData.currentTarget.passives.FindAll(p => dataTableManager.GetPassiveDefine(p.passiveId).groupId == abilityData.abilityArg2);
        var stackCount = 0;
        for (int i = 0; i < passives.Count; i++)
        {
            stackCount += passives[i].currentStack;
        }
        var count = stackCount / abilityData.abilityArg1;
        if (count > 0)
        {
            var actorPassive = passiveManager.GainActorPassive(abilityData.abilityArg3, count);
            actorPassive.sender = battleData.sender;
            actorPassive.owner = battleData.currentTarget;
            var newbattleData = battleData.Clone();
            newbattleData.modifyActorPassive = actorPassive;
            passiveManager.AddPassive(newbattleData);
        }
    }

    public void TargetBuffStackAffrodPassiveForSelf(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        // 擊中特效and音效
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);

        var passives = battleData.currentTarget.passives.FindAll(p => dataTableManager.GetPassiveDefine(p.passiveId).groupId == abilityData.abilityArg2);
        var stackCount = 0;
        for (int i = 0; i < passives.Count; i++)
        {
            stackCount += passives[i].currentStack;
        }
        var count = stackCount / abilityData.abilityArg1;
        if (count > 0)
        {
            var actorPassive = passiveManager.GainActorPassive(abilityData.abilityArg3, count);
            actorPassive.sender = battleData.sender;
            actorPassive.owner = battleData.sender;
            var newbattleData = battleData.Clone();
            newbattleData.modifyActorPassive = actorPassive;
            passiveManager.AddPassive(newbattleData);
        }
    }

    public void SelfBuffStackAffrodPassiveForTarget(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        // 擊中特效and音效
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);

        var passives = battleData.sender.passives.FindAll(p => dataTableManager.GetPassiveDefine(p.passiveId).groupId == abilityData.abilityArg2);
        var stackCount = 0;
        for (int i = 0; i < passives.Count; i++)
        {
            stackCount += passives[i].currentStack;
        }
        var count = stackCount / abilityData.abilityArg1;
        if (count > 0)
        {
            var actorPassive = passiveManager.GainActorPassive(abilityData.abilityArg3, count);
            actorPassive.sender = battleData.sender;
            actorPassive.owner = battleData.currentTarget;
            var newbattleData = battleData.Clone();
            newbattleData.modifyActorPassive = actorPassive;
            passiveManager.AddPassive(newbattleData);
        }
    }

    public void BannedSkill(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var target = battleData.currentTarget;
        ActorSkill skill = null;
        // 參數技能類型
        switch (abilityData.abilityArg1)
        {
            // 技能原始位置
            case 1:
                skill = target.skills.Find(s => s.originIndex == abilityData.abilityArg2);
                break;
            // 技能群組ID
            case 2:
                skill = target.skills.Find(s =>
                {
                    var define = dataTableManager.GetSkillDefine(s.skillId);
                    return define.groupId == abilityData.abilityArg2;
                });
                break;
            // 技能ID
            case 3:
                skill = target.skills.Find(s => s.skillId == abilityData.abilityArg2);
                break;
            default:
                break;
        }
        var p = new PPlayerSkillDataBanned();
        var idx = target.skills.IndexOf(skill);
        if (idx >= 0) p.indexs.Add(idx);
        if (skill == null)
        {
            Debug.LogError($"類型:{abilityData.abilityArg1} 數值:{abilityData.abilityArg2} 是否新增:{abilityData.abilityArg3}");
            return;
        }
        else if (abilityData.abilityArg3 == 1)
        {
            p.bannedEnum = PPlayerSkillDataBanned.BannedTypeEnum.Start;
            skill.isBanned = true;
        }
        else if (abilityData.abilityArg3 == 0)
        {
            p.bannedEnum = PPlayerSkillDataBanned.BannedTypeEnum.End;
            skill.isBanned = false;
        }
        else
        {
            Debug.LogError("abilityData.abilityArg3 is not 1/0");
        }
        gameFlow.AddPerformanceData(p);
    }

    public void CostColorCountMultipleSkill(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var total = 0;
        for (int i = 0; i < battleData.costs.Count; i++)
        {
            var cost = battleData.costs[i];
            foreach (var item in cost)
            {
                if (((SkillCostColorEnum)abilityData.abilityArg2).HasFlag(item.Key))
                {
                    total += item.Value;
                }
            }
        }
        var count = total / abilityData.abilityArg1;
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);

        for (int i = 0; i < count; i++)
        {
            var newBattleData = battleData.Clone();
            newBattleData.isSkill = true;
            newBattleData.skillId = abilityData.abilityArg3;
            skillManager.OnSkill(newBattleData, false);
            //battleData.currentDmg.RestValue(battleManager.GetDamageFromAttckPower(battleData.sender, abilityData.abilityArg3));
            //// 擊中特效and音效
            //passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnAttackBefore, battleData);
            //passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.BeAttackBefore, battleData);

            //passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.BeAttackBefore, battleData);
            //battleData.currentDmg.RestValue(battleManager.OnDamage(battleData.currentTarget, battleData.currentDmg.GetValue()));

            //passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.BeAttackAfter, battleData);
            //passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnAttackAfter, battleData);
        }
    }
    enum CostRowEnum
    {
        One = 1 << 0,
        Two = 1 << 1,
        Three = 1 << 2,
        Four = 1 << 3
    }

    public void CostCountRowMultipleSkill(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var rows = (CostRowEnum)abilityData.abilityArg1;
        var total = GetCostRowCont(battleData, rows);
        var count = total / abilityData.abilityArg2;
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);

        for (int i = 0; i < count; i++)
        {
            var newBattleData = battleData.Clone();
            newBattleData.isSkill = true;
            newBattleData.skillId = abilityData.abilityArg3;
            skillManager.OnSkill(newBattleData, false);
        }
    }

    public void CostCountRowDamage(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var rows = (CostRowEnum)abilityData.abilityArg2;
        var total = GetCostRowCont(battleData, rows);
        var dmg = total * (abilityData.abilityArg3 / 100f);
        battleData.currentDmg.RestValue(battleManager.GetDamageFromAttckPower(battleData.sender, (int)dmg + abilityData.abilityArg1));

        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnAttackBefore, battleData);
        passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.BeAttackBefore, battleData);
        passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.OnDamage, battleData);
        var result = battleManager.OnDamage(battleData.currentTarget, battleData.currentDmg.GetValue());
        // 擊中特效and音效
        var pDmg = new POnDamageData();
        pDmg.Init(battleData.currentTarget, result);
        battleData.pMultipleDatas.performanceDatas.Add(pDmg);
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);
        battleData.currentDmg.RestValue(result.Item1);

        passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.BeAttackAfter, battleData);
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnAttackAfter, battleData);
    }

    public void Heal(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        battleData.currentHeal.RestValue(abilityData.abilityArg1);

        // 觸發治療前
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnHealBefore, battleData);
        passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.BeHealBefore, battleData);
        passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.OnHeal, battleData);
        // 擊中特效and音效
        var value = battleManager.OnHeal(battleData.currentTarget, battleData.currentHeal.GetValue());
        var p = new POnHealData();
        p.Init(battleData.currentTarget, value);
        battleData.pMultipleDatas.performanceDatas.Add(p);

        battleData.currentHeal.RestValue(value);
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);
        // 觸發治療後
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.BeHealAfter, battleData);
        passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.OnHealAfter, battleData);
    }

    public void GetRandomColor(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        // 擊中特效and音效
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);

        var colors = skillManager.GetColorsList((SkillCostColorEnum)abilityData.abilityArg1);
        battleData.currentColorCount.RestValue(abilityData.abilityArg2);
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.PlayerGetCostColorBefore, battleData);
        var count = battleData.currentColorCount.GetValue();
        //表演獲得能量球
        var p = new PModifyColorData();
        for (int i = 0; i < count; i++)
        {
            var r = UnityEngine.Random.Range(0, colors.Count);
            battleManager.AddColor(battleData.sender, colors[r], 1);
            p.SetColorEffectEnum(colors[r], 1);
        }
        p.Init(battleData.sender);
        gameFlow.AddPerformanceData(p);

        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.PlayerGetCostColorAfter, battleData);
    }

    public void ClearPassiveHeal(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var passives = battleData.currentTarget.passives.FindAll(p => dataTableManager.GetPassiveDefine(p.passiveId).groupId == abilityData.abilityArg2);
        var total = 0;
        for (int i = 0; i < passives.Count; i++)
        {
            total += passives[i].currentStack;
        }
        var count = total / abilityData.abilityArg1;
        battleData.currentHeal.RestValue(count * abilityData.abilityArg3);
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnHealBefore, battleData);
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.BeHealBefore, battleData);
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnHeal, battleData);
        // 擊中特效and音效
        var value = battleManager.OnHeal(battleData.sender, battleData.currentHeal.GetValue());
        var p = new POnHealData();
        p.Init(battleData.sender, value);
        battleData.pMultipleDatas.performanceDatas.Add(p);
        battleData.currentHeal.RestValue(value);
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);
        battleData.currentHeal.RestValue(value);

        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.BeHealAfter, battleData);
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnHealAfter, battleData);

        for (int i = 0; i < passives.Count; i++)
        {
            battleData.modifyActorPassive = passives[i].Clone();
            passiveManager.ClearPassive(battleData);
        }
    }

    public void ClearPassiveGetColor(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var passives = battleData.currentTarget.passives.FindAll(p => dataTableManager.GetPassiveDefine(p.passiveId).groupId == abilityData.abilityArg2);
        var total = 0;
        for (int i = 0; i < passives.Count; i++)
        {
            total += passives[i].currentStack;
        }
        var count = total / abilityData.abilityArg1;
        count *= abilityData.abilityArg3;
        var colors = new List<SkillCostColorEnum>()
        {
            SkillCostColorEnum.Red,
            SkillCostColorEnum.Green,
            SkillCostColorEnum.Blue
        };
        battleData.currentColorCount.RestValue(count);
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.PlayerGetCostColorBefore, battleData);
        // 擊中特效and音效
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);
        count = battleData.currentColorCount.GetValue();
        for (int i = 0; i < count; i++)
        {
            var r = UnityEngine.Random.Range(0, colors.Count);
            battleManager.AddColor(battleData.sender, colors[r], 1);
        }
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.PlayerGetCostColorAfter, battleData);
    }

    public void ReduceHpActiveSkill(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var selfHp = battleData.sender.currentHp;
        var hurtSelf = selfHp - 1;
        var dmg = hurtSelf * abilityData.abilityArg1;
        battleData.sender.currentHp = 1;
        var p = new POnDamageData();
        p.Init(battleData.sender, (hurtSelf, false));
        gameFlow.AddPerformanceData(p);

        var isDead = battleData.currentTarget.isDead;
        battleData.currentDmg.baseValue = battleManager.GetDamageFromAttckPower(battleData.sender, dmg);

        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnAttackBefore, battleData);
        passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.BeAttackBefore, battleData);

        // 擊中特效and音效
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);
        passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.OnDamage, battleData);
        var result = battleManager.OnDamage(battleData.currentTarget, battleData.currentDmg.GetValue());
        p = new POnDamageData();
        p.Init(battleData.currentTarget, result);
        gameFlow.AddPerformanceData(p);
        battleData.currentDmg.RestValue(result.Item1);

        passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.BeAttackAfter, battleData);
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnAttackAfter, battleData);
        // 傷害前死亡 不執行額外技能
        if (!isDead && battleData.currentTarget.isDead)
        {
            var newBattleData = battleData.Clone();
            newBattleData.isSkill = true;
            newBattleData.skillId = abilityData.abilityArg2;
            skillManager.OnSkill(newBattleData, false);
        }
    }
    public void TargetPassiveStackHealOrDamage(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var passives = battleData.currentTarget.passives.FindAll(p => dataTableManager.GetPassiveDefine(p.passiveId).groupId == abilityData.abilityArg1);
        var total = 0;
        for (int i = 0; i < passives.Count; i++)
        {
            total += passives[i].currentStack;
        }

        var amount = total * abilityData.abilityArg3;

        if (abilityData.abilityArg2 == 1)
        {
            battleData.currentDmg.RestValue(battleManager.GetDamageFromAttckPower(battleData.sender, amount));
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnAttackBefore, battleData);
            passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.BeAttackBefore, battleData);
            // 擊中特效and音效
            battleData.pMultipleDatas.AddPShowParticle();
            gameFlow.AddPerformanceData(battleData.pMultipleDatas);
            passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.OnDamage, battleData);
            var result = battleManager.OnDamage(battleData.currentTarget, battleData.currentDmg.GetValue());
            var p = new POnDamageData();
            p.Init(battleData.currentTarget, result);
            battleData.currentDmg.RestValue(result.Item1);
            gameFlow.AddPerformanceData(p);
            passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.BeAttackAfter, battleData);
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnAttackAfter, battleData);
        }
        else if (abilityData.abilityArg2 == 2)
        {
            battleData.currentHeal.RestValue(amount);
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnHealBefore, battleData);
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.BeHealBefore, battleData);
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnHeal, battleData);
            // 擊中特效and音效
            var value = battleManager.OnHeal(battleData.sender, battleData.currentHeal.GetValue());
            var p = new POnHealData();
            p.Init(battleData.sender, value);
            battleData.pMultipleDatas.performanceDatas.Add(p);
            battleData.currentHeal.RestValue(value);
            battleData.pMultipleDatas.AddPShowParticle();
            gameFlow.AddPerformanceData(battleData.pMultipleDatas);
            battleData.currentHeal.RestValue(value);

            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.BeHealAfter, battleData);
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnHealAfter, battleData);
        }
    }

    public void ModifyShield(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var actor = abilityData.abilityArg1 == 1 ? battleData.sender : battleData.currentTarget;
        battleData.currentShield.RestValue(abilityData.abilityArg2);
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnShieldBefore, battleData);
        passiveManager.OnActorPassive(actor, PassiveTriggerEnum.BeShieldBefore, battleData);
        // 擊中特效and音效
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);
        passiveManager.OnActorPassive(actor, PassiveTriggerEnum.OnShield, battleData);
        var shield = battleData.currentShield.GetValue();
        var p = new PModifyShieldData();
        p.beforeValue = actor.shield;

        actor.shield += shield;

        p.isPlayer = actor.isPlayer;
        p.monsterPosition = actor.monsterPos;
        p.shieldValue = actor.shield;
        gameFlow.AddPerformanceData(p);
        battleData.currentShield.RestValue(shield);
        passiveManager.OnActorPassive(actor, PassiveTriggerEnum.BeShieldAfter, battleData);
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnShieldAfter, battleData);
    }

    public void AttributeHealOrDamage(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var amount = 0;
        passiveManager.GetCurrentActorAttribute(battleData.currentTarget);
        switch (abilityData.abilityArg1)
        {
            case 1:
                amount = battleData.currentTarget.currentActorBaseAttribute.defense.GetValue();
                break;
            case 2:
                amount = battleData.currentTarget.currentActorBaseAttribute.attackPower.GetValue();
                break;
            case 3:
                amount = battleData.currentTarget.currentActorBaseAttribute.maxHp.GetValue();
                break;
            case 4:
                amount = battleData.currentTarget.currentActorBaseAttribute.currentMove;
                break;
            case 5:
                amount = battleData.currentTarget.currentActorBaseAttribute.currentColorCount;
                break;
            case 6:
                amount = battleData.currentTarget.shield;
                break;
        }
        if (abilityData.abilityArg2 == 1)
        {
            var dmg = abilityData.abilityArg3 * amount / 100;
            battleData.currentDmg.RestValue(battleManager.GetDamageFromAttckPower(battleData.sender, dmg));
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnAttackBefore, battleData);
            passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.BeAttackBefore, battleData);

            // 擊中特效and音效
            battleData.pMultipleDatas.AddPShowParticle();
            gameFlow.AddPerformanceData(battleData.pMultipleDatas);
            passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.OnDamage, battleData);
            var result = battleManager.OnDamage(battleData.currentTarget, battleData.currentDmg.GetValue());
            var p = new POnDamageData();
            p.Init(battleData.currentTarget, result);
            battleData.currentDmg.RestValue(result.Item1);
            gameFlow.AddPerformanceData(p);

            passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.BeAttackAfter, battleData);
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnAttackAfter, battleData);

        }
        else if (abilityData.abilityArg2 == 2)
        {
            var heal = abilityData.abilityArg3 * amount / 100;
            battleData.currentHeal.RestValue(heal);
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnHealBefore, battleData);
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.BeHealBefore, battleData);
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnHeal, battleData);
            // 擊中特效and音效
            var value = battleManager.OnHeal(battleData.sender, battleData.currentHeal.GetValue());
            var p = new POnHealData();
            p.Init(battleData.sender, value);
            battleData.pMultipleDatas.performanceDatas.Add(p);
            battleData.pMultipleDatas.AddPShowParticle();
            gameFlow.AddPerformanceData(battleData.pMultipleDatas);
            battleData.currentHeal.RestValue(value);

            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.BeHealAfter, battleData);
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnHealAfter, battleData);
        }
    }

    public void CreateMonster(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        // 擊中特效and音效
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);
        // 有空位才產出怪物
        if (battleManager.monsters.Count < 3)
        {
            //產幾隻
            var p = new PSummonMonsterData();
            var monsters = new List<BattleActor>();
            for (int i = 0; i < abilityData.abilityArg2; i++)
            {
                // 確認位置上沒有怪物0~2
                for (int j = 0; j < 3; j++)
                {
                    var pos = (BattleActor.MonsterPositionEnum)j;
                    if (battleManager.monsters.Find(m => m.monsterPos == pos) == null)
                    {
                        var monster = monsterManager.GainMonsterActor(abilityData.abilityArg1);
                        monster.monsterPos = pos;
                        battleManager.monsters.Add(monster);
                        p.positions.Add(pos);
                        monsters.Add(monster);
                        break;
                    }
                }
            }
            if (p.positions.Count > 0)
            {
                gameFlow.AddPerformanceData(p);
                for (int i = 0; i < monsters.Count; i++)
                {
                    var m = monsters[i];
                    var mp = environmentManager.monsterPoints[(int)m.monsterPos];
                    environmentManager.SetMonsterPoint(mp, m, battleManager.monsters.IndexOf(m));
                    monsterManager.DoMonsterAI(m);
                }
            }
        }
    }

    public void InserSkill(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        // 擊中特效and音效
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);
        var count = abilityData.abilityArg2;
        var total = abilityData.abilityArg2 + battleData.currentTarget.skills.Count;
        var overCount = 0;
        if (total > 20)
        {
            overCount = total - 20;
            count = abilityData.abilityArg2 - overCount;
        }
        // 插入技能
        if (count > 0)
        {
            var p = new PPlayerSkillDataInsert();
            passiveManager.GetCurrentActorAttribute(battleData.currentTarget);
            var range = battleData.currentTarget.currentActorBaseAttribute.currentSkillRange;
            for (int i = 0; i < count; i++)
            {
                var newSkill = new ActorSkill();
                newSkill.skillId = abilityData.abilityArg1;
                newSkill.isUsed = false;
                newSkill.originIndex = -1;
                var r = UnityEngine.Random.Range(0, battleData.currentTarget.skills.Count);
                // 先移除第一個
                var first = battleData.currentTarget.skills[0];
                battleData.currentTarget.skills.Remove(first);
                // 放置最後一個
                battleData.currentTarget.skills.Add(first);
                // 再插入對應位置
                battleData.currentTarget.skills.Insert(r, newSkill);
                if (r >= range)
                    p.pushSkills.Add(battleData.currentTarget.skills[range - 1]);
                else
                    p.pushSkills.Add(null);
                p.skills.Add(newSkill);
                p.indexs.Add(r);
            }
            gameFlow.AddPerformanceData(p);
        }
        // 超出
        if (overCount > 0)
        {
            var newSkill = new ActorSkill();
            newSkill.skillId = abilityData.abilityArg1;
            newSkill.isUsed = false;
            newSkill.originIndex = -1;
            var overP = new PPlayerSkillDataInsert();
            overP.isOverFlow = true;
            for (int i = 0; i < overCount; i++)
            {
                overP.skills.Add(newSkill);
            }
            gameFlow.AddPerformanceData(overP);
        }
    }

    [Flags]
    enum DamageIgnoreEnum
    {
        None,
        Shield,
        Def
    }
    public void CrossDamage(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var target = battleData.currentTarget;
        var ignoreEnum = (DamageIgnoreEnum)abilityData.abilityArg2;
        var result = battleManager.OnDamage(target, abilityData.abilityArg1, ignoreEnum.HasFlag(DamageIgnoreEnum.Shield), ignoreEnum.HasFlag(DamageIgnoreEnum.Def));
        var pDmg = new POnDamageData();
        pDmg.Init(target, result);
        battleData.pMultipleDatas.performanceDatas.Add(pDmg);
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);
    }

    /// <summary>
    /// 計算第幾欄位的消耗數量
    /// </summary>
    /// <param name="battleData"></param>
    /// <param name="costRow"></param>
    /// <returns></returns>
    int GetCostRowCont(BattleData battleData, CostRowEnum costRow)
    {
        var total = 0;
        Action<CostRowEnum, int> func = (a, b) =>
        {
            if (costRow.HasFlag(a))
            {
                if (battleData.costs.Count > b)
                {
                    foreach (var item in battleData.costs[b])
                    {
                        total += item.Value;
                    }
                }
                else Debug.LogWarning($"CostCountRowMultipleDamage Index:{b} over battleData.costs.Count:{battleData.costs.Count}");
            }
        };

        func(CostRowEnum.One, 0);
        func(CostRowEnum.Two, 1);
        func(CostRowEnum.Three, 2);
        func(CostRowEnum.Four, 3);
        return total;
    }

    public void ModifyPassiveStackByCost(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var rows = (CostRowEnum)abilityData.abilityArg1;
        var total = GetCostRowCont(battleData, rows);
        var count = total * abilityData.abilityArg3 / 100;
        var mp = passiveManager.GainActorPassive(abilityData.abilityArg2, count);
        mp.owner = battleData.currentTarget;
        mp.sender = battleData.sender;
        battleData.modifyActorPassive = mp;
        passiveManager.AddPassive(battleData);

    }

    public void SelfPassiveStackHealOrDamage(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var passives = battleData.sender.passives.FindAll(p => dataTableManager.GetPassiveDefine(p.passiveId).groupId == abilityData.abilityArg1);
        var total = 0;
        for (int i = 0; i < passives.Count; i++)
        {
            total += passives[i].currentStack;
        }

        var amount = total * abilityData.abilityArg3;

        if (abilityData.abilityArg2 == 1)
        {
            battleData.currentDmg.RestValue(battleManager.GetDamageFromAttckPower(battleData.sender, amount));
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnAttackBefore, battleData);
            passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.BeAttackBefore, battleData);
            // 傷害前死亡 不執行額外技能
            passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.OnDamage, battleData);
            var result = battleManager.OnDamage(battleData.currentTarget, battleData.currentDmg.GetValue());
            var p = new POnDamageData();
            p.Init(battleData.currentTarget, result);
            battleData.currentDmg.RestValue(result.Item1);
            gameFlow.AddPerformanceData(p);
            // 擊中特效and音效
            passiveManager.OnActorPassive(battleData.currentTarget, PassiveTriggerEnum.BeAttackAfter, battleData);
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnAttackAfter, battleData);
        }
        else if (abilityData.abilityArg2 == 2)
        {
            battleData.currentHeal.RestValue(amount);
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnHealBefore, battleData);
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.BeHealBefore, battleData);
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnHeal, battleData);

            var value = battleManager.OnHeal(battleData.sender, battleData.currentHeal.GetValue());
            battleData.pMultipleDatas.AddPShowParticle();
            gameFlow.AddPerformanceData(battleData.pMultipleDatas);
            battleData.currentHeal.RestValue(value);

            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.BeHealAfter, battleData);
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnHealAfter, battleData);
        }
    }

    public void HealPercentage(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var target = battleData.currentTarget;
        passiveManager.GetCurrentActorAttribute(target);
        var maxHp = target.currentActorBaseAttribute.maxHp.GetValue();
        var heal = maxHp * abilityData.abilityArg1 / 100;
        battleData.currentHeal.RestValue(heal);
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnHealBefore, battleData);
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.BeHealBefore, battleData);
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnHeal, battleData);

        var value = battleManager.OnHeal(target, battleData.currentHeal.GetValue());
        var p = new POnHealData();
        p.Init(target, value);
        battleData.pMultipleDatas.performanceDatas.Add(p);
        battleData.pMultipleDatas.AddPShowParticle();
        gameFlow.AddPerformanceData(battleData.pMultipleDatas);
        battleData.currentHeal.RestValue(value);

        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.BeHealAfter, battleData);
        passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnHealAfter, battleData);
    }

    public void RemoveSkill(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var target = battleData.currentTarget;
        var p = new PPlayerSkillDataRemove();
        if (abilityData.abilityArg1 == -1)
        {
            p.indexs.Add(target.skills.IndexOf(battleData.originSkill));
            p.pushSkills.Add(target.skills[target.currentActorBaseAttribute.currentSkillRange]);
            target.skills.Remove(battleData.originSkill);
        }
        else
        {
            var removeSkillId = abilityData.abilityArg1;
            var define = dataTableManager.GetSkillDefine(removeSkillId);
            //只能移除怪物類型技能 
            if (define.profession != ActorProfessionEnum.Monster) return;
            for (int i = 0; i < abilityData.abilityArg2; i++)
            {
                var actorSkill = target.skills.Find(s => s.skillId == removeSkillId);
                if (actorSkill != null)
                {
                    p.indexs.Add(target.skills.IndexOf(actorSkill));
                    p.pushSkills.Add(target.skills[target.currentActorBaseAttribute.currentSkillRange]);
                    target.skills.Remove(actorSkill);
                }
            }
        }
        gameFlow.AddPerformanceData(p);
    }

    public void ModifyCoin(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var target = battleData.currentTarget;
        battleData.currentCoin = abilityData.abilityArg1;
        passiveManager.OnActorPassive(target, PassiveTriggerEnum.OnGetCoinBefore, battleData);
        //獲得金幣
        var oldCoin = target.coin;
        target.coin += battleData.currentCoin;
        if (target.coin < 0)
        {
            target.coin = 0;
            // 失去的金幣數量
            battleData.currentCoin = -oldCoin;
        }

        passiveManager.OnActorPassive(target, PassiveTriggerEnum.OnGetCoinAfter, battleData);
    }
    public void SwitchMonsterAI(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        if (battleData.sender.isPlayer) return;
        battleData.sender.behaviorData.aiId = abilityData.abilityArg1;
        battleData.sender.behaviorData.roundCount = 0;
    }
    #endregion

    #region 技能效果條件
    public bool ColorCost(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var totalCount = 0;
        for (int i = 0; i < battleData.costs.Count; i++)
        {
            var color = (SkillCostColorEnum)abilityData.conditionArg1;
            foreach (var item in battleData.costs[i])
            {
                if (color.HasFlag(item.Key))
                {
                    totalCount += item.Value;
                }
            }
        }
        bool check = false;
        switch (abilityData.conditionArg2)
        {
            case 1:
                check = totalCount > abilityData.conditionArg3;
                break;
            case 2:
                check = totalCount < abilityData.conditionArg3;
                break;
            case 3:
                check = totalCount == abilityData.conditionArg3;
                break;
            case 4:
                check = totalCount >= abilityData.conditionArg3;
                break;
            case 5:
                check = totalCount <= abilityData.conditionArg3;
                break;
        }
        return check;
    }

    public bool BuffStack(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var passives = battleData.currentTarget.passives.FindAll(p => dataTableManager.GetPassiveDefine(p.passiveId).groupId == abilityData.conditionArg1);
        var total = 0;
        for (int i = 0; i < passives.Count; i++)
        {
            total += passives[i].currentStack;
        }
        bool check = false;
        switch (abilityData.conditionArg2)
        {
            case 1:
                check = total > abilityData.conditionArg3;
                break;
            case 2:
                check = total < abilityData.conditionArg3;
                break;
            case 3:
                check = total == abilityData.conditionArg3;
                break;
            case 4:
                check = total >= abilityData.conditionArg3;
                break;
            case 5:
                check = total <= abilityData.conditionArg3;
                break;
        }
        return check;
    }

    public bool HpPercentage(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        passiveManager.GetCurrentActorAttribute(battleData.sender);
        var percentage = battleData.sender.currentHp * 100 / battleData.sender.currentActorBaseAttribute.maxHp.GetValue();
        switch (abilityData.conditionArg1)
        {
            //大於
            case 1:
                return percentage > abilityData.conditionArg2;
            //小於
            case 2:
                return percentage < abilityData.conditionArg2;
            //等於
            case 3:
                return percentage == abilityData.conditionArg2;
            //大於等於
            case 4:
                return percentage >= abilityData.conditionArg2;
            //小於等於
            case 5:
                return percentage <= abilityData.conditionArg2;
            default:
                return false;
        }
    }

    public bool FirstOrLast(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var haveSame = false;
        var value = 0;
        var targetValue = abilityData.conditionArg2 == 1 ? 0 : 999999999;
        BattleActor target = null;
        for (int i = 0; i < battleData.allEnemy.Count; i++)
        {
            passiveManager.GetCurrentActorAttribute(battleData.allEnemy[i]);
            var actor = battleData.allEnemy[i];
            switch (abilityData.conditionArg1)
            {
                case 1:
                    value = actor.currentHp;
                    break;
                case 2:
                    value = actor.currentHp * 100 / actor.currentActorBaseAttribute.maxHp.GetValue();
                    break;
                case 3:
                    value = actor.currentActorBaseAttribute.attackPower.GetValue();
                    break;
                default:
                    break;
            }
            if (abilityData.conditionArg2 == 1)
            {
                if (targetValue < value)
                {
                    target = actor;
                    targetValue = value;
                    haveSame = false;
                }
                else if (targetValue == value) haveSame = true;
            }
            else if (abilityData.conditionArg2 == 2)
            {
                if (targetValue > value)
                {
                    target = actor;
                    targetValue = value;
                    haveSame = false;
                }
                else if (targetValue == value) haveSame = true;
            }
        }
        if (haveSame) return false;
        return battleData.currentTarget == target;
    }

    public bool SelfPassiveStack(BattleData battleData, SkillAbilityDataDefine abilityData)
    {
        var passives = battleData.sender.passives.FindAll(p => dataTableManager.GetPassiveDefine(p.passiveId).groupId == abilityData.conditionArg1);
        var total = 0;
        for (int i = 0; i < passives.Count; i++)
        {
            total += passives[i].currentStack;
        }
        bool check = false;
        switch (abilityData.conditionArg2)
        {
            case 1:
                check = total > abilityData.conditionArg3;
                break;
            case 2:
                check = total < abilityData.conditionArg3;
                break;
            case 3:
                check = total == abilityData.conditionArg3;
                break;
            case 4:
                check = total >= abilityData.conditionArg3;
                break;
            case 5:
                check = total <= abilityData.conditionArg3;
                break;
        }
        return check;
    }
    #endregion
}
