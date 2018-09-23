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
    #endregion

    public static void ShowInfo(string text)
    {
        if (instance == null)
            return;

        instance.infoPageHolder.gameObject.SetActive(true);
        instance.infoPageHolder.GetChild(0).GetComponent<Text>().text = text;
    }
    public static void SetCharacterSelectionWaitingCount(int count)
    {
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
        characterSelectionList[currentCharacterSelected].gameObject.SetActive(true);
    }
    public void CreateCharacterButtonClick()
    {
        characterPageHolder.gameObject.SetActive(false);
        characterCreateHolder.gameObject.SetActive(true);
    }
    public void CharacterCreationBackButtonClick()
    {
        characterPageHolder.gameObject.SetActive(true);
        characterCreateHolder.gameObject.SetActive(false);
    }
    public void CharacterCreationCreateButtonClick()
    {
        characterSelectionWaitingCount++;
        ShowLoading(true);
        characterCreateHolder.gameObject.SetActive(false);

    }
    public void RemoveCharacterButtonClick()
    {
        if (currentCharacterSelected == -1)
            return;

        NetworkMessageResolve.NetworkRemoveCharacterRequest(characterSelectionList[currentCharacterSelected].GetId());
        DestroyImmediate(characterSelectionList[currentCharacterSelected].gameObject);
        characterSelectionList.RemoveAt(currentCharacterSelected);
        CharacterSelectionReconstruction();
    }
    public void ExitGameButtonClick()
    {
        Application.Quit();
    }
    #endregion

    #region private
    private static LoginScreen instance;
    private int characterSelectionWaitingCount;
    private int currentCharacterSelected = -1;
    private List<Entity> characterSelectionList = new List<Entity>();
    private List<Transform> characterSelectionButtonList = new List<Transform>();

    private void Awake()
    {
        instance = this; 
    }
    private void CheckIfCharactersAreLoaded()
    {
        if (characterSelectionList.Count >= characterSelectionWaitingCount)
        {
            if (loginPageHolder.gameObject.activeSelf)
            {
                loginPageHolder.gameObject.SetActive(false);
                characterPageHolder.gameObject.SetActive(true);
            }
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
    #endregion
}
