using Cysharp.Threading.Tasks;
using SDKProtocol;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameFlowChestState : BaseState<GameFlowController, GameFlowController.GameFlowState>
{
    [Inject]
    UIManager uIManager;

    [Inject]
    SkillManager skillManager;

    [Inject]
    DataManager dataManager;

    [Inject]
    ItemManager itemManager;
    [Inject]
    NetworkSaveManager saveManager;
    [Inject]
    BattleManager battleManager;
    [Inject]
    GameFlowController gameFlow;

    [Inject]
    DataTableManager dataTableManager;
    [Inject]
    SDKProtocol.IProtocolBridge sdk;
    public override UniTask End()
    {
        return default;
    }

    public override void OnAbort()
    {
        throw new System.NotImplementedException();
    }

    public async override UniTask Start()
    {
        await OpenUIAsync();
        var ui = uIManager.FindUI<UIBattle>();
        battleManager.ResetPlayerActor();
        ui.UpdateAntiqueImages(battleManager.player.passives);
    }

    public override void Update()
    {

    }

    private async UniTask OpenUIAsync()
    {
        var itemDataList = new List<ViewItemData>();
        var leveData = dataManager.GetCurrentDungeonLeveData();
        if (leveData.nodeEnum != MapNodeEnum.Chest)
        {
            Debug.LogErrorFormat("進了寶箱流程 DungeonLeveData 的 MapNodeEnum 卻是{0}??? DungeonId: {1}",
                leveData.nodeEnum, leveData.dungeonId);
            ChestEnd();
            return;
        }

        if (leveData.acquisitionItems != null &&
           leveData.acquisitionItems.Count > 0 &&
           leveData.acquisitionItems[0] != null)
        {
            var itemList = leveData.acquisitionItems[0];
            ViewItemData item = null;
            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i].count == 0)
                {
                    Debug.LogWarningFormat("取得寶箱: 道具id: {0} 的數量為0個", itemList[i].id);
                    continue;
                }

                item = null;
                if (saveManager.GetContainer<NetworkSaveBattleHeroAttrContainer>().Exists(NetworkSaveBattleHeroAttrContainer.AttrType.ItemGet, itemList[i].id))
                {
                    item = itemManager.GetViewItemData(ViewItemType.ItemData, 1);
                    var antiqueItem = itemManager.GetViewItemData(ViewItemType.ItemData, itemList[i].id);
                    item.count = antiqueItem.coinPrice;
                }
                else
                {
                    item = itemManager.GetViewItemData(ViewItemType.ItemData, itemList[i].id);
                    item.count = itemList[i].count;
                }
                item.coinPrice = 0;
                item.Insufficient = false;
                item.ItemTypeStr = "寶箱";
                itemDataList.Add(item);

            }
        }

        var chestUi = await uIManager.OpenUI<UIChest>();
        await chestUi.Init(itemDataList);

        await UniTask.WaitUntil(() => { return chestUi.IsDone; });
        foreach (var item in itemDataList)
        {
            var result = await sdk.BattleGainItem(item.id, item.count);
            if (!result) continue;
            //依照物品類型進行表演&處理，目前只有治療、技能物品。
            switch (item.itemType)
            {
                case ItemTpyeEnum.Item:
                    var effectDefine = dataTableManager.GetItemEffectDataDefine(item.arg);
                    switch (effectDefine.effect.type)
                    {
                        case ItemEffectTypeEnum.cure:
                            chestUi.HealParticle.Play();
                            var value = battleManager.OnHeal(battleManager.player, effectDefine.effect.Arg2);
                            var healPerformance = new POnHealData();
                            healPerformance.Init(battleManager.player, value);
                            gameFlow.AddPerformanceData(healPerformance);
                            await UniTask.Delay((int)(chestUi.HealParticle.ParticleSystemLength() * 1000)); // 配合特效時間
                            break;
                        case ItemEffectTypeEnum.skill:
                            break;
                    }
                    break;
                case ItemTpyeEnum.Skill:
                    await OpenSkillUi(item.arg);
                    break;
                case ItemTpyeEnum.Chest:
                    //取得寶相的掉落物並且通知 sever
                    //var itmeLs = itemManager.GetItmes(mapP.dropGroupId, mapP.dropCount);
                    break;

            }
        }
        await chestUi.uITopBar.IsEffectEnd();
        uIManager.RemoveUI(chestUi);

        ChestEnd();
    }

    private async UniTask OpenSkillUi(int skillId)
    {
        var skill = await uIManager.OpenUI<UISkill>();
        await skill.OpenChangeSkillPage(saveManager.GetContainer<NetworkSaveBattleSkillContainer>().GetOriginalSKillList(), skillId);
        var isChange = await skill.IsSkillsChange();
    }
    /// <summary>
    /// 遺跡結束，執行下一階段 (Flow)
    /// </summary>
    private void ChestEnd()
    {
        GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.NextLevel);
    }
}
