using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Sektor.DarkestDungeon.Lan.Contracts.Results;

// ReSharper disable PossibleLossOfFraction

public class RoomSelector : MonoBehaviour
{
    [SerializeField]
    private List<MultiplayerRoomSlot> roomSlots;
    [SerializeField]
    private Button startCampaignButton;
    [SerializeField]
    private Button returnButton;
    [SerializeField]
    private Button refreshButton;
    [SerializeField]
    private RectTransform saveFrame;
    [SerializeField]
    private RectTransform sceneryRect;
    [SerializeField]
    private Text versionLabel;
    [SerializeField]
    private Text regionLabel;

    [SerializeField]
    private Button playButton;
    [SerializeField]
    private InputField nicknameField;
    [SerializeField]
    private Image progressPanel;
    [SerializeField]
    private Text progressLabel;

    public Button StartCampaignButton { get { return startCampaignButton; } }
    public Button ReturnButton { get { return returnButton; } }
    public Text RegionLabel { get { return regionLabel; } }
    public Image ProgressPanel { get { return progressPanel; } }
    public Text ProgressLabel { get { return progressLabel; } }

    private List<MultiplayerRoomSlot> RoomSlots { get { return roomSlots; } }
    private Button PlayButton { get { return playButton; } }
    private InputField NicknameField { get { return nicknameField; } }
    private Button RefreshButton { get { return refreshButton; } }
    private RectTransform SaveFrame { get { return saveFrame; } }
    private RectTransform SceneryRect { get { return sceneryRect; } }
    private Text VersionLabel { get { return versionLabel; } }

    private static readonly List<string> RoomNameTemplates = new List<string>
    {
        "Lepoundmaster", "Lepersader", "Leprusader", "Cruleper",
        "Crusoundmaster", "Crusaster", "Lepster", "Jesleper",
        "Jesteper", "Jesterer", "Jesterper", "Jesteraster",
        "Hourusader", "Manatahunter", "Mancultist", "Moccultist",
        "Manataltist", "Manatarmist" ,"Manatartist", "Bountester",
        "Occulatarms", "Occulthunter", "Occultiser", "Vestalhunter",
        "Gravestal", "Graverosader", "Abomirobber", "Abominestal",
        "Abomisader", "Grabomination", "Vestalnation", "Arbalestal",
    };

    private bool isSelecting;
    private MultiplayerRoomSlot selectedRoomSlot;

    private IEnumerator sliderCoroutine;
    private IEnumerator slideBackCoroutine;
    private IEnumerator fadeCoroutine;

    private void Awake()
    {
        Random.InitState(GetInstanceID() + System.DateTime.Now.Millisecond);

        foreach (MultiplayerRoomSlot slot in RoomSlots)
            slot.RoomSelector = this;

        MultiplayerSync.SessionJoined += OnSteamSessionJoined;
        MultiplayerSync.SessionEnded += OnSteamSessionEnded;

        VersionLabel.text = DarkestPhotonLauncher.GameVersion;
    }

    private void OnDestroy()
    {
        MultiplayerSync.SessionJoined -= OnSteamSessionJoined;
        MultiplayerSync.SessionEnded -= OnSteamSessionEnded;
    }

    private void Start()
    {
        SaveFrame.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isSelecting)
            return;

