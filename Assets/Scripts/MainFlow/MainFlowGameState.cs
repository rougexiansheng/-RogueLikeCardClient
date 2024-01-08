using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MainFlowGameState : BaseState<MainFlowController, MainFlowController.MainFlowState>
{
    [Inject]
    GameFlowController gameFlowController;
    [Inject]
    UIManager uIManager;
    public override UniTask End()
    {
        return default;
    }

    public override void OnAbort()
    {

    }

    public override UniTask Start()
    {
        gameFlowController.Trigger(GameFlowController.GameFlowState.Init);
        return default;
    }

    public override void Update()
    {
        gameFlowController?.Update();
    }
}
