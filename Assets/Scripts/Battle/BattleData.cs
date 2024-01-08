using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BattleData
{
    public List<ActorPassive> doingPassives = new List<ActorPassive>();
    /// <summary>當前所有敵人</summary>
    public List<BattleActor> allEnemy = new List<BattleActor>();
    /// <summary>所有友軍</summary>
    public List<BattleActor> allPartner = new List<BattleActor>();
    /// <summary>當前觸發的被動</summary>
    public ActorPassive actorPassive;
    /// <summary>新增/移除 被動</summary>
    public ActorPassive modifyActorPassive;
    /// <summary>當前使用的技能ID</summary>
    public int skillId;
    /// <summary>當前使用的技能ID</summary>
    public ActorSkill originSkill;
    /// <summary>施放此技能的花費</summary>
    public List<Dictionary<SkillCostColorEnum, int>> costs = new List<Dictionary<SkillCostColorEnum, int>>();
    /// <summary>當前技能選擇的目標</summary>
    public BattleActor selectTarget;
    /// <summary>當前使用的技能施法者</summary>
    public BattleActor sender;
    /// <summary>技能可影響的目標</summary>
    public List<BattleActor> targets = new List<BattleActor>();
    /// <summary>當前技能影響的目標</summary>
    public BattleActor currentTarget;
    public bool isSkill = true;
    public BattleFormulaValue currentDmg = new BattleFormulaValue();
    public BattleFormulaValue currentHeal = new BattleFormulaValue();
    public BattleFormulaValue currentShield = new BattleFormulaValue();
    public BattleFormulaValue currentColorCount = new BattleFormulaValue();
    public int currentMoveCount = 0;
    /// <summary>當前獲得金幣</summary>
    public int currentCoin;
    public List<Dictionary<SkillCostColorEnum, int>> getColors = new List<Dictionary<SkillCostColorEnum, int>>();
    /// <summary>表演資料</summary>
    public PMultipleData pMultipleDatas;
    public BattleData Clone()
    {
        var newBattleData = new BattleData();
        newBattleData.doingPassives = doingPassives;
        newBattleData.actorPassive = actorPassive?.Clone();
        newBattleData.modifyActorPassive = modifyActorPassive?.Clone();
        newBattleData.allEnemy = allEnemy;
        newBattleData.targets = new List<BattleActor>(targets);
        newBattleData.costs = costs;
        newBattleData.currentDmg = new BattleFormulaValue();
        newBattleData.currentTarget = currentTarget;
        newBattleData.currentHeal = new BattleFormulaValue();
        newBattleData.skillId = skillId;
        newBattleData.currentShield = new BattleFormulaValue();
        newBattleData.selectTarget = selectTarget;
        newBattleData.currentColorCount = new BattleFormulaValue();
        newBattleData.sender = sender;
        newBattleData.isSkill = isSkill;
        newBattleData.originSkill = originSkill;
        newBattleData.pMultipleDatas = pMultipleDatas;
        return newBattleData;
    }
}

[Serializable]
public class BattleActorBaseAttribute
{
    public bool isDone = true;
    public BattleFormulaValue maxHp = new BattleFormulaValue();
    public BattleFormulaValue defense = new BattleFormulaValue();
    public BattleFormulaValue attackPower = new BattleFormulaValue();
    /// <summary>玩家當前移動數</summary>
    public int currentMove;
    /// <summary>玩家當前得到能量數</summary>
    public int currentColorCount;
    public int currentSkillRange;
}



/// <summary>
/// 戰鬥計算 (Atk/Def/MaxHp/Heal/Damage/Sheild/ColorCount)
/// </summary>
public class BattleFormulaValue
{
    /// <summary>基礎值</summary>
    public int baseValue;
    /// <summary>基礎增加值</summary>
    public List<int> baseAdd = new List<int>();
    /// <summary>固定值</summary>
    public List<int> staticValue = new List<int>();
    /// <summary>百分比加值</summary>
    public List<int> percentageAdd = new List<int>();
    /// <summary>百分比乘值</summary>
    public List<int> percentageMultiplication = new List<int>();
    /// <summary>
    /// 公式:(baseValue+baseAdd1+baseAdd2...)*
    /// (100+(percentageAdd1+percentageAdd2...)*
    /// (percentageMultiplication1*percentageMultiplication2...))
    /// +staticValue
    /// </summary>
    /// <param name="autoRest">是否清除</param>
    /// <returns></returns>
    public int GetValue()
    {
        var total = baseValue;
        for (int i = 0; i < baseAdd.Count; i++)
        {
            baseValue += baseAdd[i];
        }

        // 100%
        var pAdd = 0;
        for (int i = 0; i < percentageAdd.Count; i++)
        {
            pAdd += percentageAdd[i];
        }

        // 1
        var pMul = 1f;
        for (int i = 0; i < percentageMultiplication.Count; i++)
        {
            pMul *= percentageMultiplication[i] / 100f;
        }

        total = total * (100 + pAdd) / 100;

        total = (int)(total * pMul);

        for (int i = 0; i < staticValue.Count; i++)
        {
            total += staticValue[i];
        }
        return total;
    }

    public void RestValue(int baseValue = 0)
    {
        this.baseValue = baseValue;
        baseAdd.Clear();
        staticValue.Clear();
        percentageAdd.Clear();
        percentageMultiplication.Clear();
    }
}
