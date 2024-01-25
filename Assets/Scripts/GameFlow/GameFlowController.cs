using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Linq;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine.UI;

public class GameFlowController : BaseControl<GameFlowController, GameFlowController.GameFlowState>, IInitializable
{
    [Inject]
    FakeServer sdk;
    [Inject]
    EnvironmentManager environmentManager;
    [Inject]
    AssetManager assetManager;
    [Inject]
    MonsterManager monsterManager;
    [Inject]
    SkillManager skillManager;
    [Inject]
    PassiveManager passiveManager;
    [Inject]
    DiContainer diContainer;
    [Inject]
    BattleManager battleManager;
    [Inject]
    DataManager dataManager;
    [Inject]
    UIManager uIManager;
    [Inject]
    PerformanceMethods performanceMethods;
    Dictionary<string, List<Func<PerformanceData, UniTask>>> performanceFuncs = new Dictionary<string, List<Func<PerformanceData, UniTask>>>();
    Queue<PerformanceData> performanceDatas = new Queue<PerformanceData>();
    public override Action<string> OnMessage => s => Debug.Log(string.Format("<color=#006b82>{0}</color>", s));
    bool doingPerformance = true;
    bool isWinCache = false;
    public enum GameFlowState
    {
        /// <summary>初始化</summary>
        Init,
        /// <summary>空狀態</summary>
        Empty,
        /// <summary>寶箱</summary>
        Chest,
        /// <summary>獎勵三選一(回血/技能/金幣)</summary>
        ChoiceReward,
        /// <summary>完結</summary>
        EndGame,
        /// <summary>取得遺物</summary>
        GetAntique,
        /// <summary>怪物行動回合</summary>
        MonsterAction,
        /// <summary>怪物回合</summary>
        MonsterRound,
        /// <summary>下一關卡(生成地形/燈光環境設置)</summary>
        NextLevel,
        /// <summary>玩家行動回合</summary>
        PlayerAction,
        /// <summary>玩家回合</summary>
        PlayerRound,
        /// <summary>結算</summary>
        Profit,
        /// <summary>休憩</summary>
        Rest,
        /// <summary>購買</summary>
        Store,
        /// <summary>開始戰鬥</summary>
        StartFight
    }

    public void OnMonsterPassive(List<BattleActor> monsters, PassiveTriggerEnum triggerEnum)
    {
        for (int i = 0; i < monsters.Count; i++)
        {
            passiveManager.OnActorPassive(monsters[i], triggerEnum);
        }
    }

    public void Initialize()
    {
        SetTransition(GameFlowState.Empty, SetInjectObj(new GameFlowEmptyState()));
        //進入副本
        SetTransition(GameFlowState.Init, SetInjectObj(new GameFlowInitState()));
        //進入關卡
        SetTransition(GameFlowState.StartFight, SetInjectObj(new GameFlowStartFightState()));
        //戰鬥循環
        SetTransition(GameFlowState.PlayerRound, SetInjectObj(new GameFlowPlayerRoundState()));
        SetTransition(GameFlowState.PlayerAction, SetInjectObj(new GameFlowPlayerActionState()));
        SetTransition(GameFlowState.MonsterRound, SetInjectObj(new GameFlowMonsterRoundState()));
        SetTransition(GameFlowState.MonsterAction, SetInjectObj(new GameFlowMonsterActionState()));
        //結算畫面
        SetTransition(GameFlowState.Profit, SetInjectObj(new GameFlowProfitState()));
        //結算後獎勵選擇
        SetTransition(GameFlowState.ChoiceReward, SetInjectObj(new GameFlowChoiceRewardState()));
        //下一個關卡前
        SetTransition(GameFlowState.NextLevel, SetInjectObj(new GameFlowNextLevelState()));
        //地圖選擇
        SetTransition(GameFlowState.Chest, SetInjectObj(new GameFlowChestState()));
        SetTransition(GameFlowState.GetAntique, SetInjectObj(new GameFlowGetAntiqueState()));
        SetTransition(GameFlowState.Rest, SetInjectObj(new GameFlowRestState()));
        SetTransition(GameFlowState.Store, SetInjectObj(new GameFlowShopState()));
        //結束副本
        SetTransition(GameFlowState.EndGame, SetInjectObj(new GameFlowEndGameState()));
    }


