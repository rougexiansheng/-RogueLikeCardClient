using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using DG.Tweening;

public class UICG : UIBase
{
    [Inject]
    AssetManager assetManager;
    [Inject]
    DataManager dataManager;
    [Inject]
    NetworkSaveManager saveManager;
    [Inject]
    DataTableManager tableManager;
    [SerializeField]
    Spine.Unity.SkeletonGraphic spine;
    [SerializeField]
    RectTransform spinePoint;
    [SerializeField]
    CanvasGroup canvasGroup;
    public void Init()
    {
        var pDefine = tableManager.GetProfessionDataDefine(saveManager.GetContainer<NetworkSaveBattleDungeonContainer>().SelectProfession);
        var currentDungeonData = dataManager.GetCurrentDungeonLeveData();
        var dungeonDefine = tableManager.GetDungeonDataDefine(currentDungeonData.dungeonId);
        GameObject spineObj = null;
        switch (dungeonDefine.mapLayer)
        {
            case 1:
                spineObj = GameObject.Instantiate(pDefine.cgSpine1.gameObject, spinePoint);
                break;
            case 2:
                spineObj = GameObject.Instantiate(pDefine.cgSpine2.gameObject, spinePoint);
                break;
            case 3:
                spineObj = GameObject.Instantiate(pDefine.cgSpine3.gameObject, spinePoint);
                break;
            case 4:
            default:
                spineObj = GameObject.Instantiate(pDefine.cgSpine4.gameObject, spinePoint);
                break;
        }
        if (spineObj != null)
        {
            spine = spineObj.GetComponent<Spine.Unity.SkeletonGraphic>();
        }
    }

    public UniTask PlayerCGAnimation()
    {
        var pDefine = tableManager.GetProfessionDataDefine(saveManager.GetContainer<NetworkSaveBattleDungeonContainer>().SelectProfession);
        var t = new UniTaskCompletionSource();
        spine.AnimationState.ClearTrack(0);
        assetManager.PlayerAudio(AssetManager.AudioMixerVolumeEnum.Speak, pDefine.cgSound1);
        spine.AnimationState.SetAnimation(0, "assassin_ero", false).Complete += _ =>
        {
            spine.AnimationState.ClearTrack(0);
            assetManager.PlayerAudio(AssetManager.AudioMixerVolumeEnum.Speak, pDefine.cgSound2);
            spine.AnimationState.SetAnimation(0, "assassin_ero_2", false).Complete += async _ =>
            {
                await FadeOut();
                t.TrySetResult();
            };
        };
        return t.Task;
    }

    UniTask FadeOut()
    {
        return canvasGroup.DOFade(0, 0.2f).AsyncWaitForCompletion().AsUniTask();
    }
}
