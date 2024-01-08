using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UISkillInfo : UIBase
{
    [Inject]
    DataTableManager dataTableManager;
    [Inject]
    SkillManager skillManager;
    public enum CostItemColor
    {
        Non,
        Red,
        Green,
        Blue,
        Gray,
    }
    [SerializeField]
    private TMP_Text skillTileText;

    [SerializeField]
    private TMP_Text skillInfoText;

    [SerializeField]
    private List<Sprite> manaItemImages;
    [SerializeField]
    private List<ManaItem> manaItems;

    private int currentIndex = 0;

    public void Init(int id)
    {
        CloseMamaItem();
        skillTileText.text = "";
        skillInfoText.text = "";
        currentIndex = 0;
        setupPopupInfoPage(id);
    }

    public void CloseMamaItem()
    {
        foreach (var item in manaItems)
        {
            item.gameObject.SetActive(false);
        }
    }

    private void SetText(string title, string info)
    {
        skillTileText.text = title;
        skillInfoText.text = info;
    }
    private void setupPopupInfoPage(int id)
    {
        var data = dataTableManager.GetSkillDefine(id);
        SetText(data.skillName, data.comment);
        foreach (var cost in data.costColors)
        {
            var colorsList = skillManager.GetColorsList(cost.colorEnum);
            CostItemColor itemColor = CostItemColor.Non;
            if (colorsList.Count > 1)
            {
                itemColor = CostItemColor.Gray;
            }
            else
            {
                if (colorsList[0] == SkillCostColorEnum.Red)
                {
                    itemColor = CostItemColor.Red;
                }
                else if (colorsList[0] == SkillCostColorEnum.Green)
                {
                    itemColor = CostItemColor.Green;
                }
                else if (colorsList[0] == SkillCostColorEnum.Blue)
                {
                    itemColor = CostItemColor.Blue;
                }
            }
            addCostItem(itemColor, cost.count.ToString());
        }
    }
    private void addCostItem(CostItemColor color, string num)
    {
        ManaItem prefab = manaItems[currentIndex];
        prefab.gameObject.SetActive(true);
        Image image = prefab.GetComponent<Image>();
        switch (color)
        {
            case CostItemColor.Red:
                image.sprite = manaItemImages[0];
                break;
            case CostItemColor.Green:
                image.sprite = manaItemImages[1];
                break;
            case CostItemColor.Blue:
                image.sprite = manaItemImages[2];
                break;
            case CostItemColor.Gray:
                image.sprite = manaItemImages[3];
                break;
        }
        if (num == "-1")
            num = "X";
        prefab.SetText(num);
        currentIndex++;
    }
}
