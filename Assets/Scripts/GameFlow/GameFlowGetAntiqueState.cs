using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Zenject;

public class GameFlowGetAntiqueState : BaseState<GameFlowController, GameFlowController.GameFlowState>
{
    [Inject]
    UIManager uIManager;

    [Inject]
    PassiveManager passiveManager;

    [Inject]
    DataManager dataManager;

    [Inject]
    ItemManager itemManager;

    [Inject]
    SDKProtocol.IProtocolBridge sdk;

    [Inject]
    NetworkSaveManager saveManager;
    [Inject]
    BattleManager battleManager;

    ViewItemData selectItemData = null;
    public override UniTask End()
    {
        return default;
    }

    public override void OnAbort()
    {

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
        if (leveData.nodeEnum != MapNodeEnum.Antique)
        {
            Debug.LogErrorFormat("進了遺跡流程 DungeonLeveData 的 MapNodeEnum 卻是{0}??? DungeonId: {1}",
                leveData.nodeEnum, leveData.dungeonId);
            AntiqueEnd();
            return;
        }
        if (leveData.acquisitionItems != null &&
           leveData.acquisitionItems.Count > 0 &&
           leveData.acquisitionItems[0] != null &&
           leveData.acquisitionItems[0].Count > 0)
        {
            ViewItemData item = null;
            if (saveManager.GetContainer<NetworkSaveBattleHeroAttrContainer>().Exists(NetworkSaveBattleHeroAttrContainer.AttrType.ItemGet, leveData.acquisitionItems[0][0].id))
            {
                item = itemManager.GetViewItemData(ViewItemType.ItemData, 1);
                var antiqueItem = itemManager.GetViewItemData(ViewItemType.ItemData, leveData.acquisitionItems[0][0].id);
                item.count = antiqueItem.coinPrice;
            }
            else
            {
                item = itemManager.GetViewItemData(ViewItemType.ItemData, leveData.acquisitionItems[0][0].id);
                item.count = leveData.acquisitionItems[0][0].count;
            }

            //var item = itemManager.GetViewItemData(ViewItemType.ItemData, leveData.acquisitionItems[0][0].id);

            //item.count = leveData.acquisitionItems[0][0].count;
            item.coinPrice = 0;
            item.Insufficient = false;
            if (item != null)
            {
                itemDataList.Add(item);
            }
        }

        var antiqueUi = await uIManager.OpenUI<UIAntique>();
        if (antiqueUi != null)
        {
            await antiqueUi.Init(itemDataList, (passiveData) =>
            {
                selectItemData = passiveData;
            });

            await UniTask.WaitUntil(() => antiqueUi.IsDone);

            uIManager.RemoveUI(antiqueUi);
        }
        else
        {
            Debug.LogWarning("取得遺跡: UIAntique 開啟失敗，玩家將直接放棄取得被動技能");
        }

        if (selectItemData != null)
        {
            await sdk.BattleGainItem(selectItemData.id, selectItemData.count);
        }


        AntiqueEnd();
    }

    /// <summary>
    /// 遺跡結束，執行下一階段 (Flow)
    /// </summary>
    private void AntiqueEnd()
    {
        GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.NextLevel);
    }
}