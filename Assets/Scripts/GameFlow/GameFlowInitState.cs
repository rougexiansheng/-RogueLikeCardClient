using Cysharp.Threading.Tasks;
using SDKProtocol;
using System.Collections.Generic;
using Zenject;

public class GameFlowInitState : BaseState<GameFlowController, GameFlowController.GameFlowState>
{
    [Inject]
    IProtocolBridge sdk;
    [Inject]
    DataManager dataManager;
    [Inject]
    AssetManager assetManager;
    [Inject]
    BattleManager battleManager;
    [Inject]
    UIManager uIManager;
    [Inject]
    EnvironmentManager environmentManager;
    [Inject]
    PassiveManager passiveManager;
    [Inject]
    PreloadManager preloadManager;
    [Inject]
    DataTableManager dataTableManager;
    [Inject]
    NetworkSaveManager saveManager;

    public async override UniTask End()
    {
        uIManager.LoadingUI(false);
        await uIManager.FadeOut(0.5f);
    }

    public override void OnAbort()
    {

    }

    public override async UniTask Start()
    {
        battleManager.ResetTiemr();

        GetController().ActivePerformance(true);
        GetController().InitPerformanceCallBack();
        await UniTask.WhenAll(
            // 預先載入UI
            uIManager.PreloadUI(typeof(UITargetInfo),
            typeof(UIBattle),
            typeof(UISkillPopupInfoPage),
            typeof(UIBattleRewardChoose),
            typeof(UIMap)
            ),
            // 預載UIItem物件
            assetManager.LoadAndSetInObjectPool<UIJumpHpText>(),
            assetManager.LoadAndSetInObjectPool<UISkillBattleItem>(),
            assetManager.LoadAndSetInObjectPool<UIStateItem>(),
            assetManager.LoadAndSetInObjectPool<UIItemIcon>()
            );


        var dungeonCache = saveManager.GetContainer<NetworkSaveBattleDungeonContainer>();
        await preloadManager.PreLoadSceneObject(dungeonCache.DungeonGroupId);
        // 設置場景
        for (int i = 0; i < dungeonCache.LastCache.Count; i++)
        {
            environmentManager.SetNextScene(dungeonCache.LastCache[i][0]);
        }
        var ui = await uIManager.OpenUI<UIBattle>();

        // 更新第一關的 acquisitionItems
        if (dataManager.GetCurrentDungeonLeveData().nodeEnum == MapNodeEnum.Monster || dataManager.GetCurrentDungeonLeveData().nodeEnum == MapNodeEnum.EliteMonster || dataManager.GetCurrentDungeonLeveData().nodeEnum == MapNodeEnum.Boss)
        {
            await sdk.BattleGetMonsterAcquistion();
        }
        else
        {
            await sdk.BattleGetEventAcquistion();
        }

        var doungeonData = dataManager.GetCurrentDungeonLeveData();
        environmentManager.SetCenterMonsterHpBar(doungeonData.nodeEnum == MapNodeEnum.Boss);
        // 預載入怪物
        await preloadManager.PreloadAllMonster();
        // 設置怪物
        await battleManager.SetMonsterActor(doungeonData.monsterPosAndId, doungeonData.acquisitionItems);
        environmentManager.SetMonsterPos(battleManager.monsters, doungeonData.nodeEnum == MapNodeEnum.EliteMonster);

        // 預載第一關關可能會獲得的道具 Icon (掉落物)
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
        tLs.Add(preloadManager.PreloadItemData(1));
        await UniTask.WhenAll(tLs);
        await UniTask.DelayFrame(1);
        //await UniTask.WhenAll(preloadManager.PreLoadAllItemData());
        //await preloadManager.PreloadItemData(12);
        //var itemDefine = dataTableManager.GetItemDataDefine(1);
        // 重新設置玩家資料
        battleManager.ResetPlayerActor();


        ui.Init(battleManager.player);
        ui.UpdateAntiqueImages(battleManager.player.passives);
        ui.SetTitle(doungeonData.dungeonId);
        // UI 層級 顯示跳血 設置
        for (int i = 0; i < ui.monsterScreenPoint.Count; i++)
        {
            ui.monsterScreenPoint[i].SetFollowTarget(environmentManager.monsterPoints[i].transform);
        }

        battleManager.PlayerPushSkills(-1, battleManager.player);
        passiveManager.GetCurrentActorAttribute(battleManager.player);
        GetController().UpdatePassiveData(battleManager.player);
        for (int i = 0; i < battleManager.monsters.Count; i++)
        {
            GetController().UpdatePassiveData(battleManager.monsters[i]);
        }
        var doungeonDefine = dataTableManager.GetDungeonDataDefine(doungeonData.dungeonId);
        var sceneDefine = dataTableManager.GetSceneDataDefine(doungeonDefine.sceneId);
        GetController().AddPerformanceData(new PUIAnimatonStateData { stateEnum = UIBattle.UIAnimatonStateEnum.SceneName, autoClose = true, sprite = sceneDefine.sceneNameSprite });
        GetController().SwichGameStateByPerformanceData(GameFlowController.GameFlowState.StartFight);
    }

    public override void Update()
    {

    }
}
