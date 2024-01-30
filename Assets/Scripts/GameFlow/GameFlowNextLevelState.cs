using Cysharp.Threading.Tasks;
using SDKProtocol;
using System.Collections.Generic;
using Zenject;

public class GameFlowNextLevelState : BaseState<GameFlowController, GameFlowController.GameFlowState>
{
    [Inject]
    IProtocolBridge sdk;
    [Inject]
    DataManager dataManager;
    [Inject]
    BattleManager battleManager;
    [Inject]
    UIManager uIManager;
    [Inject]
    EnvironmentManager environmentManager;
    [Inject]
    DataTableManager dataTableManager;
    [Inject]
    NetworkSaveManager saveManager;

    [Inject]
    PreloadManager preloadManager;

    public override UniTask End()
    {
        battleManager.StopTimer();
        return default;
    }

    public override void OnAbort()
    {

    }

    public override async UniTask Start()
    {
        battleManager.StartTimer();
        var container = saveManager.GetContainer<NetworkSaveBattleDungeonContainer>();
        await sdk.BattleGainHeroAttr(battleManager.player.currentHp);
        await openUIMapAsync();
        await UniTask.WaitUntilValueChanged(container, x => x.FightDungeonId);

        //選完轉場
        var fightDungeonId = container.FightDungeonId;
        var currentDefine = dataTableManager.GetDungeonDataDefine(fightDungeonId);

        var nextData = container.LastCache[dataManager.GetCurrentDungeonLeveIndex() - 1][0];
        var nextDungeoData = container.LastCache[container.LastCache.Count - 1];

        // 載入下一個地形
        if (nextDungeoData.Count > 0)
        {
            if (dataTableManager.GetDungeonDataDefine(nextDungeoData[0].dungeonId).sceneId == currentDefine.sceneId)
                environmentManager.SetNextScene(nextDungeoData[0]);
        }
        // 判斷是否要轉場
        var isChangeScene = currentDefine.sceneId != dataTableManager.GetDungeonDataDefine(nextData.dungeonId).sceneId;

        // 更新下一關的 acquisitionItems
        if (dataManager.GetCurrentDungeonLeveData().nodeEnum == MapNodeEnum.Monster || dataManager.GetCurrentDungeonLeveData().nodeEnum == MapNodeEnum.EliteMonster || dataManager.GetCurrentDungeonLeveData().nodeEnum == MapNodeEnum.Boss)
        {
            await sdk.BattleGetMonsterAcquistion();
        }
        else
        {
            await sdk.BattleGetEventAcquistion();
        }

        var doungeonData = dataManager.GetCurrentDungeonLeveData();


        // 預載下一關可能會獲得的道具 Icon
        var tLs = new List<UniTask>();
        for (int i = 0; i < doungeonData.acquisitionItems.Count; i++)
        {
            var itemList = doungeonData.acquisitionItems[i];
            if (itemList != null && itemList.Count > 0)
            {
                for (int j = 0; j < itemList.Count; j++)
                {
                    tLs.Add(preloadManager.PreloadItemData(itemList[j].id));
                }
            }
        }
        await UniTask.WhenAll(tLs);

        // 重新設置玩家資料
        battleManager.ResetPlayerActor();
        if (doungeonData.nodeEnum == MapNodeEnum.Chest)
            GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.Chest);
        else if (doungeonData.nodeEnum == MapNodeEnum.Store)
            GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.Store);
        else if (doungeonData.nodeEnum == MapNodeEnum.Antique)
            GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.GetAntique);
        else if (doungeonData.nodeEnum == MapNodeEnum.Rest)
            GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.Rest);
        else
        {
            // 設置怪物
            battleManager.monsters.Clear();
            await battleManager.SetMonsterActor(doungeonData.monsterPosAndId, doungeonData.acquisitionItems);
            environmentManager.SetMonsterPos(battleManager.monsters, doungeonData.nodeEnum == MapNodeEnum.EliteMonster);
            GetController().UpdatePassiveData(battleManager.player);
            for (int i = 0; i < battleManager.monsters.Count; i++)
            {
                GetController().UpdatePassiveData(battleManager.monsters[i]);
            }
            GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.StartFight);
            if (isChangeScene)
            {
                var doungeonDefine = dataTableManager.GetDungeonDataDefine(doungeonData.dungeonId);
                var sceneDefine = dataTableManager.GetSceneDataDefine(doungeonDefine.sceneId);
                GetController().AddPerformanceData(new PUIAnimatonStateData { stateEnum = UIBattle.UIAnimatonStateEnum.SceneName, autoClose = true, sprite = sceneDefine.sceneNameSprite });
            }
        }
        var ui = uIManager.FindUI<UIBattle>();
        ui.SetTitle(doungeonData.dungeonId);
        // 表演轉場或是移動相機
        if (isChangeScene)
        {
            await uIManager.FadeIn(0.5f);
            environmentManager.RestScene();
            var index = dataManager.GetCurrentDungeonLeveIndex();
            var count = container.LastCache.Count - index;
            var ls = container.LastCache.GetRange(index, count);
            for (int i = 0; i < ls.Count; i++)
            {
                environmentManager.SetNextScene(ls[i][0]);
            }
            await uIManager.FadeOut(0.5f);
        }
        else
        {
            await environmentManager.MoveScene();
        }
    }
    private async UniTask openUIMapAsync()
    {
        await uIManager.OpenUI<UIMap>();
        var map = uIManager.FindUI<UIMap>();
        map.Init();
    }



    public override void Update()
    {

    }
}
