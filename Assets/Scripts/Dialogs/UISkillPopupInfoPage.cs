using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UISkillPopupInfoPage : UISkillInfo
{
    [SerializeField]
    public Button CancelButton;
    public ObservableEventTrigger eventTrigger;
}
