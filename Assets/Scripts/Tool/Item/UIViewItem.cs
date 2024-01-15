using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


public class ViewItemDataBase
{
    /// <summary> 資料表ID </summary>
    public int Id;
    /// <summary>是否已買</summary>
    public bool Purchased;
    /// <summary>是否買得起</summary>
    public bool Insufficient;
    /// <summary>道具種類</summary>
    public string ItemTypeStr; // "技能(此字串未讀表)"
}

public class ViewTableItem : ViewItemDataBase
{
    /// <summary>道具資料</summary>
    public ItemDataDefine ItemDataDefine;
}

public class ViewTableSkill : ViewItemDataBase
{
    /// <summary>技能資料</summary>
    public SkillDataDefine ItemDataDefine;
}

public class ViewItemData //: ViewItemDataBase
{
    public ViewItemType viewItemType;
    public int id;
    public string name;
    public Sprite icon;
    public string comment;
    public int count;


    /// <summary>是否已買</summary>
    public bool Purchased;
    /// <summary>是否買得起</summary>
    public bool Insufficient;
    /// <summary>道具種類</summary>
    public string ItemTypeStr; // "技能(此字串未讀表)"

    // 以下 ItemDataDefine 專用
    public int coinPrice;
    public int mallPrice;
    public int arg;
    public ItemTpyeEnum itemType;
}


public class UIViewItem : MonoBehaviour
{
    [Inject]
    UIManager uIManager;
    [Inject]
    NetworkSaveManager saveManager;

    [Inject]
    DataTableManager dataTableManager;

    [SerializeField]
    protected Image m_ImageShopItemIcon;
    [SerializeField]
    protected TMP_Text m_textShopItemName;
    [SerializeField]
    protected TMP_Text m_textShopItemType;
    [SerializeField]
    protected TMP_Text m_textPrice;
    [SerializeField]
    protected Button m_ButtonItem;
    [SerializeField]
    protected GameObject m_objPrice;
    [SerializeField]
    protected GameObject m_purchasedMask;
    [SerializeField]
    protected GameObject m_insufficientMask;

    private ViewItemData m_shopItemDataDefine;
    private Action<ViewItemData> m_onItemClickCallback;

    private bool ShopMode = false;
    public void Start()
    {
        m_textShopItemType.raycastTarget = false;
        m_textShopItemName.raycastTarget = false;
        m_textPrice.raycastTarget = false;
        m_ButtonItem.onClick.AddListener(OnViewItemClick);
    }

    public void SetViewItemData(ViewItemData itemData)
    {
        m_shopItemDataDefine = itemData;
        var itemDefine = dataTableManager.GetItemDataDefine(itemData.id);
        if (m_shopItemDataDefine != null)
        {
            m_textPrice.text = m_shopItemDataDefine.coinPrice.ToString();
            m_ImageShopItemIcon.sprite = m_shopItemDataDefine.icon;
            if (m_shopItemDataDefine.count == -1)
            {
                m_textShopItemName.text = m_shopItemDataDefine.name;
            }
            else
            {
                m_textShopItemName.text = m_shopItemDataDefine.name + " x" + m_shopItemDataDefine.count;
            }

            m_textShopItemType.text = m_shopItemDataDefine.ItemTypeStr;
            m_objPrice.SetActive(m_shopItemDataDefine.coinPrice > 0);

            m_purchasedMask.SetActive(itemData.Purchased);
            if (!itemData.Purchased)
            {
                m_insufficientMask.SetActive(itemData.Insufficient);
            }
            else
            {
                m_insufficientMask.SetActive(false);
            }
        }
        else
        {
            // 如果沒資料強制顯示已購買
            m_purchasedMask.SetActive(true);
            m_insufficientMask.SetActive(false);
        }
    }

    public void BindClick(Action<ViewItemData> onItemClickCallback)
    {
        m_onItemClickCallback = onItemClickCallback;
    }

    private void OnViewItemClick()
    {
        m_onItemClickCallback?.Invoke(m_shopItemDataDefine);
        if (ShopMode)
            Buyed();
    }

    public void TurnOnShopMode()
    {
        ShopMode = true;
    }

    public void Buyed()
    {
        m_ButtonItem.interactable = false;
        m_purchasedMask.SetActive(true);
    }


    private async UniTask OpenSkillUi(int skillId)
    {
        var skill = await uIManager.OpenUI<UISkill>();
        await skill.OpenChangeSkillPage(saveManager.GetContainer<NetworkSaveBattleSkillContainer>().GetOriginalSKillList(), skillId);
        var isChange = await skill.IsSkillsChange();
        if (isChange)
        {
            m_onItemClickCallback?.Invoke(m_shopItemDataDefine);
        }
    }
}
