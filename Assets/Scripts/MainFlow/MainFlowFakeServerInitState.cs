using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MainFlowFakeServerInitState : BaseState<MainFlowController, MainFlowController.MainFlowState>
{
    [Inject]
    FakeServer fakeServer;

    public override UniTask End()
    {
        return default;
    }

    public override void OnAbort()
    {

    }

    public override UniTask Start()
    {
        fakeServer.LoadFakeServerData();
        GetController().Trigger(MainFlowController.MainFlowState.Init);
        return default;
    }

    public override void Update()
    {

    }
}
