using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISkillStateItem : MonoBehaviour
{
    public enum SkillItemTypeEnum
    {
        Equipment,
        Unequipped,
        Reward,
        ShopWithMoney,
        ShopWithShard,
        InsufficientMoney,
        ShopUnlcok,
        Relic,
    }

    [SerializeField]
    private TMP_Text SkillNameText;

    [SerializeField]
    private Image SkillIcon;

    [SerializeField]
    private GameObject moneyUI;

    [SerializeField]
    private TMP_Text moneyText;

    [SerializeField]
    private GameObject shardUI;

    [SerializeField]
    private TMP_Text shardText;

    [SerializeField]
    private GameObject equipUI;

    [SerializeField]
    private GameObject skillType;

    [SerializeField]
    private GameObject relicType;

    [SerializeField]
    private GameObject unlockedUI;

    [SerializeField]
    private GameObject InsufficientMoneyUI;

    [SerializeField]
    private GameObject availableUIEffect;

    [SerializeField]
    public ButtonLongPress ButtonLongPress;

    public SkillItemTypeEnum state;


    public void Init(string skillName, Sprite icon)
    {
        moneyUI.SetActive(false);
        shardUI.SetActive(false);
        equipUI.SetActive(false);
        skillType.SetActive(false);
        relicType.SetActive(false);
        unlockedUI.SetActive(false);
        InsufficientMoneyUI.SetActive(false);
        availableUIEffect.SetActive(false);
        SkillNameText.text = skillName;
        SkillIcon.sprite = icon;
    }

    public void SetUpSkillType(SkillItemTypeEnum type)
    {
        state = type;
        switch (state)
        {
            case SkillItemTypeEnum.Equipment:
                skillType.SetActive(true);
                equipUI.SetActive(true);

                break;
            case SkillItemTypeEnum.Unequipped:
                skillType.SetActive(true);
                break;
            case SkillItemTypeEnum.Reward:
                skillType.SetActive(true);
                availableUIEffect.SetActive(true);
                break;
            case SkillItemTypeEnum.ShopWithMoney:
                skillType.SetActive(true);
                moneyUI.SetActive(true);
                break;
            case SkillItemTypeEnum.ShopWithShard:
                skillType.SetActive(true);
                shardUI.SetActive(true);
                break;
            case SkillItemTypeEnum.InsufficientMoney:
                skillType.SetActive(true);
                InsufficientMoneyUI.SetActive(true);
                break;
            case SkillItemTypeEnum.ShopUnlcok:
                skillType.SetActive(true);
                unlockedUI.SetActive(true);
                break;
            case SkillItemTypeEnum.Relic:
                relicType.SetActive(true);
                availableUIEffect.SetActive(true);
                break;
        }
    }

    public void SetMoneyPriceText(string price)
    {
        moneyText.text = price;
    }

    public void SetShardPriceText(string price)
    {
        shardText.text = price;
    }


}
