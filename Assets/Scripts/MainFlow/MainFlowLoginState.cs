using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MainFlowLoginState : BaseState<MainFlowController, MainFlowController.MainFlowState>
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

    async public override UniTask Start()
    {     
        await fakeServer.Login(SystemInfo.deviceUniqueIdentifier);
        GetController().Trigger(MainFlowController.MainFlowState.Lobby);
    }

    public override void Update()
    {

    }
}
