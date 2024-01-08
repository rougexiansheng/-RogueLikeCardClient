using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Zenject;
using System;
using DG.Tweening;
public class PrMainFlowLobbyState : BaseState<PrMainFlowController, PrMainFlowController.PrMainFlow>
{
    [Inject]
    UIManager uIManager;

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
        await assetManager.LoadAndSetInObjectPool<UIJumpHpText>();
        await uIManager.FadeIn(2);
        Debug.Log("Loby in");
        await uIManager.FadeOut(2);
        await UniTask.Delay(1000);
        Debug.Log("Loby Delay");
        var uiJump = assetManager.GetObject<UIJumpHpText>();
        Transform target = GameObject.Find("Overlay").transform;
        uiJump.transform.SetParent(target);
        ((RectTransform)uiJump.transform).anchoredPosition3D = Vector3.zero;
        ((RectTransform)uiJump.transform).localScale = Vector2.one;
        Action jump = async () =>
        {
            await uiJump.Jump(50, true);
            if (uiJump == null) return;
            assetManager.ReturnObjToPool(uiJump.gameObject);
        };
        jump();
    }

    public override void Update()
    {

    }
}
