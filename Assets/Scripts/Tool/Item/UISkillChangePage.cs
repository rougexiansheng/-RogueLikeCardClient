using System;
using UnityEngine;
using UnityEngine.UI;

public class UISkillChangePage : MonoBehaviour
{
    [SerializeField]
    public UISkillInfo CurrentInfoItem;

    [SerializeField]
    public UISkillInfo UpdateSkillInfo;

    [SerializeField]
    private Button cancelButton;

    [SerializeField]
    private Button replaceButton;

    [SerializeField]
    private Image replaceIcon;

    [SerializeField]
    private Button levelUpButton;
    [SerializeField]
    private Image levelUpIcon;

    [SerializeField]
    private Image maxLevelIcon;

    public void Init()
    {
        RemoveAllListeners();
        replaceIcon.gameObject.SetActive(false);
        replaceButton.gameObject.SetActive(false);
        levelUpIcon.gameObject.SetActive(false);
        levelUpButton.gameObject.SetActive(false);
        maxLevelIcon.gameObject.SetActive(false);

    }

    public void ResetStateUI()
    {
        replaceIcon.gameObject.SetActive(false);
        replaceButton.gameObject.SetActive(false);
        levelUpIcon.gameObject.SetActive(false);
        levelUpButton.gameObject.SetActive(false);
        maxLevelIcon.gameObject.SetActive(false);
        RemoveAllListeners();
    }

    public void OpenReplaceUI()
    {
        replaceIcon.gameObject.SetActive(true);
        replaceButton.gameObject.SetActive(true);
    }


    public void OpenLevelUpUI()
    {
        levelUpIcon.gameObject.SetActive(true);
        levelUpButton.gameObject.SetActive(true);
    }

    public void OpenMaxLevelUI()
    {
        maxLevelIcon.gameObject.SetActive(true);
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
