using UnityEngine;
using UnityEngine.SceneManagement;

#if !UNITY_WEBGL
public class GameIntro : MonoBehaviour
{
    [SerializeField]
    private MoviePlayer gameMovie;

    private GameLogo[] gameLogos;
    private int currentLogo;

    private void Awake()
    {
        gameLogos = transform.GetComponentsInChildren<GameLogo>(true);
    }

    private void Start()
    {
        if (currentLogo < gameLogos.Length)
            gameLogos[currentLogo].Play();
	}

    public void LogoEnded()
    {
        if (++currentLogo < gameLogos.Length)
            gameLogos[currentLogo].Play();
        else
        {
            // Intro movie disabled after Unity 2017 -> 6.4 migration (MovieTexture removed,
            // MoviePlayer is now a stub). Jump straight to the campaign selection instead.
            // gameMovie.Play();
            FinishIntro();
        }
    }

    public void FinishIntro()
    {
        SceneManager.LoadScene("CampaignSelection");
    }
}
#endif
