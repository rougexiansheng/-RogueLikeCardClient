using NanoidDotNet;
using SDKProtocol;
using System;
using System.Linq;
using System.Collections.Generic;
using static NetworkSaveBattleHeroAttrContainer;
using static NetworkSaveRankContainer;
using Debug = UnityEngine.Debug;

public partial class FakeServer
{
    /// <summary>
    /// 將伺服器玩家資料轉成客戶端用的存檔
    /// </summary>
    private ClientSave.Data ConvertSaveToPlayerData()
    {
        var username = (!string.IsNullOrEmpty(fakeServerData.player.profile.userName)) ?
            fakeServerData.player.profile.userName : "預設玩家";

        var uid = (!string.IsNullOrEmpty(fakeServerData.player.profile.uid)) ?
            fakeServerData.player.profile.uid : Nanoid.Generate(size: 16);

        var playerDatas = new List<INetworkSaveData>();
        playerDatas.Add(new NetworkSavePlayerData()
        {
            username = username,
            uid = uid,
            unlockProfessionIds = fakeServerData.player.profile.unlockProfessionIDs,
            unlockSkinIds = fakeServerData.player.profile.unlockSkinIds
        });
        return new ClientSave.Data() { targetSaveContainer = SaveContainer.player, datas = playerDatas };
    }

    /// <summary>
    /// 將伺服器所有的道具轉成客戶端用的存檔
    /// </summary>
    private ClientSave.Data ConvertSaveToBattleItemData()
    {
        var itemDatas = new List<INetworkSaveData>();
        foreach (var item in fakeServerData.player.profile.items)
        {
            itemDatas.Add(new NetworkSaveBattleItemData() { id = item.Key, count = item.Value });
        }
        return new ClientSave.Data() { targetSaveContainer = SaveContainer.battleItem, datas = itemDatas };
    }

    /// <summary>
    /// 將伺服器特定的道具轉成客戶端用的存檔
    /// </summary>
    private ClientSave.Data ConvertSaveToBattleItemData(params int[] itemIds)
    {
        var itemDatas = new List<INetworkSaveData>();
        if (itemIds == null)
        {
            return new ClientSave.Data() { targetSaveContainer = SaveContainer.battleItem, datas = itemDatas };
        }
        foreach (var itemId in itemIds)
        {
            itemDatas.Add(new NetworkSaveBattleItemData() { id = itemId, count = GetItemCount(itemId) });
        }
        return new ClientSave.Data() { targetSaveContainer = SaveContainer.battleItem, datas = itemDatas };
    }

    private int GetItemCount(int itemId)
    {
        fakeServerData.player.profile.items.TryGetValue(itemId, out var count);
        return count;
    }

    private void SetItemCount(int itemId, int count)
    {
        if (!fakeServerData.player.profile.items.ContainsKey(itemId))
            fakeServerData.player.profile.items.Add(itemId, count);
        else
            fakeServerData.player.profile.items[itemId] = count;
    }

    private int DoReduceItem(int itemId, int count)
    {
        var currentCount = GetItemCount(itemId);
        var result = Math.Max(0, currentCount - count);
        SetItemCount(itemId, result);
        return result;
    }

    private int DoAddItem(int itemId, int count)
    {
        var currentCount = GetItemCount(itemId);
        var result = Math.Max(0, currentCount + count);
        SetItemCount(itemId, result);
        return result;
    }
    private bool SkillExist(ActorProfessionEnum professionEnum, int id)
    {
        return fakeServerData.player.profile.playerProfessionDataDic[professionEnum].UnlockSkillIDs.Contains(id);
    }

