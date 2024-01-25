using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UISkillChangePage : UIBase
{
    [Inject]
    DataTableManager dataTableManager;
    [Inject]
    SkillManager skillManager;
    [SerializeField]
    public UISkillInfo CurrentInfoItem;

    [SerializeField]
    public UISkillInfo UpdateSkillInfo;

    [SerializeField]
    private Button cancelButton;

    [SerializeField]
    private Button replaceButton;
    [SerializeField]
    private Button levelUpButton;
    [SerializeField]
    private UISkillBattleItem targetSkill;

    [SerializeField]
    private TMP_Text changeStateText;

    [SerializeField]
    private List<ManaChangeItem> manaChangeItems;

    public void Init(int skillID)
    {
        targetSkill.gameObject.SetActive(true);
        targetSkill.SetSkillItem(dataTableManager.GetSkillDefine(skillID));
        RemoveAllListeners();
        changeStateText.text = "";
        replaceButton.gameObject.SetActive(false);
        levelUpButton.gameObject.SetActive(false);

        foreach (var item in manaChangeItems)
        {
            item.gameObject.SetActive(false);
        }
    }

    public void SetManaChangeItems(int orignalID, int chnageID)
    {
        foreach (var item in manaChangeItems)
        {
            item.gameObject.SetActive(false);
            item.SetText(0);
        }

        var orignalDic = createManaColorDic(dataTableManager.GetSkillDefine(orignalID).costColors);
        var changeDic = createManaColorDic(dataTableManager.GetSkillDefine(chnageID).costColors);

        foreach (var item in changeDic)
        {
            if (orignalDic.TryGetValue(item.Key, out int value))
            {
                SetChangeManageItem(item.Key, item.Value - value);
            }
            else
            {
                SetChangeManageItem(item.Key, item.Value);
            }
        }
        foreach (var item in orignalDic)
        {
            if (!changeDic.ContainsKey(item.Key))
            {
                SetChangeManageItem(item.Key, -item.Value);
            }
        }

    }


    private void SetChangeManageItem(ManaItemColor color, int count)
    {
        switch (color)
        {
            case ManaItemColor.Red:
                manaChangeItems[0].gameObject.SetActive(true);
                manaChangeItems[0].SetText(count);
                break;
            case ManaItemColor.Green:
                manaChangeItems[1].gameObject.SetActive(true);
                manaChangeItems[1].SetText(count);
                break;
            case ManaItemColor.Blue:
                manaChangeItems[2].gameObject.SetActive(true);
                manaChangeItems[2].SetText(count);
                break;
        }
    }

    private Dictionary<ManaItemColor, int> createManaColorDic(List<SkillCostColorData> costColorList)
    {
        Dictionary<ManaItemColor, int> result = new Dictionary<ManaItemColor, int>();
        foreach (var cost in costColorList)
        {
            var colorsList = skillManager.GetColorsList(cost.colorEnum);
            ManaItemColor itemColor = ManaItemColor.Non;
            if (colorsList.Count > 1)
            {
                itemColor = ManaItemColor.Gray;
            }
            else
            {
                if (colorsList[0] == SkillCostColorEnum.Red)
                {
                    itemColor = ManaItemColor.Red;
                }
                else if (colorsList[0] == SkillCostColorEnum.Green)
                {
                    itemColor = ManaItemColor.Green;
                }
                else if (colorsList[0] == SkillCostColorEnum.Blue)
                {
                    itemColor = ManaItemColor.Blue;
                }
            }
            result.Add(itemColor, cost.count);

        }
        return result;
    }


    public void ResetStateUI()
    {
        replaceButton.gameObject.SetActive(false);
        levelUpButton.gameObject.SetActive(false);
        changeStateText.text = "";
        RemoveAllListeners();
    }

    public void OpenReplaceUI()
    {
        changeStateText.text = "技能替換";
        replaceButton.gameObject.SetActive(true);
    }


    public void OpenLevelUpUI()
    {
        changeStateText.text = "技能升級";
        levelUpButton.gameObject.SetActive(true);
    }

    public void OpenMaxLevelUI()
    {
        changeStateText.text = "技能等級最高";
    }

    public void AddLevelUpButtonListener(Action action)
    {
        levelUpButton.onClick.AddListener(() =>
        {
            action();
        });
    }

    public void AddReplaceButtonListener(Action action)
    {
        replaceButton.onClick.AddListener(() =>
        {
            action();
        });
    }

    public void RemoveAllListeners()
    {
        replaceButton.onClick.RemoveAllListeners();
        levelUpButton.onClick.RemoveAllListeners();
    }



    /// <summary>
    /// 返回技能三選一
    /// </summary>
    /// <param name="action"></param>
    public void AddCancelButtonListener(Action action)
    {
        cancelButton.onClick.AddListener(() =>
        {
            action();
        });
    }

    private void OnDestroy()
    {
        RemoveAllListeners();
    }

}
