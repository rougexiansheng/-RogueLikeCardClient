using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainFlowPreparationState : BaseState<MainFlowController, MainFlowController.MainFlowState>
{
    public override UniTask End()
    {
        return default;
    }

    public override void OnAbort()
    {

    }

    public override UniTask Start()
    {
        return default;
    }

    public override void Update()
    {

    }
}
