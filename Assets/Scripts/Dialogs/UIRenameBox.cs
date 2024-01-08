using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UIRenameBox : UIBase
{
    [SerializeField]
    private string m_defaultName;
    [SerializeField]
    private TMP_InputField m_inputFieldName;
    [SerializeField]
    private Button m_buttonConfirm;
    [SerializeField]
    private Button m_buttonCancel;

    public bool IsDone { get; private set; }

    private Action<string> m_onConfirm;
    private Action<string> m_onCancel;
    private string m_name;
    private string m_lastInputText;

    public override UniTask OnOpen()
    {
        IsDone = false;

        m_inputFieldName.onValueChanged.AddListener(OnInputFieldChanged);
        m_buttonConfirm.onClick.AddListener(OnButtonConfirmClick);
        m_buttonCancel.onClick.AddListener(OnButtonCancelChest);

        m_name = string.Empty;
        m_lastInputText = string.Empty;

        return base.OnOpen();
    }

    public override void OnClose()
    {
        m_inputFieldName.onValueChanged.RemoveListener(OnInputFieldChanged);
        m_buttonConfirm.onClick.RemoveListener(OnButtonConfirmClick);
        m_buttonCancel.onClick.RemoveListener(OnButtonCancelChest);

        m_name = string.Empty;
        m_lastInputText = string.Empty;

        base.OnClose();
    }

    public async UniTask Init(Action<string> onConfirm = null, Action<string> onCancel = null)
    {
        await UniTask.DelayFrame(1);
        m_onConfirm = onConfirm;
        m_onCancel = onCancel;
    }

    private void OnInputFieldChanged(string context)
    {
        if (context.Length >= m_inputFieldName.characterLimit)
        {
            //Debug.Log($"OnInputFieldChanged: 文字到達上限, '{m_inputFieldName.text}'還原成最後的文字'{m_lastInputText}'");
            m_name = m_lastInputText;
            m_inputFieldName.text = m_lastInputText;
            return;
        }

        //Debug.Log($"OnInputFieldChanged m_name: {m_name} -> {context}");

        m_name = context;     

        //Debug.Log($"OnInputFieldChanged m_lastInputText: {m_lastInputText} -> {m_name}");

        m_lastInputText = m_name;
    }

    private void OnButtonConfirmClick()
    {
        // 至少需兩個字
        if (!string.IsNullOrEmpty(m_name) && m_name.Length < 2)
        {
            return;
        }

        // 沒有輸入直接用預設名稱
        if (string.IsNullOrEmpty(m_name))
        {
            //Debug.Log($"OnButtonConfirmClick: 文字未達兩字使用預設名字'{m_defaultName}'");
            m_name = m_defaultName;
            m_inputFieldName.text = m_defaultName;
        }

        m_onConfirm?.Invoke(m_name);
        IsDone = true;
    }

    private void OnButtonCancelChest()
    {
        m_onCancel?.Invoke(m_defaultName);
        IsDone = true;
    }
}
