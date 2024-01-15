using Cysharp.Threading.Tasks;
using DG.Tweening;
using SDKProtocol;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class GameFlowChoiceRewardState : BaseState<GameFlowController, GameFlowController.GameFlowState>
{
    [Inject]
    SDKProtocol.IProtocolBridge sdk;
    [Inject]
    BattleManager battleManager;
    [Inject]
    UIManager uIManager;
    [Inject]
    NetworkSaveManager saveManager;
    [Inject]
    DataTableManager dataTableManager;

    [Inject]
    GameFlowController gameFlow;

    [Inject]
    DiContainer diContainer;

    NumericalManager numericalManager = new NumericalManager();


    [SerializeField]
    private int coinReward = 100;

    [SerializeField]
    private int resetPersent = 20;

    [Inject]
    ItemManager itemManager;

    public enum RewardChooseEnum
    {
        Coin,
        Skill,
        Rest,
    }
    public override UniTask End()
    {
        battleManager.StopTimer();
        RxEventBus.UnRegister(this);
        return default;
    }

    public override void OnAbort()
    {

    }

    public async override UniTask Start()
    {
        diContainer.Inject(numericalManager);
        battleManager.StartTimer();

        NetworkSaveBattleDungeonContainer dungeonData = saveManager.GetContainer<NetworkSaveBattleDungeonContainer>();
        await sdk.BattleLevelRewards(null);
        // Check Dungeon is done or not
        if (dungeonData.IsDone)
        {
            GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.EndGame);
            return;
        }
        var dungon = dataTableManager.GetDungeonDataDefine(dungeonData.FightDungeonId);
        //固定回復20
        int resetValue = numericalManager.GetHealHPValue(battleManager.player.maxHp);

        int coinValue = numericalManager.GetCoinValue(dungon.selectCoin);
        var uiBattleRewardChoose = await openUIBattleRewardChoose(coinValue, resetValue);
        var rewardType = await uiBattleRewardChoose.ChooseReward();
        ViewItemData selectItemData = null;
        switch (rewardType)
        {
            case RewardChooseEnum.Coin:

                await sdk.BattleGainItem(NetworkSaveBattleItemContainer.COIN, coinValue);
                uiBattleRewardChoose.GoldRingParticle.Play();
                await uiBattleRewardChoose.IconMoveToCenterAsync(UIBattleRewardChoose.RewardType.Coin);
                uiBattleRewardChoose.GoldParticle.Play();
                //TODO 金幣音效
                await uiBattleRewardChoose.IsCoinEffectFinished();
                await UniTask.Delay((int)(uiBattleRewardChoose.GoldParticle.ParticleSystemLength() * 1000)); // 配合特效時間
                uIManager.RemoveUI<UIBattleRewardChoose>();

                GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.NextLevel);
                break;
            case RewardChooseEnum.Skill:
                var chooseSkillDataList = createRewardSkills();
                await uiBattleRewardChoose.FadeOutThisUIPage();
                uIManager.RemoveUI<UIBattleRewardChoose>();
                var uiSkillRewardPage = await uIManager.OpenUI<UIChest>();
                uiSkillRewardPage.InitWitoutPerformance(chooseSkillDataList);
                while (!uiSkillRewardPage.IsDone)
                {
                    selectItemData = await uiSkillRewardPage.SelectSkill();
                    uiSkillRewardPage.ResetTask();
                    if (selectItemData != null)
                    {
                        var skill = await uIManager.OpenUI<UISkill>();
                        await skill.OpenChangeSkillPage(saveManager.GetContainer<NetworkSaveBattleSkillContainer>().GetOriginalSKillList(), selectItemData.id);
                        var isChange = await skill.IsSkillsChange();
                        if (isChange)
                        {
                            uiSkillRewardPage.Done();
                        }
                        else
                        {
                            uIManager.RemoveUI(skill);
                        }
                        selectItemData = null;
                    }
                }
                uIManager.RemoveUI(uiSkillRewardPage);
                GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.NextLevel);
                break;
            case RewardChooseEnum.Rest:
                uiBattleRewardChoose.HealRingParticle.Play();
                await uiBattleRewardChoose.IconMoveToCenterAsync(UIBattleRewardChoose.RewardType.Rest);
                uiBattleRewardChoose.HealParticle.Play();
                var value = battleManager.OnHeal(battleManager.player, resetValue);
                //新增治療表演
                var healPerformance = new POnHealData();
                healPerformance.Init(battleManager.player, value);
                gameFlow.AddPerformanceData(healPerformance);
                await UniTask.Delay((int)(uiBattleRewardChoose.HealParticle.ParticleSystemLength() * 1000)); // 配合特效時間
                await uiBattleRewardChoose.IconDisappearAsync(UIBattleRewardChoose.RewardType.Rest);
                await uiBattleRewardChoose.FadeOutThisUIPage();
                uIManager.RemoveUI<UIBattleRewardChoose>();
                GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.NextLevel);

                break;

        }


    }

    /// <summary>
    /// 換到 Handler
    /// </summary>
    /// <returns></returns>
    private List<ViewItemData> createRewardSkills()
    {
        var itemDataList = new List<ViewItemData>();

        var dungeonDataDefine = dataTableManager.GetDungeonDataDefine(saveManager.GetContainer<NetworkSaveBattleDungeonContainer>().FightDungeonId);
        ProfessionDataDefine professionDefine = dataTableManager.GetProfessionDataDefine(saveManager.GetContainer<NetworkSaveBattleDungeonContainer>().SelectProfession);
        var selecSkillList = professionDefine.selectSkills[dungeonDataDefine.selectSkillIndex];

        for (int i = 0; i < 3; i++)
        {
            int index = Random.Range(0, selecSkillList.Count);
            var itemData = itemManager.GetViewItemData(ViewItemType.SkillData, selecSkillList[index]);
            itemData.coinPrice = 0; // 把價格修改為0，符合預覽
            itemData.count = -1;
            itemDataList.Add(itemData);
        }
        return itemDataList;
    }

    private async UniTask<UIBattleRewardChoose> openUIBattleRewardChoose(int coin, float resetValue)
    {
        await uIManager.OpenUI<UIBattleRewardChoose>();
        var rewardChoose = uIManager.FindUI<UIBattleRewardChoose>();
        rewardChoose.Init(coin, resetValue);
        return rewardChoose;
    }
    public override void Update()
    {

    }
}
