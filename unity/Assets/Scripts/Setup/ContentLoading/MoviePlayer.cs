using UnityEngine;

#if !UNITY_WEBGL
public class MoviePlayer : MonoBehaviour
{
    private GameIntro gameIntro;

    private void Awake()
    {
        gameIntro = GetComponentInParent<GameIntro>();
    }

    public void Play()
    {
        // BROKEN after Unity upgrade 2017 -> 6.4: gameIntro is null (GetComponentInParent finds no GameIntro),
        // causing a NullReferenceException here. Commented out until the intro flow is reworked.
        // gameIntro.FinishIntro();
    }
}
#endif