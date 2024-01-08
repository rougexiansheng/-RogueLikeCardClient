using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

/// <summary>
/// 技能管理
/// </summary>
public class SkillManager : IInitializable
{
    [Inject]
    GameFlowController gameFlow;
    [Inject]
    PassiveManager passiveManager;
    [Inject]
    SkillAbilityMethods skillMethods;
    [Inject]
    BattleManager battleManager;
    [Inject]
    DataTableManager dataTableManager;

    Dictionary<SkillAbilityEnum, Action<BattleData, SkillAbilityDataDefine>> skillAbilityMethods = new Dictionary<SkillAbilityEnum, Action<BattleData, SkillAbilityDataDefine>>();
    Dictionary<SkillAbilityConditionEnum, Func<BattleData, SkillAbilityDataDefine, bool>> skillAbilityConditionMethods = new Dictionary<SkillAbilityConditionEnum, Func<BattleData, SkillAbilityDataDefine, bool>>();

    

    public void Initialize()
    {
        AddAbility(SkillAbilityEnum.None, (a, b) => { });
        AddAbility(SkillAbilityEnum.Damage, skillMethods.Damage);
        AddAbility(SkillAbilityEnum.GetColor, skillMethods.GetColor);
        AddAbility(SkillAbilityEnum.ActiveSkill, skillMethods.ActiveSkill);
        AddAbility(SkillAbilityEnum.AffrodPassive, skillMethods.AffrodPassive);
        AddAbility(SkillAbilityEnum.SkillMove, skillMethods.SkillMove);
        AddAbility(SkillAbilityEnum.KilledActiveSkill, skillMethods.KilledActiveSkill);
        AddAbility(SkillAbilityEnum.TargetBuffStackAffrodPassiveForTarget, skillMethods.TargetBuffStackAffrodPassiveForTarget);
        AddAbility(SkillAbilityEnum.TargetBuffStackAffrodPassiveForSelf, skillMethods.TargetBuffStackAffrodPassiveForSelf);
        AddAbility(SkillAbilityEnum.SelfBuffStackAffrodPassiveForTarget, skillMethods.SelfBuffStackAffrodPassiveForTarget);
        AddAbility(SkillAbilityEnum.BannedSkill, skillMethods.BannedSkill);
        AddAbility(SkillAbilityEnum.CostColorCountMultipleSkill, skillMethods.CostColorCountMultipleSkill);
        AddAbility(SkillAbilityEnum.CostCountRowMultipleSkill, skillMethods.CostCountRowMultipleSkill);
        AddAbility(SkillAbilityEnum.CostCountRowDamage, skillMethods.CostCountRowDamage);
        AddAbility(SkillAbilityEnum.Heal, skillMethods.Heal);
        AddAbility(SkillAbilityEnum.GetRandomColor, skillMethods.GetRandomColor);
        AddAbility(SkillAbilityEnum.ClearPassiveHeal, skillMethods.ClearPassiveHeal);
        AddAbility(SkillAbilityEnum.ClearPassiveGetColor, skillMethods.ClearPassiveGetColor);
        AddAbility(SkillAbilityEnum.ReduceHpActiveSkill, skillMethods.ReduceHpActiveSkill);
        AddAbility(SkillAbilityEnum.TargetPassiveStackHealOrDamage, skillMethods.TargetPassiveStackHealOrDamage);
        AddAbility(SkillAbilityEnum.ModifyShield, skillMethods.ModifyShield);
        AddAbility(SkillAbilityEnum.AttributeHealOrDamage, skillMethods.AttributeHealOrDamage);
        AddAbility(SkillAbilityEnum.CreateMonster, skillMethods.CreateMonster);
        AddAbility(SkillAbilityEnum.InserSkill, skillMethods.InserSkill);
        AddAbility(SkillAbilityEnum.CrossDamage, skillMethods.CrossDamage);
        AddAbility(SkillAbilityEnum.ModifyPassiveStackByCost, skillMethods.ModifyPassiveStackByCost);
        AddAbility(SkillAbilityEnum.SelfPassiveStackHealOrDamage, skillMethods.SelfPassiveStackHealOrDamage);
        AddAbility(SkillAbilityEnum.HealPercentage, skillMethods.HealPercentage);
        AddAbility(SkillAbilityEnum.RemoveSkill, skillMethods.RemoveSkill);
        AddAbility(SkillAbilityEnum.ModifyCoin, skillMethods.ModifyCoin);
        AddAbility(SkillAbilityEnum.SwitchMonsterAI, skillMethods.SwitchMonsterAI);


        AddCondtion(SkillAbilityConditionEnum.None, (a, b) => true);
        AddCondtion(SkillAbilityConditionEnum.ColorCost, skillMethods.ColorCost);
        AddCondtion(SkillAbilityConditionEnum.BuffStack, skillMethods.BuffStack);
        AddCondtion(SkillAbilityConditionEnum.HpPercentage, skillMethods.HpPercentage);
        AddCondtion(SkillAbilityConditionEnum.FirstOrLast, skillMethods.FirstOrLast);
        AddCondtion(SkillAbilityConditionEnum.SelfPassiveStack, skillMethods.SelfPassiveStack);
    }

