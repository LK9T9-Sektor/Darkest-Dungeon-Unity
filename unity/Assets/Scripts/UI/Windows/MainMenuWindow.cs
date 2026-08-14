using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuWindow : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup uiCanvasGroup;

    public CanvasGroup UICanvasGroup { private get { return uiCanvasGroup; } set { uiCanvasGroup = value; } }
    public bool IsOpened { get { return gameObject.activeSelf; } }

    public event Action EventWindowClosed;

    public void OpenMenu()
    {
        Debug.Log("[DD] [MENU] MainMenuWindow.OpenMenu: uiCanvasGroup=" + (uiCanvasGroup == null ? "null" : "set"));
        gameObject.SetActive(true);
        DarkestDungeonManager.GamePaused = true;
        if (UICanvasGroup != null)
            UICanvasGroup.blocksRaycasts = false;
    }

    public void WindowClosed()
    {
        Debug.Log("[DD] [MENU] MainMenuWindow.WindowClosed: uiCanvasGroup=" + (uiCanvasGroup == null ? "null" : "set"));
        DarkestDungeonManager.GamePaused = false;
        gameObject.SetActive(false);

        if (EventWindowClosed != null)
            EventWindowClosed();
        if (UICanvasGroup != null)
            UICanvasGroup.blocksRaycasts = true;
    }

    public void ReturnToCampaignSelection()
    {
        Debug.Log("[DD] [MENU] MainMenuWindow.ReturnToCampaignSelection: scene " + SceneManager.GetActiveScene().name);
        if(SceneManager.GetActiveScene().name == "DungeonMultiplayer")
        {
            WindowClosed();
            RaidSceneManager.Instanse.AbandonButtonClicked();
            return;
        }
        else if(SceneManager.GetActiveScene().name == "EstateManagement")
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
        Debug.Log("[MAINMENU] QUIT GAME REQUESTED. STOPPING PLAY MODE IN EDITOR OR QUITTING APPLICATION IN BUILD.");
        if (SceneManager.GetActiveScene().name == "DungeonMultiplayer")
        {
            RaidSceneManager.Instanse.OnSceneLeave();
            MultiplayerSync.LeaveRoom();
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
            if(!RaidSceneManager.HasAnyEvents)
            {
                DarkestDungeonManager.SaveData.UpdateFromRaid();
                DarkestDungeonManager.Instanse.SaveGame();
            }
            RaidSceneManager.Instanse.OnSceneLeave();
        }
        DarkestSoundManager.SilenceNarrator();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}