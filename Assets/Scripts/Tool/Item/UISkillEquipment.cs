using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISkillEquipment : MonoBehaviour
{

    [SerializeField]
    private UISkillStateItem skillStatePrefab;

    [SerializeField]
    private GameObject unlockScrollContentRoot;

    [SerializeField]
    private Button changeSkillNameButton;

    [SerializeField]
    public TMP_Text skillSetName;


    [SerializeField]
    private Button changeNameButton;

    [SerializeField]
    public Button prevPageButton;
    [SerializeField]
    public Button nextPageButton;

    [SerializeField]
    private List<SkillSetItem> skillSetItems;



    [SerializeField]
    private ManaItem playerRedMana;

    [SerializeField]
    private ManaItem playerGreenMana;

    [SerializeField]
    private ManaItem playerBlueMana;

    [SerializeField]
    private Button changeSkillButton;

    [SerializeField]
    public Button backButton;
    [SerializeField]
    private Button cancelSaveButton;
    [SerializeField]
    private Button saveButton;

    public void Init()
    {
        ResetSkillSetItem();
        RemoveAllButtonListener();
        changeNameButton.interactable = false;
        changeSkillNameButton.interactable = false;
        skillSetName.text = "預設名稱";
    }


    /// <summary>
    /// 生成Player目前擁有的SKill item ，Icon類型預設是沒裝備的狀態
    /// </summary>
    /// <param name="name"></param>
    /// <param name="icon"></param>
    /// <returns></returns>
    public UISkillStateItem CreatePlayerSkill(string name, Sprite icon)
    {
        var statetitem = Instantiate(skillStatePrefab);
        statetitem.Init(name, icon);
        statetitem.transform.SetParent(unlockScrollContentRoot.transform, false);
        statetitem.SetUpSkillType(UISkillStateItem.SkillItemTypeEnum.Unequipped);

        return statetitem;
    }


    public void SetManaPool(int redMana, int greenMana, int blueMana)
    {
        playerRedMana.SetText(redMana.ToString());
        playerGreenMana.SetText(greenMana.ToString());
        playerBlueMana.SetText(blueMana.ToString());
    }

    public void ResetSkillSetItem()
    {
        foreach (var item in skillSetItems)
        {
            item.Rest();
        }
    }
    public void SetTargetSetItemButton(int pageNumnum, int index)
    {
        skillSetItems[index].button.interactable = true;
        skillSetItems[index].numText.text = pageNumnum.ToString();
    }

    public void TurnOffAllSetItemButton()
    {
        foreach (var item in skillSetItems)
        {
            item.button.enabled = false;
        }
    }
    public void TurnOnAllSetItemButton()
    {
        foreach (var item in skillSetItems)
        {
            item.button.enabled = true;
        }
    }



    public void ClickTargetSetButton(int index)
    {
        skillSetItems[index].button.onClick.Invoke();
    }

    public void TurnOnTargetSelectImage(int index)
    {
        skillSetItems[index].SelectImage.gameObject.SetActive(true);
    }

    public void ResetAllSelectImage()
    {
        foreach (var item in skillSetItems)
        {
            item.SelectImage.gameObject.SetActive(false);
        }
    }

    public void AddTargetSetItemButtonListener(Action action, int index)
    {
        skillSetItems[index].button.onClick.AddListener(() =>
        {
            action();
        });
    }

    public void RemoveAllSetItemButtonListener()
    {
        foreach (var item in skillSetItems)
        {
            item.button.onClick.RemoveAllListeners();
        }
    }


    public void RemoveAllButtonListener()
    {
        backButton.onClick.RemoveAllListeners();
        foreach (var item in skillSetItems)
        {
            item.button.onClick.RemoveAllListeners();
        }
        // cancel Save
        // save
        // change Skill
    }

}
