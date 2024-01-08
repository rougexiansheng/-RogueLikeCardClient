using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class MainFlowInitState : BaseState<MainFlowController, MainFlowController.MainFlowState>
{
    [Inject]
    UIManager uIManager;
    [Inject]
    SkillManager skillManager;
    [Inject]
    PassiveManager passiveManager;
    [Inject]
    ItemManager itemManager;
    [Inject]
    EnvironmentManager environmentManager;
    [Inject]
    MonsterManager monsterManager;
    [Inject]
    DataManager dataManager;
    [Inject]
    DataTableManager dataTableManager;
    [Inject]
    PreloadManager preloadManager;

    public override UniTask End()
    {
        return default;
    }

    public override void OnAbort()
    {

    }

    async public override UniTask Start()
    {
        Application.targetFrameRate = GameConfig.instance.targetFrameRate;

        await uIManager.FadeIn(0);
        uIManager.LoadingUI(true);

        await dataTableManager.LoadMainTable();
        GetController().PlayLocalBgm(AssetManager.LocalBGMEnum.Lobby);
        GetController().Trigger(MainFlowController.MainFlowState.Download);
    }

    public override void Update()
    {

    }

    /// <summary>
    /// 開發測試使用
    /// 技能/被動 重新載入
    /// </summary>
    public async UniTask Reload()
    {
        await uIManager.FadeIn(0.2f);
        await dataTableManager.LoadMainTable();
        await UniTask.WhenAll(preloadManager.PrelaodProfessionData(ActorProfessionEnum.Witch),
            preloadManager.PreloadSkillPrefab(ActorProfessionEnum.Witch),
            preloadManager.PreloadPassiveAll());
        await uIManager.FadeOut(0.2f);
    }

}
