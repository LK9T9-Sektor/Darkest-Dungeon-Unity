using UnityEngine;
using UnityEngine.UI;
using System;

public class SaveSlot : MonoBehaviour
{
    [SerializeField]
    private int slotId;

    public SaveSelector SaveSelector { private get; set; }

    private Button saveSlotButton;
    private Text title;
    private InputField titleInput;
    private Text location;
    private Text currentWeek;
    private RectTransform nukeFrame;
    private Button nukeSaveButton;
    private Animator saveEnvelopeAnimator;

    private SaveCampaignData saveData;

    private void Awake()
    {
        saveSlotButton = transform.Find("Save").GetComponent<Button>();
        title = saveSlotButton.transform.Find("Title").GetComponent<Text>();
        titleInput = saveSlotButton.transform.Find("Title").GetComponent<InputField>();
        location = saveSlotButton.transform.Find("Current Location").GetComponent<Text>();
        currentWeek = saveSlotButton.transform.Find("Current Week").GetComponent<Text>();
        nukeFrame = transform.Find("NukeSaveFrame").GetComponent<RectTransform>();
        nukeSaveButton = nukeFrame.GetComponentInChildren<Button>();
        saveEnvelopeAnimator = transform.Find("SaveEnvelope").GetComponent<Animator>();
    }

    public void LoadSaveFrame()
    {
        saveData = SaveLoadManager.ReadSave(slotId);
        Debug.Log("[DD] [SAVE] SaveSlot " + slotId + ".LoadSaveFrame: " + (saveData == null ? "empty" : "populated '" + saveData.HamletTitle + "'"));

        if (saveData == null)
            FillEmptySave();
        else
            FillPopulatedSave();
    }

    public void SaveButtonClick()
    {
        if(saveData == null)
        {
            Debug.Log("[DD] [SAVE] SaveSlot " + slotId + ".SaveButtonClick: empty slot, starting naming");
            title.color = Color.white;
            titleInput.interactable = true;
            titleInput.enabled = true;
            titleInput.Select();
            titleInput.ActivateInputField();
            SaveSelector.SaveNamingStart(this);
        }
        else
        {
            Debug.Log("[DD] [SAVE] SaveSlot " + slotId + ".SaveButtonClick: loading slot '" + saveData.HamletTitle +
                "' InRaid=" + saveData.InRaid);
            DarkestDungeonManager.SaveData = saveData;
            if(DarkestDungeonManager.SaveData.InRaid)
            {
                if (!DarkestDungeonManager.SaveData.Quest.IsPlotQuest || 
                    DarkestDungeonManager.SaveData.Quest.Goal.Id == "tutorial_final_room")
                    DarkestDungeonManager.LoadingInfo.SetNextScene("Dungeon",
                        "Screen/loading_screen." + DarkestDungeonManager.SaveData.Quest.Dungeon + "_0");
                else
                    DarkestDungeonManager.LoadingInfo.SetNextScene("Dungeon",
                        "Screen/loading_screen.plot_" + (DarkestDungeonManager.SaveData.Quest as PlotQuest).Id);
            }
            else
                DarkestDungeonManager.LoadingInfo.SetNextScene("EstateManagement", "Screen/loading_screen.town_visit");

            Debug.Log("[DD] [SAVE] SaveSlot " + slotId + ".SaveButtonClick: next scene " +
                DarkestDungeonManager.LoadingInfo.NextScene + " / " + DarkestDungeonManager.LoadingInfo.TextureName);
            DarkestDungeonManager.Instanse.LoadSave();
            SaveSelector.FadeToLoadingScreen();
            Debug.Log("[DD] [SAVE] SaveSlot " + slotId + ".SaveButtonClick: LoadSave + FadeToLoadingScreen invoked");
        }
    }

    public void NukeButtonClick()
    {
        Debug.Log("[DD] [SAVE] SaveSlot " + slotId + ".NukeButtonClick: deleting save");
        SaveLoadManager.DeleteSave(slotId);
        DarkestSoundManager.PlayOneShot("event:/ui/town/button_click");
        FillEmptySave();
    }

    public void SaveNamingCompleted()
    {
        if(titleInput.text.Length == 0)
        {
            Debug.Log("[DD] [SAVE] SaveSlot " + slotId + ".SaveNamingCompleted: empty name, aborting naming");
            FillEmptySave();
            SaveSelector.SaveNamingCompleted();
            return;
        }
        Debug.Log("[DD] [SAVE] SaveSlot " + slotId + ".SaveNamingCompleted: naming save '" + titleInput.text + "'");
        saveData = new SaveCampaignData();
        saveData.HamletTitle = titleInput.text;
        saveData.SaveId = slotId;
        SaveLoadManager.WriteStartingSave(saveData);
        saveData = SaveLoadManager.ReadSave(slotId);
        Debug.Log("[DD] [SAVE] SaveSlot " + slotId + ".SaveNamingCompleted: after write+read saveData is " +
            (saveData == null ? "null" : "set"));
        FillPopulatedSave();

        titleInput.interactable = false;
        titleInput.enabled = false;
        DarkestSoundManager.PlayOneShot("event:/ui/town/button_click");
        SaveSelector.SaveNamingCompleted();
    }

    public void RefocusInput()
    {
        titleInput.Select();
        titleInput.ActivateInputField();
    }

    public void EnableInteraction()
    {
        saveSlotButton.interactable = true;
        if(nukeFrame.gameObject.activeSelf)
            nukeSaveButton.interactable = true;
    }

    public void DisableInteraction()
    {
        saveSlotButton.interactable = false;
        if (nukeFrame.gameObject.activeSelf)
            nukeSaveButton.interactable = false;
    }

    private void FillEmptySave()
    {
        saveData = null;
        Color color;
        ColorUtility.TryParseHtmlString("#323232FF", out color);
        title.color = color;
        title.text = "Click here to begin campaign...";
        location.text = "";
        currentWeek.text = "";
        saveEnvelopeAnimator.SetBool("Opened", false);
        nukeFrame.gameObject.SetActive(false);
    }

    private void FillPopulatedSave()
    {
        Color color;
        ColorUtility.TryParseHtmlString("#FFDB77FF", out color);
        title.color = color;
        title.text = saveData.HamletTitle;
        location.text = String.Format("In: {0}", saveData.LocationName);
        currentWeek.text = String.Format("Week {0}", saveData.CurrentWeek);
        saveEnvelopeAnimator.SetBool("Opened", true);
        nukeFrame.gameObject.SetActive(true);
    }
}