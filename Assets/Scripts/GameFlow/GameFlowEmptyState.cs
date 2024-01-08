using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameFlowEmptyState : BaseState<GameFlowController, GameFlowController.GameFlowState>
{
    [Inject]
    MainFlowController mainFlowController;
    public override UniTask End()
    {
        return default;
    }

    public override void OnAbort()
    {

    }

    public override UniTask Start()
    {
        GetController().ClearAllPerformanceCallBack();
        GetController().ActivePerformance(false);
        mainFlowController.Trigger(MainFlowController.MainFlowState.Lobby);
        return default;
    }

    public override void Update()
    {

    }
}

