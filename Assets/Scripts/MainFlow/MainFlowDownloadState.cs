using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MainFlowDownloadState : BaseState<MainFlowController, MainFlowController.MainFlowState>
{
    [Inject]
    PreloadManager preloadManager;

    public override UniTask End()
    {
        return default;
    }

    public override void OnAbort()
    {

    }

    public override async UniTask Start()
    {
        await PreloadAllProfessionSkill();
        GetController().Trigger(MainFlowController.MainFlowState.Login);

    }

    private async UniTask PreloadAllProfessionSkill()
    {
        await UniTask.WhenAll(
            preloadManager.PreloadSkillPrefab(ActorProfessionEnum.Witch),
            preloadManager.PrelaodProfessionData(ActorProfessionEnum.Witch),
            preloadManager.PrelaodProfessionData(ActorProfessionEnum.Paladin),
            preloadManager.PrelaodProfessionData(ActorProfessionEnum.Fencer),
            preloadManager.PreloadPassiveAll()
            );
    }

    public override void Update()
    {

    }
}