    /// <summary>
    /// 初始化 演出
    /// </summary>
    public void InitPerformanceCallBack()
    {
        WatchPerformanceData<PSwitchGameStateData>(SwitchState);
        WatchPerformanceData<POnDamageData>(performanceMethods.OnDamage);
        WatchPerformanceData<POnHealData>(performanceMethods.OnHeal);
        WatchPerformanceData<PMonsterRemoveData>(performanceMethods.MonsterRemove);
        WatchPerformanceData<PSummonMonsterData>(performanceMethods.PSummonMonsterData);
        WatchPerformanceData<PShowParticleData>(performanceMethods.ShowParticle);
        WatchPerformanceData<POnAttackData>(performanceMethods.OnAtack);
        WatchPerformanceData<PMonsterAppearData>(performanceMethods.MonsterAppear);
        WatchPerformanceData<PPendingData>((d) => d.taskFunc());

        WatchPerformanceData<PUIAnimatonStateData>(performanceMethods.DoUIAnimation);
        WatchPerformanceData<PCameraMoveData>(performanceMethods.MoveCamera);

        WatchPerformanceData<PPlayerSkillDataUpdate>(performanceMethods.UpdateSkillItem);
        WatchPerformanceData<PPlayerSkillDataInsert>(performanceMethods.InsertSKillItme);
        WatchPerformanceData<PPlayerSkillDataMove>(performanceMethods.MoveSKillItme);
        WatchPerformanceData<PPlayerSkillDataRemove>(performanceMethods.RemoveSKillItme);
        WatchPerformanceData<PPlayerSkillDataInit>(performanceMethods.InitSkillItem);
        WatchPerformanceData<PPlayerSkillDataBanned>(performanceMethods.BannedSkillItem);

        WatchPerformanceData<PPassiveData>(performanceMethods.PassiveData);
        WatchPerformanceData<PMonsterNextSkillData>(performanceMethods.MonsterNextSkill);
        WatchPerformanceData<PModifyShieldData>(performanceMethods.ModifyShield);
        WatchPerformanceData<PModifyColorData>(performanceMethods.ModifyColor);
        UpdatePerformance();//另一個Loop
    }

    public void DoPending(Func<UniTask> func)
    {
        AddPerformanceData(new PPendingData() { taskFunc = func });
    }

    /// <summary>
    /// 更新技能表演
    /// </summary>
    public void UpdatePlayerSkillItem()
    {
        var usedP = new PPlayerSkillDataUpdate();
        usedP.updateEnum = PPlayerSkillDataUpdate.UpdateTypeEnum.Use;
        var costP = new PPlayerSkillDataUpdate();
        costP.updateEnum = PPlayerSkillDataUpdate.UpdateTypeEnum.Cost;
        for (int i = 0; i < battleManager.player.skills.Count; i++)
        {
            if (battleManager.player.skills[i].isUsed) usedP.indexs.Add(i);
            if (battleManager.PlayerCheckSkillCost(i)) costP.indexs.Add(i);
        }
        AddPerformanceData(costP);
        AddPerformanceData(usedP);
    }

    public void ClearAllPerformanceCallBack()
    {
        performanceDatas.Clear();
        foreach (var item in performanceFuncs)
        {
            item.Value.Clear();
        }
        performanceFuncs.Clear();
        doingPerformance = false;
    }

    public void ActivePerformance(bool torf)
    {
        doingPerformance = torf;
    }

    public void UpdatePassiveData(BattleActor actor)
    {
        for (int i = 0; i < actor.passives.Count; i++)
        {
            var p = new PPassiveData();
            p.Init(actor);
            p.stackCount = actor.passives[i].currentStack;
            p.passiveId = actor.passives[i].passiveId;
            AddPerformanceData(p);
        }
    }
    /// <summary>
    /// 轉換狀態
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    UniTask SwitchState(PSwitchGameStateData data)
    {
        Trigger(data.state);
        return UniTask.WaitUntil(() => data.state == CurrentStateEnum);
    }

