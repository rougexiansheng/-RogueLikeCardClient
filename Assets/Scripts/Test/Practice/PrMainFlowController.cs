using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[Serializable]
public class PrMainFlowController : BaseControl<PrMainFlowController, PrMainFlowController.PrMainFlow>, IInitializable
{

    [Inject]
    DiContainer diContainer;

    public override Action<string> OnMessage => s => Debug.Log(string.Format("<color=#BE77FF>{0}</color>", s));

    public enum PrMainFlow
    {
        /// <summary>初始化</summary>
        Init,
        /// <summary>下載</summary>
        Lobby,

    }

    public void Initialize()
    {
        SetTransition(PrMainFlow.Init, SetInjectObj(new PrMainFlowInitState()));
        SetTransition(PrMainFlow.Lobby, SetInjectObj(new PrMainFlowLobbyState()));
    }

    T SetInjectObj<T>(T obj)
    {
        diContainer.Inject(obj);
        return obj;
    }


}
