using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SDKProtocol;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public class TestSystem : MonoBehaviour
{
    [Inject]
    UIManager uIManager;
    [Inject]
    FakeServer fakeServer;
    [Inject]
    NetworkSaveManager saveManager;
    [Inject]
    public BattleManager battleManager;
    [Inject]
    public PassiveManager passiveManager;
    [Inject]
    public SkillManager skillManager;
    [Inject]
    ItemManager itemManager;
    [Inject]
    MainFlowController mainFlowController;
    [Inject]
    GameFlowController gameFlowController;
    [Inject]
    PreloadManager preloadManager;
    [Inject]
    DataTableManager dataTableManager;

    [Header("Player Info")]
    [SerializeField]
    TestActor player;
    [SerializeField]
    int monsterIndex;
    [Header("Monster Info")]
    [SerializeField]
    List<TestActor> monster;
    [Header("TriggerEnum")]
    [SerializeField]
    PassiveTriggerEnum triggerEnum;

    [Inject]
    DataManager dataManager;

    private void Awake()
    {
        battleManager.player = player.actor;
        for (int i = 0; i < monster.Count; i++)
        {
            battleManager.monsters.Add(monster[i].actor);
        }
    }

    [InspectorButton]
    void Reload()
    {
        mainFlowController.Reload();
    }

    [InspectorButton]
    void SetData()
    {
        for (int i = 0; i < monster.Count; i++)
        {
            if (i < battleManager.monsters.Count)
            {
                monster[i].actor = battleManager.monsters[i];
                monster[i].gameObject.SetActive(true);
            }
            else
            {
                monster[i].actor.isDead = true;
                monster[i].gameObject.SetActive(false);
            } 
        }
        player.actor = battleManager.player;
        RxEventBus.Register<ActorProfessionEnum>(EventBusEnum.PlayerDataEnum.UpdateSelectTarget, (s) => monsterIndex = (int)s, this);
    }

    [InspectorButton]
    void GetDungeonList()
    {
        var dungeonList = dataTableManager.GetDungeonListDefine(1);
        Debug.Log(dungeonList);
    }
    [InspectorButton]
    async UniTask OpenCharactorPage()
    {
        var ui = await uIManager.OpenUI<UIChooseCharactor>();
        uIManager.LoadingUI(false);
        await uIManager.FadeOut(0.2f);
        List<int> unlockIds = new List<int>();
        unlockIds.Add(1);
        unlockIds.Add(2);
        unlockIds.Add(3);
        Dictionary<ActorProfessionEnum, PlayerProfessionData> dic = new Dictionary<ActorProfessionEnum, PlayerProfessionData>();
        ui.Init(unlockIds, dic);
    }

    [InspectorButton]
    async UniTask OpenCheckSkill()
    {

        var ui = await uIManager.OpenUI<UISkill>();
        var skill = uIManager.FindUI<UISkill>();
        uIManager.LoadingUI(false);
        await uIManager.FadeOut(0.2f);
        await skill.OpenCheckSkillPage(new List<ActorSkill>());
    }

    [InspectorButton]
    async UniTask OpenChooseSkill()
    {

        var ui = await uIManager.OpenUI<UISkill>();
        var skill = uIManager.FindUI<UISkill>();
        uIManager.LoadingUI(false);
        await uIManager.FadeOut(0.2f);
        var skillGroup = new List<SDKProtocol.SkillGroup>();
        var group = new SkillGroup();
        group.Name = "測試資料1";
        group.Skills.Add(5);
        group.Skills.Add(9);
        group.Skills.Add(34);
        group.Skills.Add(38);
        group.Skills.Add(42);
        group.Skills.Add(26);
        group.Skills.Add(30);
        group.Skills.Add(46);
        group.Skills.Add(50);
        skillGroup.Add(group);
        var group2 = new SkillGroup();
        group2.Name = "測試資料2";
        group2.Skills.Add(5);
        group2.Skills.Add(9);
        group2.Skills.Add(34);
        skillGroup.Add(group2);
        var group3 = new SkillGroup();
        group3.Name = "測試資料3";
        group3.Skills.Add(5);
        group3.Skills.Add(9);
        group3.Skills.Add(34);
        skillGroup.Add(group3);
        var group4 = new SkillGroup();
        group4.Name = "測試資料4";
        group4.Skills.Add(5);
        group4.Skills.Add(9);
        group4.Skills.Add(34);
        skillGroup.Add(group4);
        var group5 = new SkillGroup();
        group5.Name = "測試資料5";
        group5.Skills.Add(5);
        group5.Skills.Add(9);
        group5.Skills.Add(34);
        skillGroup.Add(group5);
        var unlockSkill = new List<int>();
        unlockSkill.Add(9);
        unlockSkill.Add(34);
        unlockSkill.Add(38);
        unlockSkill.Add(5);
        unlockSkill.Add(42);
        unlockSkill.Add(26);
        unlockSkill.Add(30);
        unlockSkill.Add(46);
        unlockSkill.Add(50);
        var selectProfessionData = new SelectProfessionData();
        selectProfessionData.SkillGroupsIndex = 4;
        skill.OpenEquipmentSkillPage(skillGroup, unlockSkill, 4);
    }
    [SerializeField]
    private int ChangeID = -1;

    [InspectorButton]
    async UniTask OpenChagneSkill()
    {

        var ui = await uIManager.OpenUI<UISkill>();
        var skill = uIManager.FindUI<UISkill>();
        uIManager.LoadingUI(false);
        await uIManager.FadeOut(0.2f);
        await skill.OpenChangeSkillPage(new List<ActorSkill>(), ChangeID);
        Debug.Log(await skill.IsSkillsChange());
    }


    [InspectorButton]
    async UniTask OPENUIRewardChooseAsync()
    {
        await uIManager.OpenUI<UIBattleRewardChoose>();
        var map = uIManager.FindUI<UIBattleRewardChoose>();
        map.Init(100, 20);
    }

    [InspectorButton]
    async UniTask OPENUIMapAsync()
    {
        await uIManager.OpenUI<UIMap>();
        var map = uIManager.FindUI<UIMap>();
        map.Init();
    }

    [InspectorButton]
    async UniTask OpenAntiqueUiAsync()
    {
        var itemData = itemManager.GetViewItemData(ViewItemType.ItemData, Random.Range(1, 40));
        itemData.coinPrice = 0; // 把價格修改為0，符合預覽

        var antiqueUi = await uIManager.OpenUI<UIAntique>();
        await antiqueUi.Init(new List<ViewItemData>() { itemData }, (passiveData) => { });
        await UniTask.WaitUntil(() => antiqueUi.IsDone);
        uIManager.RemoveUI(antiqueUi);
    }


    [InspectorButton]
    async UniTask OpenRestUiAsync()
    {
        var restUi = await uIManager.OpenUI<UIRest>();
        await restUi.Init(0, 0);
        restUi.ShowEffect();
        var battle = uIManager.FindUI<UIBattle>();
        if (battle != null)
        {
            await battle.OnStartRecover();
        }
        await restUi.RecoverFinish();
    }

    [InspectorButton]
    async Task OpenChestUiAsync()
    {
        ViewItemData skill = null;
        // 隨機給假獎勵
        var itemDataList = new List<ViewItemData>();
        for (int i = 0; i < 3; i++)
        {
            //var itemData = ColneViewItemData(itemManager.GetViewItemData(Random.Range(1, 40)));
            var itemData = itemManager.GetViewItemData(ViewItemType.ItemData, 7);
            itemData.coinPrice = 0; // 把價格修改為0，符合預覽

            itemDataList.Add(itemData);
        }

        var chestUi = await uIManager.OpenUI<UIChest>();
        await chestUi.Init(itemDataList);

        await UniTask.WaitUntil(() => { return chestUi.IsDone; });

        if (skill != null)
        {
            var ui = await uIManager.OpenUI<UISkill>();
            var uiSkill = uIManager.FindUI<UISkill>();
            uIManager.LoadingUI(false);
            await uIManager.FadeOut(0.2f);
            await uiSkill.OpenChangeSkillPage(new List<ActorSkill>(), skill.arg);
        }

        uIManager.RemoveUI(chestUi);
    }

    [InspectorButton]
    async Task OpenRenameUiAsync()
    {
        var renameUi = await uIManager.OpenUI<UIRenameBox>();
        await renameUi.Init((name) =>
        {
            Debug.Log($"name: {name}");
        });
        await UniTask.WaitUntil(() => { return renameUi.IsDone; });
        uIManager.RemoveUI(renameUi);
    }

    [InspectorButton]
    async Task OpenSimpleRankingUiAsync()
    {
        // 測試用資料，之後會從存檔或Server給予
        var maxRanking = 20;
        var maxStage = 10;
        var fullRankingData = new List<DungeonRankingDataItem>();
        DungeonRankingDataItem playerRankingData = null;
        var myRanking = Random.Range(1, maxRanking);
        for (int i = 0; i < maxRanking; i++)
        {
            var stageProgress = Random.Range(1, maxStage); // 可能破關的的關卡數
            var spendTime = 0f;
            for (int j = 1; j <= stageProgress; j++)
            {
                spendTime += 60 * 3 * 1000 * Random.Range(0.8f, 1.2f); // 假設一關要3分鐘
            }
            var data = new DungeonRankingDataItem()
            {
                stageProgress = stageProgress,
                spendTime = (int)spendTime,
                profession = ActorProfessionEnum.Witch,
                playerName = $"玩家{i + 1}"
            };
            fullRankingData.Add(data);
        }

        fullRankingData = fullRankingData.OrderByDescending(x => x.stageProgress).ThenBy(x => x.spendTime).ToList();

        for (int i = 0; i < maxRanking; i++)
        {
            fullRankingData[i].ranking = i + 1;
            if (myRanking == i + 1)
            {
                playerRankingData = fullRankingData[i];
            }
        }

        var rankingUi = await uIManager.OpenUI<UIRanking>();
        await rankingUi.Init(fullRankingData, playerRankingData, () =>
        {
            Debug.Log("UIRanking OnClose");
        });
        await UniTask.WaitUntil(() => { return rankingUi.IsDone; });
        uIManager.RemoveUI(rankingUi);
    }

    [InspectorButton]
    async Task OpenBattleResultRankingUiAsync()
    {
        var maxStage = 10;
        var stageProgress = Random.Range(1, maxStage); // 可能破關的的關卡數
        var spendTime = 0f;
        for (int j = 1; j <= stageProgress; j++)
        {
            spendTime += 60 * 3 * 1000 * Random.Range(0.8f, 1.2f); // 假設一關要3分鐘
        }
        var playerName = "";

        // open remane box ui
        var renameUi = await uIManager.OpenUI<UIRenameBox>();
        await renameUi.Init(onConfirm: (name) =>
        {
            playerName = name;
            Debug.Log($"name: {name}");
        }, onCancel: (name) =>
        {
            playerName = name;
            Debug.Log($"name: {name}");
        });
        await UniTask.WaitUntil(() => { return renameUi.IsDone; });
        uIManager.RemoveUI(renameUi);

        // build battle end request data
        var battleEndData = new BattleEndData()
        {
            stageProgress = stageProgress,
            spendTime = (int)spendTime,
            profession = ActorProfessionEnum.Witch,
            playerName = playerName
        };

        // client battle end -> server -> response result
        var battleResultData = await fakeServer.BattleResult(battleEndData);

        // open ranking ui
        var rankingUi = await uIManager.OpenUI<UIRanking>();
        await rankingUi.Init(battleResultData.ranking.fullRankings, battleResultData.ranking.lastRanking, () =>
        {
            Debug.Log("UIRanking OnClose");
        });
        await UniTask.WaitUntil(() => { return rankingUi.IsDone; });
        uIManager.RemoveUI(rankingUi);
    }

    [InspectorButton]
    async Task OpenShopUiAsync()
    {
        // 隨機給假獎勵
        var itemDataList = new List<ViewItemData>();
        for (int i = 0; i < 4; i++)
        {
            var itemData = itemManager.GetViewItemData(ViewItemType.ItemData, Random.Range(1, 40));
            itemData.ItemTypeStr = "商店";

            itemDataList.Add(itemData);
        }

        var shopUi = await uIManager.OpenUI<UIShop>();
        if (shopUi != null)
        {
            await shopUi.Init(itemDataList);
            await UniTask.WaitUntil(() => { return shopUi.IsDone; });
            uIManager.RemoveUI(shopUi);
        }
    }

    [InspectorButton]
    void PlayerEnd()
    {
        gameFlowController.SwichGameStateByPerformanceData(GameFlowController.GameFlowState.MonsterRound);
        SetData();
    }
    [InspectorButton]
    async void PreloadAll()
    {
        var st = preloadManager.PreloadSkillPrefab(ActorProfessionEnum.Witch);
        var pt = preloadManager.PreloadPassiveAll();
        await UniTask.WhenAll(st, pt);
        Debug.Log("PreloadAll Done");
    }

    List<Dictionary<SkillCostColorEnum, int>> SetCost(params List<SkillCostColorData>[] costs)
    {
        var ls = new List<Dictionary<SkillCostColorEnum, int>>();
        for (int i = 0; i < costs.Length; i++)
        {
            var cost = costs[i];
            var dic = new Dictionary<SkillCostColorEnum, int>();
            for (int j = 0; j < cost.Count; j++)
            {
                var c = cost[j];
                var have = dic.TryGetValue(c.colorEnum, out int v);
                if (have) dic.Remove(c.colorEnum);
                dic.Add(c.colorEnum, v + c.count);
            }
            ls.Add(dic);
        }
        return ls;
    }

    public void Attack(TestActor sender)
    {
        var battleData = new BattleData();
        battleData.isSkill = true;
        if (sender.actor.isPlayer)
        {
            battleData.costs.Clear();
            battleData.costs.AddRange(SetCost(sender.cost1, sender.cost2, sender.cost3, sender.cost4));
            if (monsterIndex > monster.Count) monsterIndex = 0;
            battleData.selectTarget = monster[monsterIndex].actor;
        }
        else
        {
            battleData.selectTarget = player.actor;
        }
        var skillData = new ActorSkill();
        skillData.skillId = sender.skillId;
        skillData.isUsed = true;
        skillData.originIndex = -1;

        battleData.originSkill = skillData;
        battleData.skillId = sender.skillId;
        battleData.sender = sender.actor;

        sender.actor.lastBattleData = battleData;
        skillManager.OnSkill(battleData);

    }


    [InspectorButton("PlayerAndMonsterTrigger")]
    public void Trigger()
    {
        var battleData = new BattleData();
        battleData.isSkill = false;
        passiveManager.OnActorPassive(player.actor, triggerEnum, battleData);
        for (int i = 0; i < monster.Count; i++)
        {
            passiveManager.OnActorPassive(monster[i].actor, triggerEnum, battleData);
        }
    }

    [InspectorButtonAttribute("[FakeServer] 儲存假Server檔案")]
    void SaveFakeServerData()
    {
        fakeServer.SaveFakeServerData();
    }

    [InspectorButtonAttribute("[FakeServer] 讀取假Server檔案")]
    void LoadFakeServerData()
    {
        fakeServer.LoadFakeServerData();
    }

    [InspectorButtonAttribute("[FakeServer] 刪除假Server檔案")]
    void DeleteFakeServerData()
    {
        fakeServer.DeleteFakeServerData();
    }

    [InspectorButtonAttribute("[NetworkSave] 測試Newtonsoft的JsonObject")]
    void TestJsonObject()
    {
        var list = new List<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        var array = new int[3];
        array[0] = 1;
        array[1] = 2;
        array[2] = 3;

        var dict = new Dictionary<string, int>();
        dict.Add("1", 3001);
        dict.Add("2", 3002);
        dict.Add("3", 3003);

        var testClass = new TestClass();
        testClass.id = 1001;
        testClass.count = 10;
        testClass.token = "i am a token";
        testClass.subClass = new TestClass.SubTestClass()
        {
            heroId = 2001
        };

        var valueJsonObject = new JsonObject();
        valueJsonObject.Add("Key100", "Value999");

        var key = "testKey";
        var jsonObject = new JsonObject();
        jsonObject.Add(key, null);

        // jsonObject: {"testKey":null}
        Debug.Log($"jsonObject: {jsonObject.ToString(Formatting.None)}");

        var isExistKey = jsonObject.ContainsKey(key);

        // isExistKey: True
        Debug.Log($"isExistKey: {isExistKey}");

        var propertyJsonObject = new JsonObject();
        propertyJsonObject.Add("string", "abc");
        propertyJsonObject.Add("int", 123);
        propertyJsonObject.Add("bool", true);
        propertyJsonObject.Add("list", list);
        propertyJsonObject.Add("array", array);
        propertyJsonObject.Add("dict", dict);
        propertyJsonObject.Add("class", testClass);
        propertyJsonObject.Add("jsonObject", valueJsonObject);
        jsonObject[key] = propertyJsonObject;

        // added property of jsonObject
        //{
        //    "testKey": {
        //        "string": "abc",
        //        "int": 123,
        //        "bool": true,
        //        "list": [
        //          1,
        //          2,
        //          3
        //        ],
        //        "array": [
        //          1,
        //          2,
        //          3
        //        ],
        //        "dict": {
        //          "1": 3001,
        //          "2": 3002,
        //          "3": 3003
        //        },
        //        "class": {
        //          "id": 1001,
        //          "count": 10,
        //          "token": "i am a token",
        //          "subClass": {
        //            "heroId": 2001
        //          }
        //                    },
        //        "jsonObject": {
        //          "Key100": "Value999"
        //        }
        //    }
        //}
        Debug.Log($"added property of jsonObject: {jsonObject.ToString(Formatting.None)}");
    }

    [InspectorButtonAttribute("[NetworkSave] 測試假Server回傳時初始化資料")]
    void TestOnServerResponse_Init()
    {
        // 製作伺服器給客戶端的封包內容
        var cmd = "init";
        var playerContainerName = "player";
        var itemContainerName = "item";

        var containerJsonObject = new JsonObject(); // 目標container的JsonObject
        var playerContainerList = new List<NetworkSavePlayerData>();
        playerContainerList.Add(new NetworkSavePlayerData() { uid = "NI2XZd", username = "派大星" });
        containerJsonObject.Add(playerContainerName, playerContainerList); // 該container的資料

        var itemContainerList = new List<NetworkSaveBattleItemData>();
        itemContainerList.Add(new NetworkSaveBattleItemData() { id = 1, count = 10 });
        itemContainerList.Add(new NetworkSaveBattleItemData() { id = 2, count = 5 });
        itemContainerList.Add(new NetworkSaveBattleItemData() { id = 3, count = 8 });
        containerJsonObject.Add(itemContainerName, itemContainerList); // 該container的資料

        var responseJsonObject = new JsonObject(); // 最終回傳的JsonObject
        responseJsonObject.Add(cmd, containerJsonObject);

        Debug.Log($"TestOnServerResponse_Init: {responseJsonObject.ToString(Formatting.None)}");

        saveManager.OnServerResponse(responseJsonObject);

        Debug.Log($"現在客戶端儲存的伺服器內容: {saveManager.ToJson()}");
    }

    [InspectorButtonAttribute("[NetworkSave] 測試假Server回傳時更新資料")]
    void TestOnServerResponse_Update()
    {
        // 製作伺服器給客戶端的封包內容
        var cmd = "update";
        var playerContainerName = "player";
        var itemContainerName = "item";

        var containerJsonObject = new JsonObject(); // 目標container的JsonObject
        var playerContainerList = new List<NetworkSavePlayerData>();
        playerContainerList.Add(new NetworkSavePlayerData() { uid = "NI2XZd", username = "超級派大星" });
        containerJsonObject.Add(playerContainerName, playerContainerList); // 該container的資料

        var itemContainerList = new List<NetworkSaveBattleItemData>();
        itemContainerList.Add(new NetworkSaveBattleItemData() { id = 1, count = 21 });
        itemContainerList.Add(new NetworkSaveBattleItemData() { id = 2, count = 7 });
        itemContainerList.Add(new NetworkSaveBattleItemData() { id = 3, count = 5 });
        itemContainerList.Add(new NetworkSaveBattleItemData() { id = 99, count = 10 });
        containerJsonObject.Add(itemContainerName, itemContainerList); // 該container的資料

        var responseJsonObject = new JsonObject(); // 最終回傳的JsonObject
        responseJsonObject.Add(cmd, containerJsonObject);

        Debug.Log($"TestOnServerResponse_Update: {responseJsonObject.ToString(Formatting.None)}");

        saveManager.OnServerResponse(responseJsonObject);

        Debug.Log($"現在客戶端儲存的伺服器內容: {saveManager.ToJson()}");
    }

    [InspectorButtonAttribute("[NetworkSave] 測試假Server回傳時刪除資料")]
    void TestOnServerResponse_Remove()
    {
        // 製作伺服器給客戶端的封包內容
        var cmd = "remove";
        var itemContainerName = "item";

        var containerJsonObject = new JsonObject(); // 目標container的JsonObject

        var removeKeyList = new List<string>();
        removeKeyList.Add("2");
        removeKeyList.Add("3");
        removeKeyList.Add("4");
        containerJsonObject.Add(itemContainerName, removeKeyList); // 要移除的key

        var responseJsonObject = new JsonObject(); // 最終回傳的JsonObject
        responseJsonObject.Add(cmd, containerJsonObject);

        Debug.Log($"TestOnServerResponse_Remove: {responseJsonObject.ToString(Formatting.None)}");

        saveManager.OnServerResponse(responseJsonObject);

        Debug.Log($"現在客戶端儲存的伺服器內容: {saveManager.ToJson()}");
    }

    [InspectorButtonAttribute("[NetworkSave] 測試使用NetworkSave內容")]
    void TestUseSaveData()
    {
        var item1 = saveManager.GetContainer<NetworkSaveBattleItemContainer>().GetData(1);
        var item2 = saveManager.GetContainer<NetworkSaveBattleItemContainer>().GetData(2);
        var item3 = saveManager.GetContainer<NetworkSaveBattleItemContainer>().GetData(3);

        Debug.Log($"目前擁有道具{GetLog(1, item1)}, {GetLog(2, item2)}, {GetLog(3, item3)}");

        string GetLog(int id, NetworkSaveBattleItemData data)
        {
            var table = dataTableManager.GetItemDataDefine(id);
            var name = table?.name;
            var count = (data != null) ? data.count : 0;
            return $"{name}*{count}";
        }
    }

    [InspectorButtonAttribute("[NetworkSave] 印出所有存檔")]
    void PrintAllNetworkSaveData()
    {
        Debug.Log($"現在客戶端儲存的伺服器內容: {saveManager.ToJson()}");
    }

    [InspectorButtonAttribute("[Battle] 擊殺所有怪物")]
    void KillAllMonster()
    {
        battleManager.KillAllMonster();
    }

    [InspectorButtonAttribute("[Battle] 戰鬥速度x1")]
    void BattleSpeedX1()
    {
        Time.timeScale = 1;
    }

    [InspectorButtonAttribute("[Battle] 戰鬥速度x2")]
    void BattleSpeedX2()
    {
        Time.timeScale = 2;
    }

    [InspectorButtonAttribute("[Battle] 戰鬥速度x10")]
    void BattleSpeedX10()
    {
        Time.timeScale = 10;
    }

    [InspectorButtonAttribute("[Battle] 添加金幣100")]
    async Task BattleGainCoin100()
    {
        await fakeServer.BattleGainItem(NetworkSaveBattleItemContainer.COIN, 100);
    }

    void LogParse(object obj)
    {
        var str = JsonConvert.SerializeObject(obj);
        Debug.Log(str);
    }


    private void OnDestroy()
    {
        RxEventBus.UnRegister(this);
    }

    private class TestClass
    {
        public class SubTestClass
        {
            public int heroId;
        }

        public int id;
        public int count;
        public string token;
        public SubTestClass subClass;
    }
}
