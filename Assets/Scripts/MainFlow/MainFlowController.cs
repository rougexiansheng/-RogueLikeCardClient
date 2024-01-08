using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using static PrMainFlowController;

[Serializable]
public class MainFlowController : BaseControl<MainFlowController, MainFlowController.MainFlowState>, IInitializable
{
    [Inject]
    UIManager uiManager;
    [Inject]
    AssetManager assetManager;
    [Inject]
    SkillManager skillManager;
    [Inject]
    PassiveManager passiveManager;
    [Inject]
    MonsterManager monsterManager;
    [Inject]
    EnvironmentManager environmentManager;
    [Inject]
    ItemManager itemManager;
    [Inject]
    DataManager dataManager;
    [Inject]
    GameFlowController gameFlowController;
    [Inject]
    SDKProtocol.IProtocolBridge sdk;
    [Inject]
    DiContainer diContainer;
    MainFlowInitState initState;

    public override Action<string> OnMessage => s => Debug.Log(string.Format("<color=#BE77FF>{0}</color>", s));
    Dictionary<AssetManager.LocalBGMEnum, AudioClip> localBgmClips = new Dictionary<AssetManager.LocalBGMEnum, AudioClip>();
    public enum MainFlowState
    {
        /// <summary>假伺服器初始化</summary>
        FakeServerInit,
        /// <summary>初始化</summary>
        Init,
        /// <summary>下載</summary>
        Download,
        /// <summary>登入畫面</summary>
        Login,
        /// <summary>開始畫面</summary>
        Start,
        /// <summary>大廳</summary>
        Lobby,
        /// <summary>遊戲畫面</summary>
        Game,
        /// <summary>商場</summary>
        Shop,
        /// <summary>整備</summary>
        Preparation,
    }

    public void Initialize()
    {
        initState = new MainFlowInitState();
        SetTransition(MainFlowState.FakeServerInit, SetInjectObj(new MainFlowFakeServerInitState()));
        SetTransition(MainFlowState.Init, SetInjectObj(initState));
        SetTransition(MainFlowState.Download, SetInjectObj(new MainFlowDownloadState()));
        SetTransition(MainFlowState.Login, SetInjectObj(new MainFlowLoginState()));
        SetTransition(MainFlowState.Lobby, SetInjectObj(new MainFlowLobbyState()));
        SetTransition(MainFlowState.Game, SetInjectObj(new MainFlowGameState()));
        SetTransition(MainFlowState.Preparation, SetInjectObj(new MainFlowPreparationState()));
        SetTransition(MainFlowState.Shop, SetInjectObj(new MainFlowShopState()));
    }

    /// <summary>
    /// 開發測試使用
    /// </summary>
    public void Reload()
    {
        initState.Reload().Forget();
    }

    public async UniTask InitLocalAudioClip()
    {
        var ls = new List<UniTask>();
        foreach (string name in Enum.GetNames(typeof(AssetManager.LocalBGMEnum)))
        {
            var e = (AssetManager.LocalBGMEnum)Enum.Parse(typeof(AssetManager.LocalBGMEnum), name);
            ls.Add(LoadAndSetAudioClip(e, $"Assets/DynamicAssets/Sound/LobbyMusic/{name}.wav"));
        }
        await UniTask.WhenAll(ls);
    }

    public void PlayLocalBgm(AssetManager.LocalBGMEnum localBGM)
    {
        if (localBgmClips.TryGetValue(localBGM, out AudioClip clip))
        {
            assetManager.PlayerAudio(AssetManager.AudioMixerVolumeEnum.BGM, clip);
        }
    }

    async UniTask LoadAndSetAudioClip(AssetManager.LocalBGMEnum e, string path)
    {
        var clip = await assetManager.AcyncLoadAsset<AudioClip>(path);
        localBgmClips.TryAdd(e, clip);
    }

    T SetInjectObj<T>(T obj)
    {
        diContainer.Inject(obj);
        return obj;
    }
}