        if (selectedRoomSlot != null)
        {
            if (Input.GetMouseButtonDown(0))
                selectedRoomSlot.RefocusInput();
        }
        else if (Input.GetKeyUp(KeyCode.Escape) && fadeCoroutine == null)
        {
            isSelecting = false;
            StopAllCoroutines();
            slideBackCoroutine = SliderBack();
            StartCoroutine(slideBackCoroutine);
        }
    }

    public string GenerateRoomName()
    {
        return RoomNameTemplates[Random.Range(0, RoomNameTemplates.Count)]
            + "#" + Random.Range(1, 1000).ToString().PadLeft(3, '0');
    }

    public void RefreshRoomList()
    {
        if (MultiplayerSync.IsSteamProvider)
        {
            CleanRoomList();
            return;
        }

        DarkestPhotonLauncher.Instanse.ConnectToMaster();

        if (PhotonNetwork.insideLobby)
        {
            var roomList = PhotonNetwork.GetRoomList();
            int roomsUpdated = Mathf.Min(RoomSlots.Count, roomList.Length);

            for (int i = 0; i < roomsUpdated; i++)
                RoomSlots[i].LoadSaveFrame(roomList[i]);

            for (int i = roomsUpdated; i < RoomSlots.Count; i++)
                RoomSlots[i].LoadSaveFrame(null);
        }
        else
        {
            CleanRoomList();
        }
    }

    public void CleanRoomList()
    {
        foreach (MultiplayerRoomSlot slot in RoomSlots)
            slot.LoadSaveFrame(null);
    }

    public void FadeToLoadingScreen()
    {
        DisableInteraction();
        fadeCoroutine = SceneFade(1, 2500);
        StartCoroutine(fadeCoroutine);
    }

    /// <summary>
    /// Entry point of the "Multiplayer" button: opens the provider selection menu.
    /// Choosing a provider initializes it and opens the shared room list, where the
    /// legacy hero selection is reused for both Photon and Steam.
    /// </summary>
    public void SaveSelectionStart()
    {
        MultiplayerProviderMenu.Open();
    }

    /// <summary>
    /// Opens the shared multiplayer room list (save frame slide-up). Provider agnostic:
    /// hero selection, room slots and status panel are reused by both providers.
    /// </summary>
    public void OpenRoomList()
    {
        CampaignSelectionManager.OnSelectionStart(CampaignSelection.Multiplayer);
        SaveFrame.gameObject.SetActive(true);

        sliderCoroutine = SceneSlider();
        StartCoroutine(sliderCoroutine);
    }

    public void SaveNamingStart(MultiplayerRoomSlot namingSaveSlot)
    {
        DarkestSoundManager.PlayOneShot("event:/general/title_screen/letter_open");
        namingSaveSlot.TitleInput.text = MultiplayerSync.IsSteamProvider ? "" : GenerateRoomName();
        selectedRoomSlot = namingSaveSlot;
        DisableInteraction();
    }

    public void RoomNamingCompleted()
    {
        selectedRoomSlot = null;
    }

    public void ReturnButtonClicked()
    {
        if(isSelecting)
        {
            isSelecting = false;
            slideBackCoroutine = SliderBack();
            StartCoroutine(slideBackCoroutine);
        }
    }

    public void DisableInteraction()
    {
        RoomSlots.ForEach(slot => slot.DisableInteraction());

        PlayButton.interactable = false;
        NicknameField.interactable = false;
        ReturnButton.interactable = false;
        RefreshButton.interactable = false;
    }

    public void EnableInteraction()
    {
        RoomSlots.ForEach(slot => slot.EnableInteraction());

        PlayButton.interactable = true;
        NicknameField.interactable = true;
        ReturnButton.interactable = true;
        RefreshButton.interactable = true;
    }

    public void PlayButtonClicked()
    {
        if (MultiplayerSync.IsSteamProvider)
        {
            HostSteamSession();
            return;
        }

        DarkestPhotonLauncher.Instanse.RandomConnect();
    }

    /// <summary>
    /// Hosts a new Steam session: validates the selected heroes, captures the party and
    /// creates a lobby. The session join handler loads the raid scene once the lobby is
    /// created. Errors are reported through the shared status panel.
    /// </summary>
    private void HostSteamSession()
    {
        if (!DarkestPhotonLauncher.Instanse.CheckSelectedSkills())
            return;

        MultiplayerPartyData party = MultiplayerSync.BuildLocalPartyData();
        if (party == null)
        {
            ProgressLabel.text = "Build your party first!";
            return;
        }

        DisableInteraction();
        ProgressLabel.enabled = true;
        ProgressPanel.enabled = true;
        ProgressLabel.text = "Creating Steam session...";

        Result result = MultiplayerSync.Steam.HostSession();
        if (!result.IsSuccess)
        {
            ProgressLabel.text = "Host failed: " + result.ErrorMessage;
            EnableInteraction();
        }
    }

    /// <summary>
    /// Joins an existing Steam session by its lobby id, entered into a room slot.
    /// Validates the selected heroes, captures the party and connects to the lobby;
    /// the session join handler loads the raid scene once connected.
    /// </summary>
    public void JoinSteamLobby(string lobbyId)
    {
        if (string.IsNullOrEmpty(lobbyId))
        {
            ProgressLabel.text = "Enter the Steam lobby ID first.";
            return;
        }

        if (!DarkestPhotonLauncher.Instanse.CheckSelectedSkills())
            return;

        MultiplayerPartyData party = MultiplayerSync.BuildLocalPartyData();
        if (party == null)
        {
            ProgressLabel.text = "Build your party first!";
            return;
        }

        DisableInteraction();
        ProgressLabel.enabled = true;
        ProgressPanel.enabled = true;
        ProgressLabel.text = "Joining Steam session " + lobbyId + "...";

        Result result = MultiplayerSync.Steam.JoinSession(lobbyId);
        if (!result.IsSuccess)
        {
            ProgressLabel.text = "Join failed: " + result.ErrorMessage;
            EnableInteraction();
        }
    }

    /// <summary>
    /// Raised when a Steam session was created or joined: captures the local party,
    /// delivers it to the rival and loads the raid scene, mirroring the Photon flow.
    /// </summary>
    private void OnSteamSessionJoined(string sessionId)
    {
        ProgressLabel.text = "Lobby joined! Waiting for the opponent...";
        ProgressLabel.enabled = true;
        ProgressPanel.enabled = true;
        Debug.Log("[MULTIPLAYER] Steam session joined: " + sessionId);

        MultiplayerSync.EnsureLocalPartyData();
        SteamSessionManager sessionManager = MultiplayerSync.Steam;
        if (sessionManager != null)
            sessionManager.SendPartyConfig(MultiplayerSync.LocalPartyData);

        StartCoroutine(LoadSteamRaidSceneRoutine());
    }

    /// <summary>Raised when the Steam session ended while the room list is open.</summary>
    private void OnSteamSessionEnded()
    {
        if (!isSelecting)
            return;

        CleanRoomList();
        ProgressLabel.text = "Session closed.";
    }

    private IEnumerator LoadSteamRaidSceneRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        if (MultiplayerSync.IsSteamSession)
            MultiplayerSync.LoadLevel("DungeonMultiplayer");
    }

    private IEnumerator SceneSlider()
    {
        DarkestSoundManager.PlayOneShot("event:/general/title_screen/campaign_button");

        bool steam = MultiplayerSync.IsSteamProvider;
        ProgressLabel.text = steam
            ? "Steam ready. Enter a lobby ID in a slot or press Play to host."
            : (PhotonNetwork.connected ? "Connected!" : "Disconnected!");

        DisableInteraction();

        while (true)
        {
            if (470 < SceneryRect.offsetMax.y)
                break;

            Vector2 offsetMax = SceneryRect.offsetMax;
            offsetMax.y += Time.deltaTime * 800;
            SceneryRect.offsetMax = offsetMax;
            Vector2 offsetMin = SceneryRect.offsetMin;
            offsetMin.y += Time.deltaTime * 800;
            SceneryRect.offsetMin = offsetMin;
            yield return null;
        }

        if (steam)
        {
            MultiplayerSync.EnsureSteamSession();
            if (!MultiplayerSync.Steam.IsInitialized)
                ProgressLabel.text = "Steam unavailable: " + MultiplayerSync.Steam.LastError;
        }
        else
        {
            DarkestPhotonLauncher.Instanse.ConnectToMaster();
        }
        yield return new WaitForSeconds(1f);
        isSelecting = true;

        if (steam)
            CleanRoomList();
        else
            RefreshRoomList();
        EnableInteraction();
    }

    private IEnumerator SliderBack()
    {
        if (MultiplayerSync.IsSteamProvider)
        {
            if (MultiplayerSync.Steam != null && MultiplayerSync.Steam.IsSessionActive)
                MultiplayerSync.Steam.LeaveSession();
        }
        else
        {
            PhotonNetwork.Disconnect();
        }

        SaveFrame.gameObject.SetActive(false);
        ReturnButton.gameObject.SetActive(false);

        while (true)
        {
            if (SceneryRect.offsetMax.y <= 0 || SceneryRect.offsetMin.y <= 0)
            {
                SceneryRect.offsetMax = Vector2.zero;
                SceneryRect.offsetMin = Vector2.zero;
                break;
            }

            Vector2 offsetMax = SceneryRect.offsetMax;
            offsetMax.y -= Time.deltaTime * 1200;
            SceneryRect.offsetMax = offsetMax;
            Vector2 offsetMin = SceneryRect.offsetMin;
            offsetMin.y -= Time.deltaTime * 1200;
            SceneryRect.offsetMin = offsetMin;
            yield return 0;
        }
        isSelecting = false;
        CampaignSelectionManager.OnSelectionReturn();
    }

    private IEnumerator SceneFade(float seconds, float speed)
    {
        CampaignSelectionManager.Instanse.TitleRect.SetParent(SceneryRect, false);

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(SceneryRect,
            new Vector2(Screen.width / 2, Screen.height), DarkestDungeonManager.Instanse.MainUICamera, out localPoint);
        CampaignSelectionManager.Instanse.TitleRect.localPosition = localPoint;

        if (DarkestSoundManager.TitleMusicInstanse != null)
        {
            float titleVolume;
            DarkestSoundManager.TitleMusicInstanse.getVolume(out titleVolume);

            for (float nextVolume = titleVolume; nextVolume >= 0; nextVolume -= Time.deltaTime * 3f)
            {
                DarkestSoundManager.TitleMusicInstanse.setVolume(nextVolume);
                yield return null;
            }
        }

        DarkestSoundManager.StopTitleMusic();
        DarkestSoundManager.PlayOneShot("event:/general/title_screen/start_game");

        while (true)
        {
            if (seconds <= 0)
                break;

            seconds -= Time.deltaTime;

            Vector2 offsetMax = SceneryRect.offsetMax;
            offsetMax.y += Time.deltaTime * speed;
            SceneryRect.offsetMax = offsetMax;
            Vector2 offsetMin = SceneryRect.offsetMin;
            offsetMin.y += Time.deltaTime * speed;
            SceneryRect.offsetMin = offsetMin;
            yield return null;
        }

        PhotonNetwork.LoadLevel("DungeonMultiplayer");
    }
}