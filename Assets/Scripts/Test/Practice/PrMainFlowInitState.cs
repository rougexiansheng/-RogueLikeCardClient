using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;


public class PrMainFlowInitState : BaseState<PrMainFlowController, PrMainFlowController.PrMainFlow>
{
    public override UniTask End()
    {
        return default;
    }

    public override void OnAbort()
    {

    }

    async public override UniTask Start()
    {
        await UniTask.Delay(1000);
        Debug.Log("Delay");
        await UniTask.Delay(1000);
        GetController().Trigger(PrMainFlowController.PrMainFlow.Lobby);
    }

    public override void Update()
    {

    }
}
