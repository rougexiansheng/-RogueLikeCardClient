using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public partial class FakeServer
{
    /// <summary>
    /// 戰鬥中選擇道具購買
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public async UniTask<bool> BattleSelectItem(int itemId)
    {
        var table = dataTableManager.GetItemDataDefine(itemId);

        var dungeonLeveData = dataManager.GetCurrentDungeonLeveData();
        // 做個防呆確認是該關卡的獎勵避免 client 亂塞
        var dropItem = dungeonLeveData.acquisitionItems.Find(x => x.Exists(y => y.id == itemId));
        if (dropItem != null)
        {
            var item = dataTableManager.GetItemDataDefine(itemId);
            if (item != null)
            {
                //if (!checkBuy ||!fakeServerData.player.actorCache.itemList.Exists(x => x == item.id)) // 判斷是否已購買

                // 檢查金幣跟扣款
                var coin = GetItemCount(NetworkSaveBattleItemContainer.COIN);
                if (coin >= item.coinPrice)
                {
                    var newCoin = await BattleLosesItem(NetworkSaveBattleItemContainer.COIN, table.coinPrice);
                    if ((coin - table.coinPrice) == newCoin) // 扣款成功
                    {
                        var success = await AddGetItem(item, dropItem.Count);
                        if (success) // 道具增加成功
                        {
                            var clientSave = new ClientSave(Cmd.update);
                            clientSave.Add(ConvertSaveToBattleItemData(NetworkSaveBattleItemContainer.COIN));
                            clientSave.Add(ConvertSaveToBattleHeroAttrData());
                            var clientSaveResult = new List<JsonObject>() { clientSave.ToJsonObject() };

                            return await EndProtocol(true, clientSaveResult);
                        }
                        else // 道具增加失敗，把錢還人家
                        {
                            await BattleGainItem(NetworkSaveBattleItemContainer.COIN, table.coinPrice);
                        }
                    }
                }

            }
        }

        return await EndProtocol(false);
    }

    /// <summary>
    /// 實際加入道具
    /// </summary>
    private async UniTask<bool> AddGetItem(ItemDataDefine itemData, int count)
    {
        switch (itemData.itemType)
        {
            case ItemTpyeEnum.Coin:
                var coin = GetItemCount(NetworkSaveBattleItemContainer.COIN);
                var newCount = DoAddItem(itemData.id, count);
                return coin != newCount;

            case ItemTpyeEnum.Item:
                var effectDefine = dataTableManager.GetItemEffectDataDefine(itemData.arg);
                switch (effectDefine.effect.type)
                {
                    case ItemEffectTypeEnum.cure:
                        return true;
                    case ItemEffectTypeEnum.skill:
                        return true;
                }
                break;
            case ItemTpyeEnum.SkillFragment:
                var skillFragment = GetItemCount(NetworkSaveBattleItemContainer.SKILL_FRAGMENT);
                var newSkillFragment = DoAddItem(itemData.id, count);
                //var getSKillResult = await BattleGainItem(NetworkSaveBattleItemContainer.SKILL_FRAGMENT, count);
                return skillFragment != newSkillFragment;

            case ItemTpyeEnum.Antique:
                //避免得到重複的遺物道具
                if (fakeServerData.player.actorCache.itemList.Exists(x => x == itemData.id))
                    return false;
                fakeServerData.player.actorCache.passives.Add(itemData.arg);
                fakeServerData.player.actorCache.itemList.Add(itemData.id);
                return true;
            case ItemTpyeEnum.Chest:
                for (int i = 0; i < count; i++)
                {
                    var chestItems = await AddDropGroup(itemData.arg, -1);
                    if (chestItems.Exists(x => x.Item2)) // 有一個加入成功就當全部成功 (暫時沒處理失敗狀況)
                        continue;
                    return false;
                }
                return true;
            case ItemTpyeEnum.LuckyBag:
                for (int i = 0; i < count; i++)
                {
                    var luckyBagItems = await AddDropGroup(itemData.arg, 1);
                    if (luckyBagItems.Exists(x => !x.Item2))
                        return false;
                }
                return true;
            case ItemTpyeEnum.Skin:
                //避免得到重複的Skin道具
                if (fakeServerData.player.actorCache.itemList.Exists(x => x == itemData.id))
                    return false;
                fakeServerData.player.actorCache.itemList.Add(itemData.id);
                return await AddSkin(itemData.arg);
            case ItemTpyeEnum.Character:
                return await AddProfession(itemData.arg);
            case ItemTpyeEnum.Energy:
                break;
            case ItemTpyeEnum.Skill:
                // 增加技能回歸到 UiSkill 去處理，所以這邊單純紀錄不處理
                fakeServerData.player.actorCache.skillIds.Add(itemData.arg);
                return true;
            default:
                break;
        }

        Debug.LogError($"尚未實做新增掉落物 ItemTpyeEnum:{itemData.itemType} 的方法, 掉落物 Id: {itemData.id}");
        return false;
    }

    /// <summary>
    /// 增加掉落物
    /// </summary>
    /// <param name="dropId">掉落物id</param>
    /// <param name="count">-1 全拿</param>
    /// <returns>取得的(道具id, 加入是否成功)</returns>
    private async UniTask<List<(int, bool)>> AddDropGroup(int dropId, int count)
    {
        var success = false;
        var result = new List<(int, bool)>();
        var itemList = itemManager.GetItmes(dropId, count);
        if (itemList != null && itemList.Count > 0)
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                var item = dataTableManager.GetItemDataDefine(itemList[i].id);
                if (item != null)
                {
                    if (item.itemType != ItemTpyeEnum.Chest && // 抽選禮物不能又是抽選禮物 (避免陷入迴圈的 BUG)
                        item.itemType != ItemTpyeEnum.LuckyBag)
                    {
                        success = await BattleSelectItem(item.id);
                        result.Add((item.id, success));
                        if (!success)
                        {
                            Debug.LogError($"大禮包有道具加入失敗，禮包id:{dropId}, 掉落物 Id: {item.id}, 目前沒特別處理");
                        }

                        // 回寫SaveContainer的資料 (已在 ChooseGetItemData 內做掉這邊不重複寫)
                    }
                }
            }
        }

        return await EndProtocol(result);
    }

    private UniTask<bool> AddCharacter(int heroId)
    {
        // 添加英雄
        var result = DoAddCharacter(heroId);

        // 回寫SaveContainer的資料
        if (result)
        {
            var clientSave = new ClientSave(Cmd.update);
            clientSave.Add(ConvertSaveToPlayerData());
            var clientSaveResult = new List<JsonObject>() { clientSave.ToJsonObject() };
            return EndProtocol(result, clientSaveResult);
        }

        return EndProtocol(result);
    }

    private UniTask<bool> AddSkin(int skinId)
    {
        var result = false;
        var skin = dataTableManager.GetProfessionSkinDataDefine(skinId);
        if (skin != null)
        {
            // 添加皮膚
            result = DoAddSkin(skinId);

            // 回寫SaveContainer的資料
            if (result)
            {
                var clientSave = new ClientSave(Cmd.update);
                clientSave.Add(ConvertSaveToPlayerData());
                var clientSaveResult = new List<JsonObject>() { clientSave.ToJsonObject() };
                return EndProtocol(result, clientSaveResult);
            }
        }

        return EndProtocol(result);
    }

    private UniTask<bool> AddProfession(int heroId)
    {
        // 添加英雄
        var result = DoAddCharacter(heroId);

        // 回寫SaveContainer的資料
        if (result)
        {
            var clientSave = new ClientSave(Cmd.update);
            clientSave.Add(ConvertSaveToPlayerData());
            var clientSaveResult = new List<JsonObject>() { clientSave.ToJsonObject() };
            return EndProtocol(result, clientSaveResult);
        }

        return EndProtocol(result);
    }
}