    bool AddCondtion(SkillAbilityConditionEnum condtionEnum, Func<BattleData, SkillAbilityDataDefine, bool> func)
    {
        var have = skillAbilityConditionMethods.ContainsKey(condtionEnum);
        if (!have)
            skillAbilityConditionMethods.Add(condtionEnum, func);

        else
            Debug.LogWarning($"SkillAbilityConditionEnum:{condtionEnum} Repeated Addition");
        return have;
    }

    

    public void ReleasePrefabe()
    {

    }

    

    bool AddAbility(SkillAbilityEnum abilityEnum, Action<BattleData, SkillAbilityDataDefine> func)
    {
        var have = skillAbilityMethods.ContainsKey(abilityEnum);
        if (!have)
            skillAbilityMethods.Add(abilityEnum, func);

        else
            Debug.LogWarning($"SkillEffectEnum:{abilityEnum} Repeated Addition");
        return have;
    }

    int stackCount = 0;
    public void OnSkill(BattleData battleData, bool trigger = true)
    {
        stackCount++;
        if (stackCount > 1000)
        {
            Debug.LogError("技能 Loop 超過1000次");
            return;
        }
        var skillDefine = dataTableManager.GetSkillDefine(battleData.skillId);
        if (skillDefine == null)
        {
            Debug.LogError($"SkillManager OnSkill: cannot found skilldId {battleData.skillId} in table.");
            return;
        }
        if (battleData == null)
        {
            Debug.LogError($"SkillManager OnSkill: battle data is null");
            return;
        }
        if (battleData.selectTarget == null)
        {
            Debug.LogError($"SkillManager OnSkill: select target is null");
            return;
        }
        if (battleData.targets == null)
        {
            Debug.LogError($"SkillManager OnSkill: targets is null");
            return;
        }
        if (battleData.sender == null)
        {
            Debug.LogError($"SkillManager OnSkill: sender is null");
            return;
        }

        // 施法特效and音效
        if (trigger) passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnSkillBefore, battleData);
        bool haveTarget = SetSkillTargets(battleData); //檢查目標是否合法 自動找下一個 都沒有目標就強制跳出以下方法
        var startLog = $"技能:{skillDefine.id}-{skillDefine.skillName} 施法者:{battleData.sender.actorName} 選擇目標:{battleData.selectTarget.actorName} 表格目標:{string.Join(",", battleData.targets.ConvertAll(t => t.monsterId + "-" + t.actorName))} 施放開始";
        UtilityHelper.BattleLog($"OriginSkillId : {(battleData.originSkill == null? 0 : battleData.originSkill.skillId)}", UtilityHelper.BattleLogEnum.Skill);
        UtilityHelper.BattleLog(startLog, UtilityHelper.BattleLogEnum.Skill);
        if (!haveTarget)
        {
            var stopLog = $"技能:{skillDefine.id}-{skillDefine.skillName} 施法者:{battleData.sender.actorName} 選擇目標:{battleData.selectTarget.actorName} 沒有其他可選擇目標 技能終止";
            UtilityHelper.BattleLog(stopLog, UtilityHelper.BattleLogEnum.Skill);
            return;
        }

        if (skillDefine.isMoveCameraUp && !skillDefine.ignoreCameraMove)
        {
            gameFlow.AddPerformanceData(new PCameraMoveData() { isAll = skillDefine.isMoveCameraUp });
        }


        var costParticleData = new PShowParticleData();
        costParticleData.isPlayer = battleData.sender.isPlayer;
        costParticleData.monsterPosition = battleData.sender.monsterPos;
        costParticleData.particle = skillDefine.costEffect;
        costParticleData.position = skillDefine.costEffectPos;
        costParticleData.needWaitDestroy = true;
        costParticleData.sound = skillDefine.costEffectSound;
        gameFlow.AddPerformanceData(costParticleData);
        UtilityHelper.BattleLog($"Cost name : {skillDefine.costEffect?.name} Pos : {skillDefine.costEffectPos}", UtilityHelper.BattleLogEnum.SkillEffect);
        UtilityHelper.BattleLog($"Cost name : {skillDefine.costEffectSound?.name}", UtilityHelper.BattleLogEnum.SkillSound);

