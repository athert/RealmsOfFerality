using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginScreen : MonoBehaviour
{
    #region public
    public Transform loginPageHolder;
    public InputField login_username;
    public InputField login_password;

    public Transform infoPageHolder;
    public Transform loadingHolder;

    #region public characterSelection
    public Transform characterPageHolder;
    public Transform selection_buttonHolder;
    public Transform selection_button;
    #endregion

    #region public characterCreation
    public Transform characterCreateHolder;
    public InputField creation_name;
    public Dropdown creation_raceDropdown;
    public Dropdown creation_ageDropdown;
    public Dropdown creation_genderDropdown;
    public Dropdown creation_maneDropdown;
    public Dropdown creation_tailDropdown;
    public Dropdown creation_earsDropdown;
    public Dropdown creation_tuftsDropdown;
    public Dropdown creation_marking_headDropdown;
    public Dropdown creation_marking_tailDropdown;
    public Dropdown creation_marking_eyeDropdown;
    public Dropdown creation_marking_bodyDropdown;
    public Slider creation_scale_muzzle;
    public Slider creation_scale_chin;
    public Slider creation_scale_nose;
    public Slider creation_scale_forehead;
    public Slider creation_scale_ear;
    public Slider creation_scale_retina;
    public Slider creation_scale_body;
    public Text creation_color_text_r;
    public Text creation_color_text_g;
    public Text creation_color_text_b;
    public Transform creation_color_body;
    public Transform creation_color_lowerBody;
    public Transform creation_color_nose;
    public Transform creation_color_eyes;
    public Transform creation_color_upperEyes;
    public Transform creation_color_lowerEyes;
    public Transform creation_color_tailTip;
    public Transform creation_color_mane;
    public Transform creation_color_marking;
    public Transform creation_color_paws;
    #endregion

    public static void ShowInfo(string text)
    {
        if (instance == null)
            return;

        instance.ShowLoading(false);
        instance.infoPageHolder.gameObject.SetActive(true);
        instance.infoPageHolder.GetChild(0).GetComponent<Text>().text = text;
    }
    public static void SetCharacterSelectionWaitingCount(int count)
    {
        instance.loginPageHolder.gameObject.SetActive(false);
        instance.characterSelectionWaitingCount = count;
        instance.CheckIfCharactersAreLoaded();
    }
    public static void AddCharacterSelectionEntity(Entity entity)
    {
        instance.characterSelectionList.Add(entity);
        instance.CheckIfCharactersAreLoaded();
    }

    public void LoginToServer()
    {
        if (login_username.text.Replace(" ", "") == "")
            return;

        if (login_password.text.Replace(" ", "") == "")
            return;

        NetworkMessageResolve.NetworkLoginRequest(login_username.text, login_password.text);
        ShowLoading(true);
    }
    public void JoinWorldButtonClick()
    {
        if(currentCharacterSelected == -1)
        {
            ShowInfo("You need to select your character first!");
            return;
        }
        Game.LevelLoadRequest();
        NetworkMessageResolve.NetworkJoinWorldRequest(characterSelectionList[currentCharacterSelected].GetId());
    }
    public void ShowInfoButtonClick()
    {
        infoPageHolder.gameObject.SetActive(false);
    }
    public void CharacterSelectionButtonClick(int index)
    {
        if(currentCharacterSelected != -1)
            characterSelectionList[currentCharacterSelected].gameObject.SetActive(false);

        currentCharacterSelected = index;

        if(currentCharacterSelected != -1)
            characterSelectionList[currentCharacterSelected].gameObject.SetActive(true);
    }

    public void RemoveCharacterButtonClick()
    {
        if (currentCharacterSelected == -1)
            return;

        characterSelectionWaitingCount--;
        NetworkMessageResolve.NetworkCharacterRemoveRequest(characterSelectionList[currentCharacterSelected].GetId());
        Game.GetMap().RemoveEntity(characterSelectionList[currentCharacterSelected].GetId());
        characterSelectionList.RemoveAt(currentCharacterSelected);
        CharacterSelectionReconstruction();
    }
    public void CreateCharacterButtonClick()
    {
        if(currentCharacterSelected >= 5)
        {
            ShowInfo("You cannot create more characters! You need to remove character before creating new one.");
            return;
        }
        OnCharacterCreationShow();
        characterPageHolder.gameObject.SetActive(false);
        characterCreateHolder.gameObject.SetActive(true);
    }
    public void CharacterCreationBackButtonClick()
    {
        OnCharacterCreationHide();
        characterPageHolder.gameObject.SetActive(true);
        characterCreateHolder.gameObject.SetActive(false);
    }
    public void CharacterCreationCreateButtonClick()
    {
        OnCharacterCreationCreateRequest();
        OnCharacterCreationHide();
        characterSelectionWaitingCount++;
        ShowLoading(true);
        characterCreateHolder.gameObject.SetActive(false);
    }
    public void ExitGameButtonClick()
    {
        Application.Quit();
    }

    public void CreationRaceChanged(int index)
    {
        freezeModelRequest = true;
        List<string> dropdownOptions = new List<string>();
        // ======================================================= AGE ==========================================================
        creation_ageDropdown.ClearOptions();
        dropdownOptions.Clear();
        for (int i = 0; i < Database.GetDBRaceList()[index].ages.Length; i++)
        {
            dropdownOptions.Add(Database.GetDBRaceList()[index].ages[i].name);
        }
        creation_ageDropdown.AddOptions(dropdownOptions);

        // ======================================================= GENDER ==========================================================
        creation_genderDropdown.ClearOptions();
        dropdownOptions.Clear();
        for (int i = 0; i < Database.GetDBRaceList()[index].genders.Length; i++)
        {
            dropdownOptions.Add(Database.GetDBRaceList()[index].genders[i].name);
        }
        creation_genderDropdown.AddOptions(dropdownOptions);

        // ======================================================= XXXXX ==========================================================
        creation_genderDropdown.value = 0;
        creation_ageDropdown.value = 0;
        freezeModelRequest = false;
        OnCharacterCreationModelRequest();
    }
    public void CreationGenderChanged(int index)
    {
        if (freezeModelRequest)
            return;

        OnCharacterCreationModelRequest();
    }
    public void CreationAgeChanged(int index)
    {
        if (freezeModelRequest)
            return;

        OnCharacterCreationModelRequest();
    }

    public void CreationManeChanged(int index)
    {
        if(index == 0)
        {
            creationDummyEntity.GetVisualModule().RemoveCharacterSubModel(Database.db_character_submodel.Type.mane);
            return;
        }
        
        index -= 1; //index 0 is always "none"
        creationDummyEntity.GetVisualModule().AssignCharacterSubModelToEntity(creationManeList[index]);
    }
    public void CreationEarsChanged(int index)
    {
        if (index == 0)
        {
            creationDummyEntity.GetVisualModule().RemoveCharacterSubModel(Database.db_character_submodel.Type.ears);
            return;
        }

        index -= 1; //index 0 is always "none"
        creationDummyEntity.GetVisualModule().AssignCharacterSubModelToEntity(creationEarsList[index]);
    }
    public void CreationTailChanged(int index)
    {
        if (index == 0)
        {
            creationDummyEntity.GetVisualModule().RemoveCharacterSubModel(Database.db_character_submodel.Type.tail);
            return;
        }

        index -= 1; //index 0 is always "none"
        creationDummyEntity.GetVisualModule().AssignCharacterSubModelToEntity(creationTailList[index]);
    }
    public void CreationTuftsChanged(int index)
    {
        if (index == 0)
        {
            creationDummyEntity.GetVisualModule().RemoveCharacterSubModel(Database.db_character_submodel.Type.tufts);
            return;
        }

        index -= 1; //index 0 is always "none"
        creationDummyEntity.GetVisualModule().AssignCharacterSubModelToEntity(creationTuftList[index]);
    }

    public void CreationColorChangeBody(float value)
    {
        CreationColorChanges(creation_color_body);
    }
    public void CreationColorChangeLowerBody(float value)
    {
        CreationColorChanges(creation_color_lowerBody);
    }
    public void CreationColorChangeNose(float value)
    {
        CreationColorChanges(creation_color_nose);
    }
    public void CreationColorChangeEyes(float value)
    {
        CreationColorChanges(creation_color_eyes);
    }
    public void CreationColorChangeUpperEyes(float value)
    {
        CreationColorChanges(creation_color_upperEyes);
    }
    public void CreationColorChangeLowerEyes(float value)
    {
        CreationColorChanges(creation_color_lowerEyes);
    }
    public void CreationColorChangeTailTip(float value)
    {
        CreationColorChanges(creation_color_tailTip);
    }
    public void CreationColorChangeMane(float value)
    {
        CreationColorChanges(creation_color_mane);
    }
    public void CreationColorChangeMarking(float value)
    {
        CreationColorChanges(creation_color_marking);
    }
    public void CreationColorChangePaws(float value)
    {
        CreationColorChanges(creation_color_paws);
    }
    #endregion

    #region private
    private static LoginScreen instance;
    private int characterSelectionWaitingCount;
    private int currentCharacterSelected = -1;
    private bool freezeModelRequest = false;
    private Entity creationDummyEntity;
    private List<Entity> characterSelectionList = new List<Entity>();
    private List<Transform> characterSelectionButtonList = new List<Transform>();

    private List<Database.db_character_submodel> creationManeList = new List<Database.db_character_submodel>();
    private List<Database.db_character_submodel> creationTailList = new List<Database.db_character_submodel>();
    private List<Database.db_character_submodel> creationEarsList = new List<Database.db_character_submodel>();
    private List<Database.db_character_submodel> creationTuftList = new List<Database.db_character_submodel>();

    private void Awake()
    {
        instance = this; 
    }
    private void Update()
    {
        
    }
    private void CheckIfCharactersAreLoaded()
    {
        if (characterSelectionList.Count >= characterSelectionWaitingCount)
        {
            characterPageHolder.gameObject.SetActive(true);
            CharacterSelectionReconstruction();
            ShowLoading(false);
        }
    }
    private void CharacterSelectionReconstruction()
    {
        for (int i = 0; i < characterSelectionButtonList.Count; i++)
        {
            DestroyImmediate(characterSelectionButtonList[i].gameObject);
        }
        characterSelectionButtonList.Clear();
        currentCharacterSelected = -1;

        for (int i = 0; i < characterSelectionList.Count; i++)
        {
            int index = i;
            characterSelectionList[i].gameObject.SetActive(false);

            Transform tempButton = Instantiate(selection_button);
            tempButton.SetParent(selection_buttonHolder, false);
            tempButton.GetComponent<Button>().onClick.AddListener(() => CharacterSelectionButtonClick(index));
            tempButton.GetComponentInChildren<Text>().text = characterSelectionList[i].name;
            tempButton.gameObject.SetActive(true);
            tempButton.localPosition = new Vector3(0, -60 * i, 0);
            characterSelectionButtonList.Add(tempButton);
        }

        if(characterSelectionButtonList.Count > 0)
            CharacterSelectionButtonClick(0);
    }
    private void ShowLoading(bool show)
    {
        loadingHolder.gameObject.SetActive(show);
    }
    private void OnCharacterCreationShow()
    {
        CharacterSelectionButtonClick(-1);

        //apply color
        SetColorToParent(creation_color_body, Color.yellow);

        creationDummyEntity = Database.CreateEntity();
        creationDummyEntity.SetPostStartAction(() =>
        {
            GameObject obj = GameObject.Find("SpawnPoint");
            creationDummyEntity.transform.SetParent(obj.transform);
            creationDummyEntity.transform.localPosition = Vector3.zero;
            creationDummyEntity.SetOrientation(obj.transform.eulerAngles.y);
        });

        List<string> dropdownOptions = new List<string>();
        creation_name.text = "";
        #region OnCharacterCreationShow InitializeDropdowns
        // ======================================================= RACE ==========================================================
        creation_raceDropdown.ClearOptions();
        dropdownOptions.Clear();
        for (int i = 0; i < Database.GetDBRaceList().Count; i++)
        {
            dropdownOptions.Add(Database.GetDBRaceList()[i].name);
        }
        creation_raceDropdown.AddOptions(dropdownOptions);
        creation_raceDropdown.value = 0;
        CreationRaceChanged(0);
        #endregion
    }
    private void OnCharacterCreationHide()
    {
        DestroyImmediate(creationDummyEntity.gameObject);
        creationDummyEntity = null;

        if (characterSelectionButtonList.Count > 0)
            CharacterSelectionButtonClick(0);
    }
    private void OnCharacterCreationCreateRequest()
    { 
        NetworkMessageResolve.NetworkCharacterCreateRequest(creation_name.text, Database.GetDBRaceList()[creation_raceDropdown.value].id, creationDummyEntity.GetVisualModule().GetCharacterModel().id);
    }
    private void OnCharacterCreationModelRequest()
    {
        if(creationDummyEntity.GetVisualModule() == null)
        {
            creationDummyEntity.SetPostStartAction(() => { OnCharacterCreationModelRequest(); });
            return;
        }

        Database.db_character_race tempRace = Database.GetDBRaceList()[creation_raceDropdown.value];
        Database.db_character_age tempAge = tempRace.ages[creation_ageDropdown.value];
        Database.db_character_gender tempGender = tempRace.genders[creation_genderDropdown.value];

        int conditionIndex = Database.GetDBCharacterModelConditionList().FindIndex(x => x.age.id == tempAge.id && x.gender.id == tempGender.id && x.race.id == tempRace.id);
        Debug.Assert(conditionIndex != -1, "There is no model conditions for age:" + tempAge.id + " gender:" + tempGender.id + " race:" + tempRace.id + " !");

        Database.db_character_modelCondition condition = Database.GetDBCharacterModelConditionList()[conditionIndex];
        creationDummyEntity.GetVisualModule().AssignCharacterModelToEntity(condition.model);

        creationManeList = Database.GetDBCharacterSubModelBySpecific(condition.model, Database.db_character_submodel.Type.mane);
        creationTailList = Database.GetDBCharacterSubModelBySpecific(condition.model, Database.db_character_submodel.Type.tail);
        creationEarsList = Database.GetDBCharacterSubModelBySpecific(condition.model, Database.db_character_submodel.Type.ears);
        creationTuftList = Database.GetDBCharacterSubModelBySpecific(condition.model, Database.db_character_submodel.Type.tufts);

        List<string> dropdownOptions = new List<string>();
        creation_maneDropdown.ClearOptions();
        dropdownOptions.Clear();
        dropdownOptions.Add("None");
        for (int i = 0; i < creationManeList.Count; i++)
        {
            dropdownOptions.Add(creationManeList[i].name);
        }
        creation_maneDropdown.AddOptions(dropdownOptions);

        creation_tailDropdown.ClearOptions();
        dropdownOptions.Clear();
        dropdownOptions.Add("None");
        for (int i = 0; i < creationTailList.Count; i++)
        {
            dropdownOptions.Add(creationTailList[i].name);
        }
        creation_tailDropdown.AddOptions(dropdownOptions);

        creation_earsDropdown.ClearOptions();
        dropdownOptions.Clear();
        dropdownOptions.Add("None");
        for (int i = 0; i < creationEarsList.Count; i++)
        {
            dropdownOptions.Add(creationEarsList[i].name);
        }
        creation_earsDropdown.AddOptions(dropdownOptions);

        creation_tuftsDropdown.ClearOptions();
        dropdownOptions.Clear();
        dropdownOptions.Add("None");
        for (int i = 0; i < creationTuftList.Count; i++)
        {
            dropdownOptions.Add(creationTuftList[i].name);
        }
        creation_tuftsDropdown.AddOptions(dropdownOptions);

        creation_maneDropdown.value = 0;
        creation_tailDropdown.value = 0;
        creation_earsDropdown.value = 0;
        creation_tuftsDropdown.value = 0;
    }

    private Color32 CreationColorChanges(Transform parent)
    {
        Color32 tempColor = new Color32();
        tempColor.r = (byte)parent.Find("BodyR").GetComponent<Slider>().value;
        tempColor.g = (byte)parent.Find("BodyG").GetComponent<Slider>().value;
        tempColor.b = (byte)parent.Find("BodyB").GetComponent<Slider>().value;
        tempColor.a = 255;

        creation_color_text_r.text = "" + tempColor.r;
        creation_color_text_g.text = "" + tempColor.g;
        creation_color_text_b.text = "" + tempColor.b;

        parent.Find("BodyImage").GetComponent<Image>().color = tempColor;
        return tempColor;
    }
    private void SetColorToParent(Transform parent, Color32 color)
    {
        parent.Find("BodyR").GetComponent<Slider>().value = color.r;
        parent.Find("BodyG").GetComponent<Slider>().value = color.g;
        parent.Find("BodyB").GetComponent<Slider>().value = color.b;

        CreationColorChanges(parent);
    }
    #endregion
}