    private bool DoUnlockSkill(ActorProfessionEnum professionEnum, int id)
    {
        if (!SkillExist(professionEnum, id))
        {
            fakeServerData.player.profile.playerProfessionDataDic[professionEnum].UnlockSkillIDs.Add(id);
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool ProfessionExist(int id)
    {
        return fakeServerData.player.profile.unlockProfessionIDs.Contains(id);
    }

    private bool DoAddCharacter(int id)
    {
        if (ProfessionExist(id))
        {
            return false;
        }

        // 職業類型
        var type = (ActorProfessionEnum)id;

        // 獲取表格
        var table = dataTableManager.GetProfessionDataDefine(type);

        // 解鎖職業ID
        fakeServerData.player.profile.unlockProfessionIDs.Add(id);

        // 建立職業資料
        fakeServerData.player.profile.playerProfessionDataDic.Add(type, new PlayerProfessionData());

        // 解鎖技能
        foreach (var skill in table.prepareSkills)
        {
            DoUnlockSkill(type, skill);
        }

        // 初始化牌組1
        var group1 = new SkillGroup()
        {
            Name = "測試資料1",
            Skills = table.prepareSkills,
        };
        fakeServerData.player.profile.playerProfessionDataDic[type].SkillGroups.Add(group1);

        // 初始化牌組2
        var group2 = new SkillGroup()
        {
            Name = "測試資料2",
            Skills = table.prepareSkills,
        };
        fakeServerData.player.profile.playerProfessionDataDic[type].SkillGroups.Add(group2);

        Debug.Log($"<color=#F9F900>☆☆☆解鎖職業{type}成功!!☆☆☆</color>");

        SaveFakeServerData();

        return true;
    }

    private bool SkinExist(int skinId)
    {
        return fakeServerData.player.profile.unlockSkinIds.Contains(skinId);
    }

    private bool DoAddSkin(int skinId)
    {
        if (!SkinExist(skinId))
        {
            fakeServerData.player.profile.unlockSkinIds.Add(skinId);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 禮包類型
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    private int DoAddPackeg(int itemId, int count)
    {
        var currentCount = GetItemCount(itemId);
        var result = Math.Max(0, currentCount + count);
        SetItemCount(itemId, result);
        return result;
    }

    private bool BattleSkillIndexExist(int index)
    {
        return fakeServerData.player.actorCache.actorSkills.ContainsKey(index);
    }

    private bool DoReplaceBattleSkill(int index, ActorSkill actorSkill)
    {
        if (BattleSkillIndexExist(index))
        {
            //Debug.Log($"BeforeDoReplaceBattleSkill: {JsonConvert.SerializeObject(fakeServerData.player.serverActorData.actorSkills)}");
            var item = fakeServerData.player.actorCache.actorSkills.First(i => i.Value.originIndex == index);
            fakeServerData.player.actorCache.actorSkills[item.Key] = actorSkill;
            //Debug.Log($"AfterDoReplaceBattleSkill: {JsonConvert.SerializeObject(fakeServerData.player.serverActorData.actorSkills)}");
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool DoSetActorSkill(ActorProfessionEnum selectProfession, int selectSkillGroupIndex)
    {
        if (!fakeServerData.player.profile.playerProfessionDataDic.ContainsKey(selectProfession))
        {
            Debug.LogError($"{TAG} DoSetActorSkill: {selectProfession} not unlocked yet.");
            return false;
        }

        if (fakeServerData.player.profile.playerProfessionDataDic[selectProfession] == null)
        {
            Debug.LogError($"{TAG} DoSetActorSkill: {selectProfession}'s data is null.");
            return false;
        }

        // 從伺服器存檔獲取對應索引的技能組
        var skillGroups = fakeServerData.player.profile.playerProfessionDataDic[selectProfession].SkillGroups;
        var selectSkills = (skillGroups != null && selectSkillGroupIndex < skillGroups.Count) ?
            skillGroups[selectSkillGroupIndex].Skills : null;

        // 將技能組轉成戰鬥中暫存的結構
        var actorSkills = new List<ActorSkill>();
        if (selectSkills != null)
        {
            for (int i = 0; i < selectSkills.Count; i++)
            {
                actorSkills.Add(new ActorSkill()
                {
                    skillId = selectSkills[i],
                    isUsed = false,
                    originIndex = i,
                });
            }
        }
        else
        {
            Debug.LogError($"{TAG} DoSetActorSkill: selectSkills is null.");
        }

        // 清空暫存技能組
        fakeServerData.player.actorCache.actorSkills.Clear();

        // 將選擇的技能組存到暫存技能組
        foreach (var actorSkill in actorSkills)
        {
            fakeServerData.player.actorCache.actorSkills.Add(actorSkill.originIndex, actorSkill);
        }

        return true;
    }


    /// <summary>
    /// 將伺服器所有的職業轉成客戶端用的存檔
    /// </summary>
    private ClientSave.Data ConvertSaveToProfessionData()
    {
        var professionDatas = new List<INetworkSaveData>();
        foreach (var item in fakeServerData.player.profile.playerProfessionDataDic)
        {
            var id = item.Key;
            var data = item.Value;
            professionDatas.Add(new NetworkSaveProfessionData()
            {
                id = id,
                unlockCharacterSkinIds = data.UnlockCharacterSkinIds,
                unlockSkillIDs = data.UnlockSkillIDs,
                skillGroups = data.SkillGroups,
                unlockPassiveGourpIds = data.unlockPassiveGourpIds,
                defalutProfessionDatas = data.DefalutProfessionDatas
            });
        }
        return new ClientSave.Data() { targetSaveContainer = SaveContainer.profession, datas = professionDatas };
    }

    /// <summary>
    /// 將伺服器特定的職業轉成客戶端用的存檔
    /// </summary>
    private ClientSave.Data ConvertSaveToProfessionData(params ActorProfessionEnum[] professions)
    {
        var professionDatas = new List<INetworkSaveData>();
        if (professions == null)
        {
            return new ClientSave.Data() { targetSaveContainer = SaveContainer.profession, datas = professionDatas };
        }
        foreach (var profession in professions)
        {
            if (fakeServerData.player.profile.playerProfessionDataDic.ContainsKey(profession))
            {
                var id = profession;
                var data = fakeServerData.player.profile.playerProfessionDataDic[profession];
                professionDatas.Add(new NetworkSaveProfessionData()
                {
                    id = id,
                    unlockCharacterSkinIds = data.UnlockCharacterSkinIds,
                    unlockSkillIDs = data.UnlockSkillIDs,
                    skillGroups = data.SkillGroups,
                    unlockPassiveGourpIds = data.unlockPassiveGourpIds,
                    defalutProfessionDatas = data.DefalutProfessionDatas
                });
            }
        }
        return new ClientSave.Data() { targetSaveContainer = SaveContainer.profession, datas = professionDatas };
    }

    /// <summary>
    /// 將伺服器特定的排行榜轉成客戶端用的存檔
    /// </summary>
    private ClientSave.Data ConvertSaveToRankData(RankType rankType)
    {
        var rankDatas = new List<INetworkSaveData>();
        if (rankDatas == null)
        {
            return new ClientSave.Data() { targetSaveContainer = SaveContainer.rank, datas = rankDatas };
        }
        switch (rankType)
        {
            case RankType.Dungeon:
                {
                    rankDatas.Add(new NetworkSaveRankData()
                    {
                        fullRankings = fakeServerData.ranking.dungeonData.fullRankings,
                        lastRanking = fakeServerData.ranking.dungeonData.lastRanking
                    });
                }
                break;
            default:
                {
                    Debug.LogError($"尚未實做排行榜{rankType}的轉換方法");
                }
                break;
        }
        return new ClientSave.Data() { targetSaveContainer = SaveContainer.rank, datas = rankDatas };
    }

    /// <summary>
    /// 將伺服器地下城緩存資料轉成客戶端用的存檔
    /// </summary>
    private ClientSave.Data ConvertSaveToBattleDungeonData()
    {
        var dungeonCacheDatas = new List<INetworkSaveData>();
        dungeonCacheDatas.Add(new NetworkSaveBattleDungeonData()
        {
            professionEnum = fakeServerData.player.dungeonCache.professionEnum,
            dungeonGroupId = fakeServerData.player.dungeonCache.dungeonGroupId,
            dungeonDataIndex = fakeServerData.player.dungeonCache.dungeonDataIndex,
            fightDungeonId = fakeServerData.player.dungeonCache.fightDungeonId,
            lastCache = fakeServerData.player.dungeonCache.lastCache,
            isDone = fakeServerData.player.dungeonCache.isDone,
            isWin = fakeServerData.player.dungeonCache.isWin
        });
        return new ClientSave.Data() { targetSaveContainer = SaveContainer.battleDungeon, datas = dungeonCacheDatas };
    }



    /// <summary>
    /// 將伺服器地下城緩存資料轉成客戶端用的存檔
    /// </summary>
    private ClientSave.Data ConvertSaveToBattleDungeonData(int maxCount, out PlayerServerData d)
    {
        d = JsonClone(fakeServerData.player);
        if (d.dungeonCache.lastCache.Count > maxCount)
        {
            var count = d.dungeonCache.lastCache.Count - maxCount;
            d.dungeonCache.lastCache.RemoveRange(0, count);
        }

        var dungeonCacheDatas = new List<INetworkSaveData>();
        dungeonCacheDatas.Add(new NetworkSaveBattleDungeonData()
        {
            dungeonGroupId = d.dungeonCache.dungeonGroupId,
            dungeonDataIndex = d.dungeonCache.dungeonDataIndex,
            fightDungeonId = d.dungeonCache.fightDungeonId,
            lastCache = d.dungeonCache.lastCache,
            isDone = d.dungeonCache.isDone,
            isWin = d.dungeonCache.isWin
        });
        return new ClientSave.Data() { targetSaveContainer = SaveContainer.battleDungeon, datas = dungeonCacheDatas };
    }

    /// <summary>
    /// 將伺服器玩家角色緩存資料轉成客戶端用的存檔
    /// </summary>
    private ClientSave.Data ConvertSaveToUnlockProfessionData(int unlockID, out PlayerServerData d)
    {
        d = JsonClone(fakeServerData.player);
        var UnlockProfessionList = new List<INetworkSaveData>();
        UnlockProfessionList.Add(new NetworkSavePlayerData()
        {
            username = d.profile.userName,
            uid = d.profile.uid,
            unlockProfessionIds = d.profile.unlockProfessionIDs,
        });

        return new ClientSave.Data()
        {
            targetSaveContainer = SaveContainer.player,
            datas = UnlockProfessionList
        };
    }

    /// <summary>
    /// 將伺服器戰鬥角色屬性緩存資料轉成客戶端用的存檔
    /// </summary>
    private ClientSave.Data ConvertSaveToBattleHeroAttrData()
    {
        var battleHeroDatas = new List<INetworkSaveData>();

        // 血量
        battleHeroDatas.Add(new NetworkSaveBattleHeroAttrData() { type = AttrType.Hp, value = fakeServerData.player.actorCache.currentHp });

        // 被動
        foreach (var id in fakeServerData.player.actorCache.passives)
        {
            battleHeroDatas.Add(new NetworkSaveBattleHeroAttrData() { type = AttrType.Passive, id = id });
        }

        // 技能
        foreach (var id in fakeServerData.player.actorCache.skillIds)
        {
            battleHeroDatas.Add(new NetworkSaveBattleHeroAttrData() { type = AttrType.Skill, id = id });
        }

        // 購買道具
        foreach (var id in fakeServerData.player.actorCache.itemList)
        {
            battleHeroDatas.Add(new NetworkSaveBattleHeroAttrData() { type = AttrType.ItemGet, id = id });
        }

        return new ClientSave.Data() { targetSaveContainer = SaveContainer.battleHeroAttr, datas = battleHeroDatas };
    }

    /// <summary>
    /// 將伺服器戰鬥角色屬性緩存資料轉成客戶端用的存檔
    /// </summary>
    private ClientSave.Data ConvertSaveToBattleHeroAttrData(AttrType attrType)
    {
        var battleHeroDatas = new List<INetworkSaveData>();
        switch (attrType)
        {
            case AttrType.Hp:
                {
                    battleHeroDatas.Add(new NetworkSaveBattleHeroAttrData() { type = AttrType.Hp, value = fakeServerData.player.actorCache.currentHp });
                }
                break;
            case AttrType.Passive:
                {
                    foreach (var id in fakeServerData.player.actorCache.passives)
                    {
                        battleHeroDatas.Add(new NetworkSaveBattleHeroAttrData() { type = AttrType.Passive, id = id });
                    }
                }
                break;
            case AttrType.Skill:
                {
                    foreach (var id in fakeServerData.player.actorCache.skillIds)
                    {
                        battleHeroDatas.Add(new NetworkSaveBattleHeroAttrData() { type = AttrType.Skill, id = id });
                    }
                }
                break;
            case AttrType.ItemGet:
                {
                    foreach (var id in fakeServerData.player.actorCache.itemList)
                    {
                        battleHeroDatas.Add(new NetworkSaveBattleHeroAttrData() { type = AttrType.ItemGet, id = id });
                    }
                }
                break;
        }
        return new ClientSave.Data() { targetSaveContainer = SaveContainer.battleHeroAttr, datas = battleHeroDatas };
    }

    /// <summary>
    /// 將伺服器戰鬥角色技能緩存資料轉成客戶端用的存檔
    /// </summary>
    private ClientSave.Data ConvertSaveToBattleSkillData()
    {
        var battleSkillDatas = new List<INetworkSaveData>();
        for (int i = 0; i < fakeServerData.player.actorCache.actorSkills.Count; i++)
        {
            var actorSkill = fakeServerData.player.actorCache.actorSkills[i];
            if (actorSkill.originIndex < 0) continue;
            battleSkillDatas.Add(new NetworkSaveBattleSkillData()
            {
                skillId = actorSkill.skillId,
                isUsed = actorSkill.isUsed,
                index = actorSkill.originIndex,
                currentIdx = i
            });
        }
        return new ClientSave.Data() { targetSaveContainer = SaveContainer.battleSkill, datas = battleSkillDatas };
    }
}