using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuWindow : MonoBehaviour
{
    public Button closeButton;

    public event WindowEvent onWindowClose;

    public bool IsOpened
    {
        get
        {
            return gameObject.activeSelf;
        }
    }

    public void OpenMenu()
    {
        gameObject.SetActive(true);
        DarkestDungeonManager.GamePaused = true;

        if (UICanvasGroup != null)
        {
            UICanvasGroup.blocksRaycasts = false;
        }
    }

    #region UICanvasGroup

    public CanvasGroup UICanvasGroup { get; private set; }
    public void UICanvasGroupSet(CanvasGroup canvasGroup)
    {
        UICanvasGroup = canvasGroup;
    }

    #endregion

    public void WindowClosed()
    {
        DarkestDungeonManager.GamePaused = false;
        gameObject.SetActive(false);

        if (onWindowClose != null)
            onWindowClose();

        if (UICanvasGroup != null)
        {
            UICanvasGroup.blocksRaycasts = true;
        }        
    }

    public void ReturnToCampaignSelection()
    {
        if (SceneManager.GetActiveScene().name == "DungeonMultiplayer")
        {
            WindowClosed();
            RaidSceneManager.Instanse.AbandonButtonClicked();
            return;
        }
        else if (SceneManager.GetActiveScene().name == "EstateManagement")
        {
            EstateSceneManager.Instanse.OnSceneLeave();
            DarkestDungeonManager.SaveData.UpdateFromEstate();
            DarkestDungeonManager.Instanse.SaveGame();
        }
        else if (SceneManager.GetActiveScene().name == "Dungeon")
        {
            if (!RaidSceneManager.HasAnyEvents)
            {
                DarkestDungeonManager.SaveData.UpdateFromRaid();
                DarkestDungeonManager.Instanse.SaveGame();
            }
            RaidSceneManager.Instanse.OnSceneLeave();
        }
        DarkestSoundManager.SilenceNarrator();
        SceneManager.LoadScene("CampaignSelection");
        WindowClosed();
    }

    public void QuitGame()
    {
        if (SceneManager.GetActiveScene().name == "DungeonMultiplayer")
        {
            RaidSceneManager.Instanse.OnSceneLeave();
            PhotonGameManager.Instanse.LeaveRoom();
            WindowClosed();
            return;
        }
        else if (SceneManager.GetActiveScene().name == "EstateManagement")
        {
            EstateSceneManager.Instanse.OnSceneLeave();
            DarkestDungeonManager.SaveData.UpdateFromEstate();
            DarkestDungeonManager.Instanse.SaveGame();
        }
        else if (SceneManager.GetActiveScene().name == "Dungeon")
        {
            if (!RaidSceneManager.HasAnyEvents)
            {
                DarkestDungeonManager.SaveData.UpdateFromRaid();
                DarkestDungeonManager.Instanse.SaveGame();
            }
            RaidSceneManager.Instanse.OnSceneLeave();
        }
        DarkestSoundManager.SilenceNarrator();
        Application.Quit();
    }
}