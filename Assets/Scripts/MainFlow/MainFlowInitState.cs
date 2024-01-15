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

    [Inject]
    AssetManager assetManager;

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

        await UniTask.WhenAll(
           // 預先載入UI
           uIManager.PreloadUI(
           typeof(UISkillPopupInfoPage),
           typeof(UISkill)
           )
           );

        await dataTableManager.LoadMainTable();
        GetController().PlayLocalBgm(AssetManager.LocalBGMEnum.Lobby);
        GetController().Trigger(MainFlowController.MainFlowState.Download);
    }

    public override void Update()
    {

    }

    /// <summary>
    /// �}�o���ըϥ�
    /// �ޯ�/�Q�� ���s���J
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
