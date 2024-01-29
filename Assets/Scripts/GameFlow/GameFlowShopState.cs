using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Zenject;


public class GameFlowShopState : BaseState<GameFlowController, GameFlowController.GameFlowState>
{
    [Inject]
    UIManager uIManager;

    [Inject]
    DataManager dataManager;

    [Inject]
    SkillManager skillManager;

    [Inject]
    ItemManager itemManager;
    [Inject]
    DataTableManager dataTableManager;
    [Inject]
    BattleManager battleManager;
    [Inject]
    GameFlowController gameFlow;
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

    }

    public override async UniTask Start()
    {
        ViewItemData selectItemData = null;
        var itemDataList = new List<ViewItemData>();
        var itemList = new List<int>();
        var leveData = dataManager.GetCurrentDungeonLeveData();
        if (leveData.acquisitionItems != null &&
            leveData.acquisitionItems.Count > 0 &&
            leveData.acquisitionItems[0] != null)
        {
            ViewItemData item = null;
            for (int i = 0; i < leveData.acquisitionItems[0].Count; i++)
            {
                item = null;
                if (saveManager.GetContainer<NetworkSaveBattleHeroAttrContainer>().Exists(NetworkSaveBattleHeroAttrContainer.AttrType.ItemGet, leveData.acquisitionItems[0][i].id))
                {
                    item = itemManager.GetViewItemData(ViewItemType.ItemData, 1);
                    var antiqueItem = itemManager.GetViewItemData(ViewItemType.ItemData, leveData.acquisitionItems[0][i].id);
                    item.count = antiqueItem.coinPrice;
                }
                else
                {
                    item = itemManager.GetViewItemData(ViewItemType.ItemData, leveData.acquisitionItems[0][i].id);
                    item.count = leveData.acquisitionItems[0][0].count;
                }

                if (item != null)
                {
                    itemDataList.Add(item);
                }
            }
        }

        var shopUi = await uIManager.OpenUI<UIShop>();

        await shopUi.Init(itemDataList);
        while (!shopUi.IsDone)
        {
            selectItemData = await shopUi.SelectItem();
            shopUi.ResetTask();
            if (selectItemData != null)
            {
                var result = await sdk.BattleSelectItem(selectItemData.id);

                if (!result) continue;
                //依照物品類型進行表演&處理，目前只有治療、技能物品。
                switch (selectItemData.itemType)
                {
                    case ItemTpyeEnum.Item:
                        var effectDefine = dataTableManager.GetItemEffectDataDefine(selectItemData.arg);
                        switch (effectDefine.effect.type)
                        {
                            case ItemEffectTypeEnum.cure:
                                shopUi.HealParticle.Play();
                                var value = battleManager.OnHeal(battleManager.player, effectDefine.effect.Arg2);
                                var healPerformance = new POnHealData();
                                healPerformance.Init(battleManager.player, value);
                                gameFlow.AddPerformanceData(healPerformance);
                                await UniTask.Delay((int)(shopUi.HealParticle.ParticleSystemLength() * 1000)); // 配合特效時間
                                break;
                            case ItemEffectTypeEnum.skill:
                                break;
                        }
                        break;
                    case ItemTpyeEnum.Skill:
                        await OpenSkillUi(selectItemData.arg);
                        break;
                    case ItemTpyeEnum.Chest:
                        //取得寶相的掉落物並且通知 sever
                        //var itmeLs = itemManager.GetItmes(mapP.dropGroupId, mapP.dropCount);
                        break;

                }
                selectItemData = null;
            }


        }
        var ui = uIManager.FindUI<UIBattle>();
        ui.UpdateAntiqueImages(battleManager.player.passives);
        uIManager.RemoveUI(shopUi);
        ShopEnd();
    }


    private async UniTask OpenSkillUi(int skillId)
    {
        var skill = await uIManager.OpenUI<UISkill>();
        await skill.OpenChangeSkillPage(saveManager.GetContainer<NetworkSaveBattleSkillContainer>().GetOriginalSKillList(), skillId);
        var isChange = await skill.IsSkillsChange();
    }
    public override void Update()
    {

    }

    private void ShopEnd()
    {

        GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.NextLevel);
    }
}
