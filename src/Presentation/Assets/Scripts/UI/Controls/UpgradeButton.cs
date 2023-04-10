using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using Assets.Scripts.Sounds;

public class UpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image buildingIcon;
    public Image infoIcon;
    public SkeletonAnimation upgradePulse;
    public Animator iconAnimator;

    public bool IsOpened { get; set; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buildingIcon.material.SetFloat("_BrightnessAmount", 1.35f);
        DarkestSoundManager.Instanse.PlayOneShot("event:/ui/town/button_mouse_over");
    }

    public void SwitchUpgrades()
    {
        IsOpened = !IsOpened;
        iconAnimator.SetBool("IsOpened", IsOpened);
        DarkestSoundManager.Instanse.PlayOneShot(IsOpened ? "event:/ui/town/page_open" : "event:/ui/town/page_close");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buildingIcon.material.SetFloat("_BrightnessAmount", 1f);
    }
}