        for (int i = 0; i < skillDefine.skillAbilities.Count; i++)
        {
            var abilityData = skillDefine.skillAbilities[i];
            battleData.pMultipleDatas = new PMultipleData();
            for (int j = 0; j < battleData.targets.Count; j++)
            {
                battleData.currentTarget = battleData.targets[j];
                if (skillDefine.isMoveCameraUp == false && !battleData.currentTarget.isPlayer && !skillDefine.ignoreCameraMove)
                {
                    gameFlow.AddPerformanceData(new PCameraMoveData() { isAll = false, monsterPosition = battleData.currentTarget.monsterPos });
                }
                if (skillAbilityConditionMethods.TryGetValue(abilityData.abilityCondition, out Func<BattleData, SkillAbilityDataDefine, bool> conditionFunc))
                {
                    var check = conditionFunc(battleData, abilityData);
                    var have = skillAbilityMethods.TryGetValue(abilityData.skillAbility, out Action<BattleData, SkillAbilityDataDefine> abilityFunc);
                    if (check && have)
                    {
                        UtilityHelper.BattleLog($"Hit name : {abilityData.hitEffect?.name} Pos : {abilityData.hitEffectPos}", UtilityHelper.BattleLogEnum.SkillEffect);
                        UtilityHelper.BattleLog("Hit name : " + abilityData.hitEffectSound?.name, UtilityHelper.BattleLogEnum.SkillSound);
                        var hitParticleData = new PShowParticleData();
                        hitParticleData.Init(abilityData, battleData.currentTarget);
                        battleData.pMultipleDatas.hitParticleData = hitParticleData;
                        abilityFunc(battleData, abilityData);
                    }
                    else
                    {
                        Debug.LogWarning($"Skill ability id:{battleData.skillId} not work condition:{abilityData.abilityCondition} check is:{check} abitity:{abilityData.skillAbility} have is:{have}");
                    }
                }
                else
                    Debug.LogWarning($"Skill ability id:{battleData.skillId} dont have:{abilityData.abilityCondition}");
            }

        }
        if (trigger)
        {
            passiveManager.OnActorPassive(battleData.sender, PassiveTriggerEnum.OnSkillAfter, battleData);
            gameFlow.AddPerformanceData(new PCameraMoveData() { isAll = false, monsterPosition = BattleActor.MonsterPositionEnum.None });
        }
        var endLog = $"技能:{skillDefine.id}-{skillDefine.skillName} 施法者:{battleData.sender.actorName} 目標:{battleData.selectTarget.actorName} 施放結束";
        UtilityHelper.BattleLog(endLog, UtilityHelper.BattleLogEnum.Skill);
        stackCount = 0;
    }

    public bool SetSkillTargets(BattleData battleData)
    {
        var skillDefine = dataTableManager.GetSkillDefine(battleData.skillId);
        if (skillDefine == null)
        {
            Debug.LogError($"SkillManager SetSkillTargets: cannot found skilldId {battleData.skillId} in table.");
            return false;
        }
        if (battleData == null)
        {
            Debug.LogError($"SkillManager SetSkillTargets: battle data is null");
            return false;
        }
        if (battleData.selectTarget == null)
        {
            Debug.LogError($"SkillManager SetSkillTargets: select target is null");
            return false;
        }

        battleManager.SetAllEnemyAndPartner(battleData);
        battleData.targets.Clear();
        // 除了自身目標 其他都要檢查是否選擇目標是否死亡 死亡要換下一個目標
        if (SkillTargetEnum.Self != skillDefine.targetEnum && battleData.selectTarget.isDead)
        {
            for (int i = 0; i < battleData.allEnemy.Count; i++)
            {
                var target = battleData.allEnemy[i];
                if (!target.isDead)
                {
                    battleData.selectTarget = target;
                    break;
                }
            }
            if (battleData.selectTarget.isDead)
            {
                return false;
            }
        }

        switch (skillDefine.targetEnum)
        {
            case SkillTargetEnum.SelectTarget:
                battleData.targets.Add(battleData.selectTarget);
                break;
            case SkillTargetEnum.AllEnemy:
                battleData.targets.AddRange(battleData.allEnemy.FindAll(actor => actor.isDead == false));
                break;
            case SkillTargetEnum.Self:
                battleData.targets.Add(battleData.sender);
                break;
            case SkillTargetEnum.RandomNotRepeat:
            case SkillTargetEnum.ExcludeSelectRandom:
                var ls = new List<BattleActor>(battleData.allEnemy);
                while (ls.Count > 0 && battleData.targets.Count != skillDefine.targetAmount)
                {
                    var r = UnityEngine.Random.Range(0, ls.Count);
                    var actor = ls[r];
                    ls.Remove(actor);
                    if (skillDefine.targetEnum == SkillTargetEnum.ExcludeSelectRandom && actor == battleData.selectTarget)
                    {
                        continue;
                    }
                    battleData.targets.Add(actor);
                }
                break;
            case SkillTargetEnum.AllExcludeSelect:
                for (int i = 0; i < battleData.allEnemy.Count; i++)
                {
                    var actor = battleData.allEnemy[i];
                    if (actor != battleData.selectTarget) battleData.targets.Add(actor);
                }
                break;
            case SkillTargetEnum.Random:
                for (int i = 0; i < skillDefine.targetAmount; i++)
                {
                    var r = UnityEngine.Random.Range(0, battleData.allEnemy.Count);
                    var actor = battleData.allEnemy[r];
                    battleData.targets.Add(actor);
                }
                break;
            case SkillTargetEnum.AllPartner:
                battleData.targets.AddRange(battleData.allPartner.FindAll(actor => actor.isDead == false));
                break;
            case SkillTargetEnum.RandomPartnerNotRepeatExcludeSelf:
            case SkillTargetEnum.RandomPartnerNotRepeat:
                var pls = new List<BattleActor>(battleData.allPartner);
                while (pls.Count > 0 && battleData.targets.Count != skillDefine.targetAmount)
                {
                    var r = UnityEngine.Random.Range(0, pls.Count);
                    var actor = pls[r];
                    pls.Remove(actor);
                    if (skillDefine.targetEnum == SkillTargetEnum.RandomPartnerNotRepeatExcludeSelf && actor == battleData.sender)
                    {
                        continue;
                    }
                    battleData.targets.Add(actor);
                }
                break;
            case SkillTargetEnum.RandomPartner:
                for (int i = 0; i < skillDefine.targetAmount; i++)
                {
                    var r = UnityEngine.Random.Range(0, battleData.allPartner.Count);
                    var actor = battleData.allEnemy[r];
                    battleData.targets.Add(actor);
                }
                break;
            case SkillTargetEnum.None:
            default:
                return false;
        }
        return battleData.targets.Count > 0;
    }


    /// <summary>
    /// 判斷Tag有哪幾類的顏色
    /// </summary>
    /// <param name="skillCostColorEnum"></param>
    /// <returns></returns>
    public List<SkillCostColorEnum> GetColorsList(SkillCostColorEnum skillCostColorEnum)
    {
        var lsEnum = Enum.GetValues(typeof(SkillCostColorEnum)).Cast<SkillCostColorEnum>().ToList();
        var ls = new List<SkillCostColorEnum>();
        for (int i = 0; i < lsEnum.Count; i++)
        {
            if (lsEnum[i] != SkillCostColorEnum.None && skillCostColorEnum.HasFlag(lsEnum[i]))
            {
                ls.Add(lsEnum[i]);
            }
        }
        return ls;
    }

    /// <summary>
    /// 取得更改Skill的狀態
    /// </summary>
    /// <param name="sourceID"></param>
    /// <param name="targetID"></param>
    /// <returns></returns>
    public SkillChangeStateEnum GetSkillChangeState(int sourceID, int targetID)
    {
        var sourceData = dataTableManager.GetSkillDefine(sourceID);
        var targetData = dataTableManager.GetSkillDefine(targetID);
        if (sourceData.groupId == targetData.groupId)
        {
            return SkillChangeStateEnum.LevelUp;
        }

        return SkillChangeStateEnum.Replace;

    }

    /// <summary>
    /// 指定技能升級，如果已經是最高等返回-1。
    /// </summary>
    /// <param name="sourceID"></param>
    /// <returns></returns>
    public int GetLeveUpID(int sourceID)
    {
        var sourceData = dataTableManager.GetSkillDefine(sourceID);
        var skillGroupDataList = dataTableManager.GetSkillGroupDefine(sourceData.groupId);

        int sourceIndex = skillGroupDataList.FindIndex(x => x.id == sourceData.id);
        if (skillGroupDataList.Count == sourceIndex + 1)
        {
            Debug.LogWarning(sourceID + "(ID) Skill is highest level");
            return -1;
        }

        return skillGroupDataList[sourceIndex + 1].id;
    }
}