    /// <summary>
    /// 是否結束
    /// </summary>
    /// <returns></returns>
    public bool CheckWinAndLose()
    {
        // 相機歸位
        AddPerformanceData(new PCameraMoveData() { isAll = false, monsterPosition = BattleActor.MonsterPositionEnum.None });
        var r = battleManager.GameIsEndAndIsWin();
        if (r.isEnd)
        {
            // Send Win lose UIData
            passiveManager.OnActorPassive(battleManager.player, PassiveTriggerEnum.OnLoseOrWinBefore);
            isWinCache = r.isWin;
            SwichGameStateByPerformanceData(GameFlowState.Profit);
        }
        return r.isEnd;
    }

    public UniTask Save()
    {
        var removeSkill = new List<ActorSkill>();
        for (int i = 0; i < battleManager.player.skills.Count; i++)
        {
            if (battleManager.player.skills[i].originIndex == -1)
            {
                removeSkill.Add(battleManager.player.skills[i]);
            }
        }
        for (int i = 0; i < removeSkill.Count; i++)
        {
            battleManager.player.skills.Remove(removeSkill[i]);
        }
        return sdk.BattleLevelEnd(isWinCache, battleManager.player.skills); // TODO: 還要看這邊怎修改 by 閔弘
    }

    public void AddPerformanceData<T>(T data) where T : PerformanceData
    {
        if (!doingPerformance) return;
        if (performanceDatas.Contains(data))
        {
            if (data is PMultipleData mp)
            {
                mp.repeatCount++;
            }
            else
                return;
        }
        performanceDatas.Enqueue(data);
    }

    /// <summary>
    /// 需要等待表演結束再切換狀態
    /// </summary>
    /// <param name="state"></param>
    public void SwichGameStateByPerformanceData(GameFlowState state)
    {
        AddPerformanceData(new PSwitchGameStateData() { state = state });
    }

    /// <summary>
    /// 監聽表演資料
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    void WatchPerformanceData<T>(Func<T, UniTask> func) where T : PerformanceData
    {
        if (!doingPerformance) return;
        var dataName = typeof(T).ToString();
        if (!performanceFuncs.TryGetValue(dataName, out List<Func<PerformanceData, UniTask>> funcLs))
        {
            funcLs = new List<Func<PerformanceData, UniTask>>();
            performanceFuncs.Add(dataName, funcLs);
        }
        funcLs.Add((d) => func(d as T));
    }
    List<UniTask> pTasks = new List<UniTask>();
    /// <summary>
    /// 更新並表演演出
    /// </summary>
    async void UpdatePerformance()
    {
        while (doingPerformance)
        {
            while (performanceDatas.Count > 0 && doingPerformance)
            {
                var data = performanceDatas.Dequeue();
                try
                {
                    if (data is PMultipleData pm)
                    {
                        // 不是0不執行
                        if (pm.repeatCount > 0)
                        {
                            pm.repeatCount--;
                        }
                        else
                        {
                            //UtilityHelper.BattleLog($"Start MultipleData ↓↓↓↓↓↓↓↓↓↓", UtilityHelper.BattleLogEnum.Performance);
                            pTasks.Clear();
                            for (int i = 0; i < pm.performanceDatas.Count; i++)
                            {
                                pTasks.Add(DoPerformance(pm.performanceDatas[i]));
                            }
                            await UniTask.WhenAll(pTasks);
                            //UtilityHelper.BattleLog($"End MultipleData ↑↑↑↑↑↑↑↑↑↑", UtilityHelper.BattleLogEnum.Performance);
                        }
                    }
                    else
                        await DoPerformance(data);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.StackTrace);
                    Debug.LogError(e.GetBaseException().StackTrace);
                }
            }
            await UniTask.Delay(1);
        }
    }

    async UniTask DoPerformance(PerformanceData data)
    {
        var dataName = data.GetType().ToString();
        if (performanceFuncs.TryGetValue(dataName, out List<Func<PerformanceData, UniTask>> funcLs))
        {
            var strData = data == null ? "Null" : JsonConvert.SerializeObject(data);
            //UtilityHelper.BattleLog($"Do {dataName} Data : {strData}", UtilityHelper.BattleLogEnum.Performance);
            for (int i = 0; i < funcLs.Count; i++)
            {
                await funcLs[i](data);
            }
        }
    }

    T SetInjectObj<T>(T obj)
    {
        diContainer.Inject(obj);
        return obj;
    }
}

