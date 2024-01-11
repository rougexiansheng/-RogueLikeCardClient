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
            await sdk.BattleGainItem(item.id, item.count);
        }
        await chestUi.uITopBar.IsEffectEnd();
        uIManager.RemoveUI(chestUi);

        ChestEnd();
    }

    /// <summary>
    /// 遺跡結束，執行下一階段 (Flow)
    /// </summary>
    private void ChestEnd()
    {
        GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.NextLevel);
    }
}
