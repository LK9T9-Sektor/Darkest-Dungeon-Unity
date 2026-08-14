using System;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    private RawImage rawImage;
    private Animator screenAnimator;

    public event Action EventFadeEnded;
    public event Action EventAppearEnded;

    private void Awake()
    {
        screenAnimator = GetComponent<Animator>();
        rawImage = GetComponent<RawImage>();
        rawImage.enabled = false;
    }

    public void StartFaded()
    {
        Debug.Log("[DD] [SCENE] ScreenFader.StartFaded");
        rawImage.enabled = true;
        screenAnimator.SetTrigger("initial_fade");
    }

    public void Fade(float speed = 1)
    {
        Debug.Log("[DD] [SCENE] ScreenFader.Fade speed=" + speed);
        rawImage.enabled = true;
        screenAnimator.SetBool("fade", true);
        screenAnimator.speed = speed;
    }

    public void Appear(float speed = 1)
    {
        Debug.Log("[DD] [SCENE] ScreenFader.Appear speed=" + speed);
        screenAnimator.SetBool("appear", true);
        screenAnimator.speed = speed;
    }

    public void Reset()
    {
        Debug.Log("[DD] [SCENE] ScreenFader.Reset");
        screenAnimator.speed = 1;
        screenAnimator.SetBool("appear", false);
        screenAnimator.SetBool("fade", false);
        screenAnimator.SetTrigger("reset");
    }

    public void FadeEnded()
    {
        Debug.Log("[DD] [SCENE] ScreenFader.FadeEnded");
        screenAnimator.SetBool("fade", false);

        if (EventFadeEnded != null)
            EventFadeEnded();
    }

    public void AppearEnded()
    {
        Debug.Log("[DD] [SCENE] ScreenFader.AppearEnded");
        screenAnimator.SetBool("appear", false);
        rawImage.enabled = false;

        if (EventAppearEnded != null)
            EventAppearEnded();
    }
}
