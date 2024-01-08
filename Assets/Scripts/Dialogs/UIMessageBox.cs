using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIMessageBox : UIBase
{
    [SerializeField]
    private TMP_Text m_message;
    [SerializeField]
    private Button m_buttonConfirm;
    [SerializeField]
    private Button m_buttonCancel;

    public bool IsDone { get; private set; }

    private Action m_onConfirm;
    private Action m_onCancel;

    public override UniTask OnOpen()
    {
        IsDone = false;

        m_buttonConfirm.onClick.AddListener(OnButtonConfirmClick);
        m_buttonCancel.onClick.AddListener(OnButtonCancelChest);

        return base.OnOpen();
    }

    public override void OnClose()
    {
        m_buttonConfirm.onClick.RemoveListener(OnButtonConfirmClick);
        m_buttonCancel.onClick.RemoveListener(OnButtonCancelChest);

        base.OnClose();
    }

    public async UniTask ShowOneBottonMessageBox(string message, Action onConfirm)
    {
        await UniTask.DelayFrame(1);
        m_message.text = message;
        m_onConfirm = onConfirm;
        m_onCancel = null;
        m_buttonConfirm.gameObject.SetActive(true);
        m_buttonCancel.gameObject.SetActive(false);
    }

    public async UniTask ShowTwoBottonMessageBox(string message, Action onConfirm, Action onCancel)
    {
        await UniTask.DelayFrame(1);
        m_message.text = message;
        m_onConfirm = onConfirm;
        m_onCancel = onCancel;
        m_buttonConfirm.gameObject.SetActive(true);
        m_buttonCancel.gameObject.SetActive(true);
    }

    private void OnButtonConfirmClick()
    {
        m_onConfirm?.Invoke();
        IsDone = true;
    }

    private void OnButtonCancelChest()
    {
        m_onCancel?.Invoke();
        IsDone = true;
    }
}
