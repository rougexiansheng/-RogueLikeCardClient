using SDKProtocol;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using static NetworkSaveBattleHeroAttrContainer;

public class ItemManager : IInitializable
{
    [Inject]
    DataManager dataManager;
    [Inject]
    DataTableManager dataTableManager;
    [Inject]
    NetworkSaveManager saveManager;

    public void Initialize()
    {

    }



    /// <summary>
    /// ���o�H���D��
    /// </summary>
    /// <param name="dropGroupId"></param>
    /// <param name="count">-1 get all</param>
    /// <returns></returns>
    public List<ItemData> GetItmes(int dropGroupId, int count)
    {
        var ls = new List<ItemData>();
        var groups = dataTableManager.GetDropGroupDataDefine(dropGroupId);
        if (count == -1)// �������o
        {
            for (int i = 0; i < groups.Count; i++)
            {
                var d = new ItemData() { id = groups[i].itemId, count = groups[i].count };
                ls.Add(d);
            }
        }
        else //��o�X��
        {
            for (int i = 0; i < count; i++)
            {
                var total = 0;
                var r = Random.Range(0, 100);
                for (int j = 0; j < groups.Count; j++)
                {
                    var d = groups[j];
                    total += d.probability;
                    if (total > r)
                    {
                        var item = new ItemData() { id = d.itemId, count = d.count };
                        ls.Add(item);
                        break;
                    }
                }
            }
        }
        return ls;
    }

    public ViewItemData GetViewItemData(ViewItemType viewItemType, int itemId)
    {
        var viewItemData = new ViewItemData();
        viewItemData.viewItemType = viewItemType;
        switch (viewItemType)
        {
            case ViewItemType.ItemData:
                var item = dataTableManager.GetItemDataDefine(itemId);
                if (item == null) return null;
                viewItemData.id = item.id;
                viewItemData.name = item.name;
                viewItemData.comment = item.comment;
                viewItemData.icon = item.icon;
                viewItemData.itemType = item.itemType;
                viewItemData.ItemTypeStr = GetViewItemDataStr(viewItemData.itemType);
                viewItemData.coinPrice = item.coinPrice;
                viewItemData.mallPrice = item.mallPrice;
                viewItemData.arg = item.arg;
                break;
            case ViewItemType.SkillData:
                var skillData = dataTableManager.GetSkillDefine(itemId);
                if (skillData == null) return null;
                viewItemData.id = itemId;
                viewItemData.name = skillData.skillName;
                viewItemData.comment = skillData.comment;
                viewItemData.icon = skillData.icon;
                viewItemData.ItemTypeStr = "技能";
                break;
        }


        var coin = saveManager.GetContainer<NetworkSaveBattleItemContainer>().GetCount(NetworkSaveBattleItemContainer.COIN);
        viewItemData.Insufficient = viewItemData.coinPrice > coin;
        viewItemData.Purchased = saveManager.GetContainer<NetworkSaveBattleHeroAttrContainer>().Exists(AttrType.ItemGet, itemId);

        return viewItemData;
    }

    /// <summary>
    /// 道具左下角會顯示的文字
    /// </summary>
    private string GetViewItemDataStr(ItemTpyeEnum itemType)
    {
        var str = "";
        switch (itemType)
        {
            case ItemTpyeEnum.Item:
                str = "道具";
                break;
            case ItemTpyeEnum.SkillFragment:
                str = "技能書殘卷";
                break;
            case ItemTpyeEnum.Antique:
                str = "被動";
                break;
            case ItemTpyeEnum.Chest:
                str = "大禮包";
                break;
            case ItemTpyeEnum.LuckyBag:
                str = "福袋";
                break;
            case ItemTpyeEnum.Skin:
                str = "外觀";
                break;
            case ItemTpyeEnum.Skill:
                str = "技能";
                break;
            case ItemTpyeEnum.Coin:
                str = "金幣";
                break;
            default:
                break;
        }

        return str;
    }
}
