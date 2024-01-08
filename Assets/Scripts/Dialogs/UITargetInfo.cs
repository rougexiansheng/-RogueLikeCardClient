using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;

public class UITargetInfo : UIBase, LoopScrollDataSource, LoopScrollPrefabSource
{
    [Inject]
    DataTableManager dataTableManager;
    [Inject]
    BattleManager battleManager;
    [Inject]
    UIManager uIManager;
    [Inject]
    AssetManager assetManager;
    [Inject]
    SkillManager skillManager;
    [Inject]
    PassiveManager passiveManager;
    [SerializeField]
    Button bgButton;
    [SerializeField]
    TMPro.TMP_Text hp, actorName;
    [SerializeField]
    Image nextSkillIcon;
    [SerializeField]
    Button skillBtn;
    [SerializeField]
    GameObject monsterNextSkillObj;
    [SerializeField]
    LoopVerticalScrollRect scrollRect;
    List<UIStateItem> stateItems = new List<UIStateItem>();
    List<ActorPassive> passives;
    public override UniTask OnOpen()
    {
        scrollRect.dataSource = this;
        scrollRect.prefabSource = this;
        ClearStateItems();
        bgButton.OnClickAsObservable().Subscribe(_ => uIManager.RemoveUI(this));
        RxEventBus.Register<BattleActor.MonsterPositionEnum>(EventBusEnum.PlayerDataEnum.UpdateSelectTarget,
            p => Init(battleManager.monsters.Find(m => m.monsterPos == p)), this);
        return base.OnOpen();
    }
    public void Init(BattleActor actor)
    {
        if (actor == null) return;
        if (!actor.isPlayer)
        {
            var define = dataTableManager.GetSkillDefine(actor.monsterNextSkill);
            nextSkillIcon.sprite = define.icon;
            skillBtn.onClick.RemoveAllListeners();
            skillBtn.OnClickAsObservable().Subscribe(_ => OpenSkillInfo(define.id));
        }
        monsterNextSkillObj.SetActive(!actor.isPlayer);
        actorName.text = actor.actorName;
        hp.text = $"{actor.currentHp}/{actor.currentActorBaseAttribute.maxHp.GetValue()}";
        passives = actor.passives.FindAll(p =>
        {
            var d = dataTableManager.GetPassiveDefine(p.passiveId);
            return !d.isInvisible;
        });
        scrollRect.totalCount = passives.Count;
        scrollRect.RefillCells();

    }
    async void OpenSkillInfo(int skillId)
    {
        var ui = await uIManager.OpenUI<UISkillPopupInfoPage>();
        ui.Init(skillId);
        ui.CancelButton.OnClickAsObservable().Subscribe(_ => uIManager.RemoveUI(ui));
    }

    void ClearStateItems()
    {
        for (int i = 0; i < stateItems.Count; i++)
        {
            var item = stateItems[i];
            item.Clear();
            assetManager.ReturnObjToPool(item.gameObject);
        }
        stateItems.Clear();
    }

    public GameObject GetObject(int index)
    {
        var item = assetManager.GetObject<UIStateItem>();
        var define = dataTableManager.GetPassiveDefine(passives[index].passiveId);
        item.SetState(passives[index], define);
        stateItems.Add(item);
        item.transform.localScale = Vector3.one;
        return item.gameObject;
    }

    public void ReturnObject(Transform trans)
    {
        var item = trans.GetComponent<UIStateItem>();
        stateItems.Remove(item);
        assetManager.ReturnObjToPool(item.gameObject);
    }

    public void ProvideData(Transform transform, int idx)
    {
        var item = transform.GetComponent<UIStateItem>();
        var define = dataTableManager.GetPassiveDefine(passives[idx].passiveId);
        item.SetState(passives[idx], define);
    }

    public override void OnClose()
    {
        RxEventBus.UnRegister(this);
        base.OnClose();
    }
}
