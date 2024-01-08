using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UniRx;
using Zenject;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class UITopBar : MonoBehaviour
{
    [SerializeField]
    private TMP_Text coinText;

    [SerializeField]
    private Button setting, leave, speed1, speed2;

    [Inject]
    NetworkSaveManager saveManager;
    [Inject]
    UIManager uIManager;
    [Inject]
    GameFlowController gameFlow;
    private int durationTime = 1;

    private int currentCoins;

    public UniTask coinEffectTask;

    private UniTaskCompletionSource<bool> effectEndTask;
    private object _lock = new object();

    private void Start()
    {
        Init();
    }


    public void Init()
    {
        currentCoins = saveManager.GetContainer<NetworkSaveBattleItemContainer>().GetCount(NetworkSaveBattleItemContainer.COIN);
        coinText.text = currentCoins.ToString();
        saveManager.GetContainer<NetworkSaveBattleItemContainer>().onItemUpdate += async (id, val) =>
        {
            switch (id)
            {
                case NetworkSaveBattleItemContainer.COIN:
                    int targetvalue = val;
                    await coinIncreaseEffect(targetvalue);
                    currentCoins = saveManager.GetContainer<NetworkSaveBattleItemContainer>().GetCount(NetworkSaveBattleItemContainer.COIN);
                    break;
            }
        };
        speed1.onClick.AddListener(() =>
        {
            speed2.gameObject.SetActive(true);
            speed1.gameObject.SetActive(false);
            Time.timeScale = 1;
        });
        speed2.onClick.AddListener(() =>
        {
            speed2.gameObject.SetActive(false);
            speed1.gameObject.SetActive(true);
            Time.timeScale = 2;
        });
        // 依照當前速度 顯示按鈕
        if (Time.timeScale <= 1f)
        {
            speed2.gameObject.SetActive(true);
            speed1.gameObject.SetActive(false);
        }
        else
        {
            speed2.gameObject.SetActive(false);
            speed1.gameObject.SetActive(true);
        }

        leave.onClick.AddListener(LeaveGame);
        setting.onClick.AddListener(OnButtonSetting);
    }

    async void LeaveGame()
    {
        await uIManager.ShowTwoBottonMessageBox("確定要離開終止遊戲?", () =>
        {
            gameFlow.Trigger(GameFlowController.GameFlowState.EndGame);
        });
    }

    public async UniTask coinIncreaseEffect(int targetValue)
    {
        effectEndTask = new UniTaskCompletionSource<bool>();
        Tween tween = DOTween.To(() => currentCoins, x => currentCoins = x, targetValue, durationTime).OnUpdate(() => coinText.text = currentCoins.ToString());
        await tween.AsyncWaitForCompletion();
        effectEndTask.TrySetResult(true);
    }

    public UniTask<bool> IsEffectEnd()
    {
        return (effectEndTask != null) ? effectEndTask.Task : default;
    }

    private async void OnButtonSetting()
    {
        await uIManager.ShowTwoBottonMessageBox("敬啟期待");
    }
}
