using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SDKProtocol;
using System.Collections.Generic;
using System.Linq;
using Debug = UnityEngine.Debug;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public partial class FakeServer
{
    private List<List<DungeonLevelData>> GetProtoDungeonData(int count)
    {
        var levelLs = new List<List<DungeonLevelData>>();
        var groups = dataTableManager.GetDungeonDataDefines(fakeServerData.player.dungeonCache.dungeonGroupId);
        var max = fakeServerData.player.dungeonCache.dungeonDataIndex + count;
        var amount = max >= groups.Count ? groups.Count : max;
        for (int i = fakeServerData.player.dungeonCache.dungeonDataIndex; i < amount; i++)
        {
            var ls = new List<DungeonLevelData>();
            var levels = groups[i];
            for (int k = 0; k < levels.Count; k++)
            {
                var mapData = GetDungeonDataList(levels[k]);
                ls.Add(mapData);
            }
            levelLs.Add(ls);
            fakeServerData.player.dungeonCache.dungeonDataIndex++;
        }

        return levelLs;
    }

    private DungeonLevelData GetDungeonDataList(DungeonDataDefine groupData)
    {
        var r = UnityEngine.Random.Range(0, 100);
        var total = 0;
        var ls = new List<DungeonLevelData>();
        for (int j = 0; j < groupData.mapProbabilityDatas.Count; j++)
        {
            var mapP = groupData.mapProbabilityDatas[j];
            total += mapP.probability;
            if (r < total)
            {
                var mapData = new DungeonLevelData();
                mapData.dungeonId = groupData.id;
                mapData.mapNodeEnum = (int)mapP.nodeEnum;
                switch (mapP.nodeEnum)
                {
                    case MapNodeEnum.Monster:
                    case MapNodeEnum.EliteMonster:
                    case MapNodeEnum.Boss:
                        mapData.passives = groupData.scenePassiveIds;
                        var rPos = UnityEngine.Random.Range(0, mapP.positions.Count);
                        var pos = new List<int>(mapP.positions[rPos]);
                        ConvertMonsterPos(pos);
                        mapData.monsterPosAndId = pos;
                        /*
                        for (int k = 0; k < mapData.monsterPosAndId.Count; k++)
                        {
                            if (mapData.monsterPosAndId[k] > 0)
                            {
                                var mDefine = dataTableManager.GetMonsterDefine(mapData.monsterPosAndId[k]);
                                mapData.acquisitionItems.Add(itemManager.GetItmes(mDefine.dropGroupId, mDefine.dropCount));
                            }
                            else mapData.acquisitionItems.Add(null);
                        }
                        */
                        mapData.threeSelectCoin = groupData.selectCoin;
                        var skills = new List<int>(dataTableManager.GetProfessionDataDefine(fakeServerData.player.dungeonCache.professionEnum).selectSkills[groupData.selectSkillIndex]);
                        var selectSkillIds = new List<int>();
                        var loopCount = 0;
                        while (selectSkillIds.Count < 3 && loopCount < 100)
                        {
                            var skillR = UnityEngine.Random.Range(0, skills.Count);
                            // TODO ���ˬd
                            selectSkillIds.Add(skills[skillR]);
                        }
                        mapData.threeSelectSkillIds = selectSkillIds;
                        break;
                    /*case MapNodeEnum.Chest:
                    case MapNodeEnum.Store:
                    case MapNodeEnum.Antique:
                        var itmeLs = itemManager.GetItmes(mapP.dropGroupId, mapP.dropCount);
                        mapData.acquisitionItems.Add(itmeLs);
                        break;*/
                    case MapNodeEnum.Rest:
                        break;
                    default:
                        break;
                }
                return mapData;
            }
        }
        return null;
    }
    private bool UpdateMonsterAcquistionList()
    {
        var dungeonLevelDatas = GetCurrentDungeonLeveData();

        int outerIndex = fakeServerData.player.dungeonCache.lastCache
              .FindIndex(d => d.Find(dd => fakeServerData.player.dungeonCache.fightDungeonId == dd.dungeonId) != null);
        if (outerIndex == -1)
            return false;
        int innerIndex = fakeServerData.player.dungeonCache.lastCache[outerIndex]
            .FindIndex(dd => fakeServerData.player.dungeonCache.fightDungeonId == dd.dungeonId);
        if (innerIndex == -1)
            return false;

        fakeServerData.player.dungeonCache.lastCache[outerIndex][innerIndex].acquisitionItems.Clear();

        for (int k = 0; k < dungeonLevelDatas.monsterPosAndId.Count; k++)
        {
            if (dungeonLevelDatas.monsterPosAndId[k] > 0)
            {
                var mDefine = dataTableManager.GetMonsterDefine(dungeonLevelDatas.monsterPosAndId[k]);
                fakeServerData.player.dungeonCache.lastCache[outerIndex][innerIndex].acquisitionItems.Add(itemManager.GetItmes(mDefine.dropGroupId, mDefine.dropCount));
            }
            else fakeServerData.player.dungeonCache.lastCache[outerIndex][innerIndex].acquisitionItems.Add(null);
        }
        return true;
    }

    private bool UpdateEvetAcquisitionList()
    {
        var dungeonLevelDatas = GetCurrentDungeonLeveData();

        var dungeonDataDefine = dataTableManager.GetDungeonDataDefine(dungeonLevelDatas.dungeonId);
        MapProbabilityData currentMapData = new MapProbabilityData();
        foreach (var mapProbabilityData in dungeonDataDefine.mapProbabilityDatas)
        {
            if (dungeonLevelDatas.nodeEnum == mapProbabilityData.nodeEnum)
            {
                currentMapData = mapProbabilityData;
            }
        }

        var itmeLs = itemManager.GetItmes(currentMapData.dropGroupId, currentMapData.dropCount);
        int outerIndex = fakeServerData.player.dungeonCache.lastCache
            .FindIndex(d => d.Find(dd => fakeServerData.player.dungeonCache.fightDungeonId == dd.dungeonId) != null);
        if (outerIndex == -1)
            return false;
        int innerIndex = fakeServerData.player.dungeonCache.lastCache[outerIndex]
            .FindIndex(dd => fakeServerData.player.dungeonCache.fightDungeonId == dd.dungeonId);
        if (innerIndex == -1)
            return false;
        fakeServerData.player.dungeonCache.lastCache[outerIndex][innerIndex].acquisitionItems.Clear();
        fakeServerData.player.dungeonCache.lastCache[outerIndex][innerIndex].acquisitionItems.Add(itmeLs);

        return true;
    }

    void ConvertMonsterPos(List<int> pos)
    {
        for (int k = 0; k < pos.Count; k++)
        {
            if (pos[k] == 0) continue;
            var monsterGroupData = dataTableManager.GetMonsterGroupDefine(pos[k]);
            var mgr = UnityEngine.Random.Range(0, 100);
            var mgTotal = 0;
            for (int l = 0; l < monsterGroupData.groupDatas.Count; l++)
            {
                var monsterGroupD = monsterGroupData.groupDatas[l];
                mgTotal += monsterGroupD.possibility;
                if (mgr < mgTotal)
                {
                    pos[k] = monsterGroupD.monsterId;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 避免跟 client (DataManager) 混用資料，所以還是另外寫一個以 server 自己資料為主
    /// </summary>
    private DungeonLevelData GetCurrentDungeonLeveData()
    {
        var dungeonData = fakeServerData.player.dungeonCache;
        foreach (var levelLs in dungeonData.lastCache)
        {
            foreach (var level in levelLs)
            {
                if (level.dungeonId == dungeonData.fightDungeonId) return level;
            }
        }
        return null;
    }
}