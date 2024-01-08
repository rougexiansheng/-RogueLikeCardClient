using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Zenject;
using SDKProtocol;
using System.Threading.Tasks;
using Unity.Properties;

public class MainFlowLobbyState : BaseState<MainFlowController, MainFlowController.MainFlowState>
{
    [Inject]
    UIManager uIManager;
    [Inject]
    BattleManager battleManager;
    [Inject]
    NetworkSaveManager saveManager;

    // For Test -----------------
    [Inject]
    SDKProtocol.IProtocolBridge sdk;
    [Inject]
    DataManager dataManager;
    [Inject]
    DataTableManager dataTableManager;
    // For Test -----------------

    public override void OnAbort()
    {

    }

    public override async UniTask Start()
    {
        var ChooseCharactor = await uIManager.OpenUI<UIChooseCharactor>();
        uIManager.LoadingUI(false);
        await uIManager.FadeOut(0.2f);

        ChooseCharactor.Init(saveManager.GetContainer<NetworkSavePlayerContainer>().unlockProfessionIds, saveManager.GetContainer<NetworkSaveProfessionContainer>().GetSkillGroupsDic());
        var (characterID, selectProfessionData) = await ChooseCharactor.CharacterChoose();

        //Set DefalutSelectCharacterData

        // Set Profession / Dungeon / ProfessionHP to FakeServer
        battleManager.player = dataManager.GainPlayerDataFromProfessionId((ActorProfessionEnum)characterID);

        await sdk.BattleStart(new SelectDungeonData()
        {
            dungeonGroupId = ChooseCharactor.DungeonGroupId,
            professionId = characterID,
            currectHp = battleManager.player.currentHp
        }, selectProfessionData);
        await uIManager.FadeIn(0.2f);
        uIManager.RemoveUI<UIChooseCharactor>();
        uIManager.LoadingUI(true);
        GetController().Trigger(MainFlowController.MainFlowState.Game);
    }

    public override UniTask End()
    {
        return default;
    }

    public override void Update()
    {

    }
}
