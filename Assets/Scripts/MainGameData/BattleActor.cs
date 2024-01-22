using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BattleActor
{
    public enum MonsterPositionEnum
    {
        None = -1,
        Left,
        Center,
        Right
    }
    /// <summary>是否為玩家</summary>
    public bool isPlayer;
    /// <summary>是否死亡</summary>
    public bool isDead;
    /// <summary>可否攻擊(怪物/玩家 回合階段前 一定是true)</summary>
    public bool canAttack = false;
    public string actorName;
    public int currentHp;
    public int maxHp;
    public int defense;
    public int attackPower;
    public int coin;
    /// <summary>護盾值(玩家在玩家回合開始前清除/怪物在怪物回合開始前清除)</summary>
    public int shield;
    public BattleData lastBattleData;
    public int getColorCount;
    /// <summary>身上狀態</summary>
    public List<ActorPassive> passives = new List<ActorPassive>();
    /// <summary>取裡面數值前 請先執行PassiveManager.GetCurrentActorAttribute</summary>
    public BattleActorBaseAttribute currentActorBaseAttribute = new BattleActorBaseAttribute();
    #region 只有怪物使用
    [Header("Monster")]
    /// <summary>怪物id</summary>
    public int monsterId;
    /// <summary>怪物下次攻擊技能</summary>
    public int monsterNextSkill;
    public MonsterAIbehaviorData behaviorData;
    public MonsterPositionEnum monsterPos;
    public List<SDKProtocol.ItemData> acquisitionList;
    #endregion
    #region 只有玩家使用
    [Header("Player")]
    /// <summary>玩家戰鬥時技能</summary>
    public List<ActorSkill> skills = new List<ActorSkill>();
    /// <summary>玩家基礎技能</summary>
    [Obsolete]
    public List<int> baseSkills = new List<int>();
    /// <summary>玩家可以使用技能基礎範圍(currentActorBaseAttribute 取得最準確的數值)</summary>
    public int skillRange;
    /// <summary>玩家技能移動次數</summary>
    public int skillMoveCount;
    /// <summary>已獲得顏色</summary>
    public Dictionary<SkillCostColorEnum, int> colors = new Dictionary<SkillCostColorEnum, int>();
    /// <summary>基礎池子共有多少顏色</summary>
    public ColorPoolData colorPool = new ColorPoolData();
    #endregion
}

/// <summary>
/// 顏色資源池的資料
/// </summary>
public class ColorPoolData
{
    public int red;
    public int blue;
    public int green;
    public List<SkillCostColorEnum> colorPools = new List<SkillCostColorEnum>();
}
