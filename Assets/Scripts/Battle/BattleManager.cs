using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BattleManager : IInitializable
{
    [Inject]
    GameFlowController gameFlow;
    [Inject]
    DataManager dataManager;
    [Inject]
    PassiveManager passiveManager;
    [Inject]
    SkillManager skillManager;
    [Inject]
    MonsterManager monsterManager;
    [Inject]
    DataTableManager dataTableManager;
    [Inject]
    PreloadManager preloadManager;
    [Inject]
    NetworkSaveManager saveManager;
    public List<BattleActor> monsters = new List<BattleActor>();
    public BattleActor player;
    public BattleActor.MonsterPositionEnum selectTargetEnum;

    private bool m_isTimerActived = false;
    private float m_elapsedTimer;
    private float m_totalElapsedTime;

    public void Initialize()
    {
        RxEventBus.Register<BattleActor.MonsterPositionEnum>(EventBusEnum.PlayerDataEnum.UpdateSelectTarget, (e) => selectTargetEnum = e, this);
    }

    public void ResetPlayerActor()
    {
        var doungeonData = dataManager.GetCurrentDungeonLeveData();
        // 重製技能組(排序不變)
        player.skills = saveManager.GetContainer<NetworkSaveBattleSkillContainer>().GetSortedActorSkillList();
        List<ActorPassive> actorPassives = new List<ActorPassive>();
        var passiveIDs = saveManager.GetContainer<NetworkSaveBattleHeroAttrContainer>().GetAttrs(NetworkSaveBattleHeroAttrContainer.AttrType.Passive);

        foreach (var id in passiveIDs)
        {
            var passive = passiveManager.GainActorPassive(id, 1);
            passive.owner = player;
            passive.sender = player;
            actorPassives.Add(passive);
        }
        player.passives = actorPassives;

        // 移除身上非遺物的被動
        var ls = new List<ActorPassive>();
        for (int i = 0; i < player.passives.Count; i++)
        {
            var p = player.passives[i];
            var define = dataTableManager.GetPassiveDefine(p.passiveId);
            if (define.passiveType != PassiveTypeEnum.Antique)
            {
                ls.Add(p);
            }
        }
        for (int i = 0; i < ls.Count; i++)
        {
            var p = new PPassiveData();
            p.Init(player);
            p.isRemove = true;
            p.passiveId = ls[i].passiveId;
            p.stackCount = ls[i].currentStack;
            gameFlow.AddPerformanceData(p);
            player.passives.Remove(ls[i]);
        }
        // 放入環境被動
        for (int i = 0; i < doungeonData.passives.Count; i++)
        {
            var passive = passiveManager.GainActorPassive(doungeonData.passives[i], 1);
            passive.owner = player;
            passive.sender = player;
            player.passives.Add(passive);
        }
        player.colors.Clear();
    }

    public async UniTask SetMonsterActor(List<int> posAndId, List<List<SDKProtocol.ItemData>> acquisitionItems)
    {
        monsters.Clear();
        if (posAndId == null)
            return;
        var tLs = new List<UniTask>();
        for (int i = 0; i < posAndId.Count; i++)
        {
            if (posAndId[i] == 0) continue;
            tLs.Add(preloadManager.PreloadMonsterSpineAndParticle(posAndId[i]));
            var monster = monsterManager.GainMonsterActor(posAndId[i]);
            monster.monsterPos = (BattleActor.MonsterPositionEnum)i;
            monster.acquisitionList = acquisitionItems[i];
            monsters.Add(monster);
        }
        await UniTask.WhenAll(tLs);
    }

    public void KillAllMonster()
    {
        monsters.ForEach(x => x.isDead = true);
    }

    public (bool isEnd, bool isWin) GameIsEndAndIsWin()
    {
        var ls = monsters.FindAll(actor => actor.isDead == true);
        if (ls != null && ls.Count > 0)
        {
            PMonsterRemoveData mRemoveData = new PMonsterRemoveData();
            for (int i = 0; i < ls.Count; i++)
            {
                var m = ls[i];
                passiveManager.OnActorPassive(m, PassiveTriggerEnum.OnDeadAndRemoveBefore);
                //已死亡 但尚未移出
                if (m.isDead)
                {
                    //確定死亡並移出
                    monsters.Remove(m);
                    mRemoveData.monsterID = m.monsterId;
                    mRemoveData.acquisitionList = m.acquisitionList;
                    mRemoveData.positions.Add(ls[i].monsterPos);
                    passiveManager.OnActorPassive(m, PassiveTriggerEnum.OnDeadAndRemoveAfter);
                }
            }
            gameFlow.AddPerformanceData(mRemoveData);
        }
        bool isWin, isEnd;
        isEnd = monsters.Count == 0 || player.isDead;
        isWin = monsters.Count == 0;
        return (isEnd, isWin);
    }

    public void SetAllEnemyAndPartner(BattleData battleData)
    {
        battleData.allEnemy.Clear();
        battleData.allPartner.Clear();
        if (battleData.sender.isPlayer)
        {
            battleData.allEnemy.AddRange(new List<BattleActor>(monsters));
            battleData.allPartner.Add(player);
        }
        else
        {
            battleData.allPartner.AddRange(new List<BattleActor>(monsters));
            battleData.allEnemy.Add(player);
        }
    }

    public void PlayerPushSkills(int count, BattleActor player)
    {
        if (!player.isPlayer)
        {
            Debug.LogWarning($"PlayerPushSkills {player.actorName} is not player");
            return;
        }
        // UI技能 前推/後退
        var d = new PPlayerSkillDataMove();
        d.isPush = count > 0;
        var moveCount = Mathf.Abs(count);
        var range = player.currentActorBaseAttribute.currentSkillRange;
        for (int i = 0; i < moveCount; i++)
        {
            if (player.skills.Count == 0)
                continue;

            if (count > 0)
            {
                var skill = player.skills[0];
                // 加入要移入進去的 技能
                d.pushSkills.Add(player.skills[range]);

                player.skills.Remove(skill);
                player.skills.Add(skill);

            }
            else
            {
                var skill = player.skills[player.skills.Count - 1];
                // 加入要移入進去的 技能
                d.pushSkills.Add(skill);

                player.skills.Remove(skill);
                player.skills.Insert(0, skill);
            }
        }
        gameFlow.AddPerformanceData(d);
    }

    public bool CostColor(BattleData battleData)
    {
        var define = dataTableManager.GetSkillDefine(battleData.skillId);
        if (!battleData.sender.isPlayer) return false;
        var p = new PModifyColorData();
        for (int i = 0; i < define.costColors.Count; i++)
        {
            var colors = skillManager.GetColorsList(define.costColors[i].colorEnum);
            for (int j = 0; j < colors.Count; j++)
            {
                var color = colors[j];
                var dic = new Dictionary<SkillCostColorEnum, int>();
                var have = player.colors.TryGetValue(color, out int value);
                if (have) player.colors.Remove(color);
                if (define.costColors[i].count == -1)
                {
                    p.SetColorEffectEnum(color, PModifyColorData.PerformanceColorEffectEnum.Usage);
                    dic.Add(color, value);
                }
                else
                {
                    p.SetColorEffectEnum(color, PModifyColorData.PerformanceColorEffectEnum.Usage);

                    if (value >= define.costColors[i].count)
                    {
                        value -= define.costColors[i].count;
                        if (value > 0) player.colors.Add(color, value);
                        dic.Add(color, define.costColors[i].count);
                    }
                    else return false;
                }
                battleData.costs.Add(dic);
            }
        }
        p.Init(battleData.sender);
        gameFlow.AddPerformanceData(p);
        return true;
    }

    /// <summary>
    /// 從球池取得
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="count"></param>
    public void GetColors(BattleActor actor, int count)
    {
        var p = new PModifyColorData();
        for (int i = 0; i < count; i++)
        {
            var max = actor.colorPool.colorPools.Count;
            if (max == 0) RestActorPool(actor);
            var r = UnityEngine.Random.Range(0, max);
            var color = (r < actor.colorPool.colorPools.Count) ?
                actor.colorPool.colorPools[r] :
                SkillCostColorEnum.None;
            if (r < actor.colorPool.colorPools.Count)
                actor.colorPool.colorPools.RemoveAt(r);
            switch (color)
            {
                case SkillCostColorEnum.Red:
                    actor.colorPool.red -= 1;
                    break;
                case SkillCostColorEnum.Green:
                    actor.colorPool.green -= 1;
                    break;
                case SkillCostColorEnum.Blue:
                    actor.colorPool.blue -= 1;
                    break;
                case SkillCostColorEnum.None:
                default:
                    break;
            }
            p.SetColorEffectEnum(color, 1);
            AddColor(actor, color, 1);
        }
        p.Init(actor);
        gameFlow.AddPerformanceData(p);
    }

    void RestActorPool(BattleActor actor)
    {
        for (int i = 0; i < actor.skills.Count; i++)
        {
            var define = dataTableManager.GetSkillDefine(actor.skills[i].skillId);
            for (int j = 0; j < define.poolColor.Count; j++)
            {
                var color = define.poolColor[j];
                actor.colorPool.colorPools.Add(color);
                switch (color)
                {
                    case SkillCostColorEnum.None:
                        break;
                    case SkillCostColorEnum.Red:
                        actor.colorPool.red++;
                        break;
                    case SkillCostColorEnum.Green:
                        actor.colorPool.green++;
                        break;
                    case SkillCostColorEnum.Blue:
                        actor.colorPool.blue++;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 從外來直接給予玩家 不影響原來的球池
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="color"></param>
    /// <param name="count"></param>
    public void AddColor(BattleActor actor, SkillCostColorEnum color, int count)
    {
        var have = actor.colors.TryGetValue(color, out int value);
        if (have) actor.colors.Remove(color);
        value += count;
        actor.colors.Add(color, value);
    }

    /// <summary>
    /// 判斷是否可以使用此技能
    /// </summary>
    /// <param name="skillId"></param>
    /// <returns></returns>
    public bool PlayerCheckSkillCost(int idx)
    {
        var skillData = player.skills[idx];
        var skillDefine = dataTableManager.GetSkillDefine(skillData.skillId);
        for (int i = 0; i < skillDefine.costColors.Count; i++)
        {
            var costDefine = skillDefine.costColors[i];
            var colorLs = skillManager.GetColorsList(costDefine.colorEnum);
            var isEnough = true;
            for (int j = 0; j < colorLs.Count; j++)
            {
                var color = colorLs[j];
                var total = 0;
                player.colors.TryGetValue(color, out int count);
                total += count;
                if (total >= costDefine.count)
                {
                    isEnough = true;
                    break;
                }
                else isEnough = false;
            }
            if (!isEnough) return false;
        }
        return true;
    }

    /// <summary>
    /// 取得當前攻擊力並加上傷害
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="dmg"></param>
    /// <returns></returns>
    public int GetDamageFromAttckPower(BattleActor actor, int dmg)
    {
        passiveManager.GetCurrentActorAttribute(actor);
        var attr = actor.currentActorBaseAttribute;
        dmg = attr.attackPower.GetValue() + dmg;
        return dmg;
    }

    /// <summary>
    /// 被攻擊扣血返回實際扣血量 
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="dmg"></param>
    /// <param name="ignoreShield">是否忽略護盾</param>
    /// <returns></returns>
    public (int, bool) OnDamage(BattleActor actor, int dmg, bool ignoreShield = false, bool ignoreDef = false)
    {
        if (dmg <= 0) return (0, false);
        passiveManager.GetCurrentActorAttribute(actor);
        UtilityHelper.BattleLog($"Name:{actor.actorName} 受到傷害:{dmg} 盾/防:{ignoreShield}/{ignoreDef}", UtilityHelper.BattleLogEnum.OnDamage);
        var isBlock = false;
        var p = new PModifyShieldData();
        p.isPlayer = actor.isPlayer;
        p.monsterPosition = actor.monsterPos;
        p.beforeValue = actor.shield;
        var attr = actor.currentActorBaseAttribute;
        if (!ignoreDef)
        {
            dmg = dmg - attr.defense.GetValue();
        }
        if (!ignoreShield && actor.shield > 0)
        {
            var oldDmg = dmg;
            var oldShield = actor.shield;
            dmg = dmg - actor.shield;
            if (oldDmg >= actor.shield) actor.shield = 0;
            else actor.shield -= oldDmg;
            // 是否有成功擋住傷害 傷害低於0 護盾數值 有漸少
            isBlock = dmg <= 0 && oldShield > actor.shield;
            // 計算後扣除的盾牌
        }
        dmg = dmg < 0 ? 0 : dmg;
        actor.currentHp -= dmg;
        if (actor.currentHp <= 0)
        {
            actor.currentHp = 0;
            actor.isDead = true;
        }
        p.shieldValue = actor.shield;
        gameFlow.AddPerformanceData(p);
        UtilityHelper.BattleLog($"Name:{actor.actorName} 實際受到傷害:{dmg} 盾/防:{ignoreShield}/{ignoreDef}", UtilityHelper.BattleLogEnum.OnDamage);
        return (dmg, isBlock);
    }

    /// <summary>
    /// 治療並返回實際治療量
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="heal"></param>
    /// <returns></returns>
    public int OnHeal(BattleActor actor, int heal)
    {
        passiveManager.GetCurrentActorAttribute(actor);
        UtilityHelper.BattleLog($"Name:{actor.actorName} 受到治療:{heal}", UtilityHelper.BattleLogEnum.OnHeal);
        var attr = actor.currentActorBaseAttribute;
        actor.currentHp += heal;
        var maxHp = attr.maxHp.GetValue();
        if (actor.currentHp > maxHp)
        {
            heal = heal - (actor.currentHp - maxHp);
            actor.currentHp = maxHp;
        }

        UtilityHelper.BattleLog($"Name:{actor.actorName} 實際治療:{heal}", UtilityHelper.BattleLogEnum.OnHeal);

        return heal;
    }

    /// <summary>
    /// 對指定 baseSkills(BattleActor) List 中替換特定Index中的技能
    /// </summary>
    /// <param name="baseSkills"></param>
    /// <param name="index"></param>
    /// <param name="changeID"></param>
    public void UpdateSkill(List<int> baseSkills, int index, int changeID)
    {
        if (index > baseSkills.Count)
        {
            Debug.LogWarning("Index  out of target list");
            return;
        }
        baseSkills[index] = changeID;
    }

    /// <summary>
    /// 透過營火休息恢復血量
    /// </summary>
    public void RestStateHeal()
    {
        // 企劃大大Dunois(迪諾亞) 2023/12/04 14:14 表示: 治療量寫死 回復最大血量的20 %
        if (player != null)
        {
            var heal = (int)(player.maxHp * .2f);
            OnHeal(player, heal);
        }
    }

    public void StartTimer()
    {
        Debug.Log("開始計時");
        m_elapsedTimer = Time.fixedUnscaledTime;
        m_isTimerActived = true;
    }

    public void StopTimer()
    {
        m_isTimerActived = false;
        if (m_elapsedTimer != 0)
        {
            m_totalElapsedTime += Time.fixedUnscaledTime - m_elapsedTimer;
            m_elapsedTimer = 0;
        }
        Debug.Log($"停止計時, 目前總經過時間: {m_totalElapsedTime}");
    }

    public void ResetTiemr()
    {
        Debug.Log("清除計時");
        m_isTimerActived = false;
        m_totalElapsedTime = 0;
        m_elapsedTimer = 0;
    }

    public bool IsTimerActived()
    {
        return m_isTimerActived;
    }

    public float GetElapsedTime()
    {
        return m_totalElapsedTime;
    }
}
