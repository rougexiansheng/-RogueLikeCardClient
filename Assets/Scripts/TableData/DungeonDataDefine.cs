using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

public class DungeonDataDefine
{
    public int id;
    public int groupId;
    public int mapLayer;
    public int UIPosition;
    public int sceneId;
    /// <summary>
    /// 依照職業表 3選1技能 表格填寫從1開始,實際使用從0開始
    /// </summary>
    public int selectSkillIndex;
    public int selectCoin;
    public List<string> pathNodes = new List<string>();
    public List<int> scenePassiveIds = new List<int>();
    public List<MapProbabilityData> mapProbabilityDatas = new List<MapProbabilityData>();
    public GameObject sceneObj;
}

public struct MapProbabilityData
{
    public MapNodeEnum nodeEnum;
    public int probability;
    /// <summary>
    /// 怪物位置
    /// </summary>
    public List<List<int>> positions;
    /// <summary>商店/遺物/寶箱 掉落ID</summary>
    public int dropGroupId;
    /// <summary>-1為全拿</summary>
    public int dropCount;
}

public class DungeonOriginDataDefine : OriginDefineBase<DungeonDataDefine>
{
    public int groupId;
    public string node;
    public string bgm;
    public int sceneId;
    public string scenePassiveIds;
    public int selectSkillIndex;
    public int selectCoin;

    public string monsterPos;
    public int monsterProbability;

    public string eliteMonsterPos;
    public int eliteMonsterProbability;

    public string bossPos;
    public int bossProbability;

    public int chestDropGroupId;
    public int chestProbability;
    public int chestCount;

    public int storeDropGroupId;
    public int storeProbability;
    public int storeCount;

    public int antiqueDropGroupId;
    public int antiqueProbability;
    public int antiqueCount;

    public int restProbability;
    public string pathNodes;
    public int coin;

    public override DungeonDataDefine ParseData()
    {
        var d = new DungeonDataDefine();
        d.id = id;
        d.groupId = groupId;
        d.mapLayer = int.Parse(node.Split('-')[0]);
        d.UIPosition = int.Parse(node.Split('-')[1]);
        d.sceneId = sceneId;
        d.selectSkillIndex = selectSkillIndex - 1;
        d.selectCoin = selectCoin;
        if (!string.IsNullOrEmpty(pathNodes)) d.pathNodes.AddRange(pathNodes.Split(',').ToList());
        if (!string.IsNullOrEmpty(scenePassiveIds)) d.scenePassiveIds.AddRange(scenePassiveIds.Split(',').ToList().ConvertAll(s => string.IsNullOrEmpty(s) ? 0 : int.Parse(s)));
        if (monsterProbability != 0)
        {
            var pData = new MapProbabilityData();
            pData.nodeEnum = MapNodeEnum.Monster;
            pData.probability = monsterProbability;
            pData.positions = ParesStringToList(monsterPos);
            d.mapProbabilityDatas.Add(pData);
        }

        if (eliteMonsterProbability != 0)
        {
            var pData = new MapProbabilityData();
            pData.nodeEnum = MapNodeEnum.EliteMonster;
            pData.probability = eliteMonsterProbability;
            pData.positions = ParesStringToList(eliteMonsterPos);
            d.mapProbabilityDatas.Add(pData);
        }

        if (bossProbability != 0)
        {
            var pData = new MapProbabilityData();
            pData.nodeEnum = MapNodeEnum.Boss;
            pData.probability = bossProbability;
            pData.positions = ParesStringToList(bossPos);
            d.mapProbabilityDatas.Add(pData);
        }

        if (chestProbability != 0)
        {
            var pData = new MapProbabilityData();
            pData.nodeEnum = MapNodeEnum.Chest;
            pData.probability = chestProbability;
            pData.dropGroupId = chestDropGroupId;
            pData.dropCount = chestCount;
            d.mapProbabilityDatas.Add(pData);
        }

        if (storeProbability != 0)
        {
            var pData = new MapProbabilityData();
            pData.nodeEnum = MapNodeEnum.Store;
            pData.probability = storeProbability;
            pData.dropGroupId = storeDropGroupId;
            pData.dropCount = storeCount;
            d.mapProbabilityDatas.Add(pData);
        }

        if (antiqueProbability != 0)
        {
            var pData = new MapProbabilityData();
            pData.nodeEnum = MapNodeEnum.Antique;
            pData.probability = antiqueProbability;
            pData.dropGroupId = antiqueDropGroupId;
            pData.dropCount = antiqueCount;
            d.mapProbabilityDatas.Add(pData);
        }

        if (restProbability != 0)
        {
            var pData = new MapProbabilityData();
            pData.nodeEnum = MapNodeEnum.Rest;
            pData.probability = restProbability;
            d.mapProbabilityDatas.Add(pData);
        }
        return d;
    }

    public override int GetGroupId()
    {
        return groupId;
    }

    List<List<int>> ParesStringToList(string str)
    {
        var ls = new List<List<int>>();
        var have = false;
        int i = 0;
        do
        {
            var r = BetweenStr(str, "[", "]");
            have = r.have;
            if (have)
            {
                str = r.str;
                ls.Add(JsonConvert.DeserializeObject<List<int>>(r.result));
            }
        }
        while (have && i < 100000);
        return ls;
    }

    (bool have, string result, string str) BetweenStr(string str, string first, string last)
    {
        var ap = str.IndexOf(first);
        if (ap == -1) return (false, str, null);
        var bp = str.Substring(ap).IndexOf(last);
        if (bp == -1) return (false, str, null);
        var r = str.Substring(ap, bp + first.Length);
        var rr = str.Substring(bp + ap + first.Length);
        return (true, r, rr);
    }
}
