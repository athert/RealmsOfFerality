using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuildUI : MonoBehaviour
{
    #region public
    public Transform guildPanel;

    public Transform activeGuildHolder;
    public Transform inactiveGuildHolder;

    public Transform addMemberButton;
    public Transform rankManagerButton;

    public Text guildName;
    public Transform guildListPanel;
    public Transform guildListPrefab;
    public InputField guildCreateName;

    public Transform characterPanelContent;
    public Transform characterButtonPrefab;

    public Transform announcementPanel;
    public InputField announcementText;
    public Transform announcementSubmitButton;

    public Transform inviteMemberPanel;
    public InputField inviteMemberName;

    public Transform rankManagerPanel;
    public Transform rankContent;
    public Transform rankButtonPrefab;
    public Transform rankSettingsHolder;
    public Transform rankCreateButton;
    public InputField rankName;
    public Toggle rankIsMain;
    public Toggle rankIsBasic;
    public Toggle rankInviteMembers;
    public Toggle rankRemoveMembers;
    public Toggle rankCreateRemoveRanks;
    public Toggle rankChangeAnnouncement;
    public Toggle rankChangeInfo;

    public Transform infoPanel;
    public InputField infoPanelText;
    public Transform infoSubmitButton;

    public Transform memberPanel;
    public InputField memberName;
    public Dropdown memberRankDropdown;
    public Transform memberRemoveButton;

    public Transform waitPanel;

    public static void ShowGuild(bool show)
    {
        instance.isGuildVisible = show;
        if (show)
        {
            if (Game.GetPlayer().GetGuildInfo().id == -1)
            {
                instance.inactiveGuildHolder.gameObject.SetActive(true);
                instance.ShowWaitPanel();
                NetworkMessageResolve.NetworkGuildRequestGuildList();
            }
            else
            {
                instance.activeGuildHolder.gameObject.SetActive(true);
                instance.ShowWaitPanel();
                NetworkMessageResolve.NetworkGuildRequest(Game.GetPlayer().GetGuildInfo().id);
            }
        }
        else
        {
            instance.inactiveGuildHolder.gameObject.SetActive(false);
            instance.activeGuildHolder.gameObject.SetActive(false);

            instance.HideAnnouncement();
            instance.HideInfoPanel();
            instance.HideRankManager();
            instance.HideMemberManager();
            instance.HideInviteMember();
        }
    }
    public static void GuildMessageArrived()
    {
        instance.HideWaitPanel();

        Player.GuildInfo.Member tempPlayerMember = Game.GetPlayer().GetGuildInfo().members.Find(x => x.id == Game.GetPlayer().GetPlayerId());
        if (tempPlayerMember != null)
        {
            instance.announcementSubmitButton.gameObject.SetActive(tempPlayerMember.rank.canChangeAnnouncement);
            instance.announcementText.interactable = tempPlayerMember.rank.canChangeAnnouncement;
            instance.infoSubmitButton.gameObject.SetActive(tempPlayerMember.rank.canChangeInfo);
            instance.infoPanelText.interactable = tempPlayerMember.rank.canChangeInfo;
            instance.addMemberButton.gameObject.SetActive(tempPlayerMember.rank.canMemberInvite);
            instance.rankManagerButton.gameObject.SetActive(tempPlayerMember.rank.canRankCreateRemove);
            instance.memberRankDropdown.gameObject.SetActive(tempPlayerMember.rank.canRankCreateRemove);
            instance.memberRemoveButton.gameObject.SetActive(tempPlayerMember.rank.canMemberRemove);
        }
        else
        {
            instance.announcementSubmitButton.gameObject.SetActive(false);
            instance.announcementText.interactable = false;
            instance.infoSubmitButton.gameObject.SetActive(false);
            instance.infoPanelText.interactable = false;
            instance.addMemberButton.gameObject.SetActive(false);
            instance.rankManagerButton.gameObject.SetActive(false);
            instance.memberRankDropdown.gameObject.SetActive(false);
            instance.memberRemoveButton.gameObject.SetActive(false);
        }

        instance.guildName.text = Game.GetPlayer().GetGuildInfo().name;
        for(int i=0;i< instance.guildMemberButtonList.Count;i++)
        {
            DestroyImmediate(instance.guildMemberButtonList[i].gameObject);
        }
        instance.guildMemberButtonList.Clear();

        for(int i=0;i< Game.GetPlayer().GetGuildInfo().members.Count;i++)
        {
            Player.GuildInfo.Member tempMember = Game.GetPlayer().GetGuildInfo().members[i];

            Transform tempButton = Instantiate(instance.characterButtonPrefab);
            tempButton.SetParent(instance.characterPanelContent);
            tempButton.GetComponent<Button>().onClick.AddListener(() => instance.OnClickGuildMemberButton(tempMember));
            tempButton.Find("Name").GetComponent<Text>().text = tempMember.name;
            tempButton.Find("Rank").GetComponent<Text>().text = tempMember.rank.name;
            tempButton.Find("Location").GetComponent<Text>().text = "Unknown";
            tempButton.Find("IsOnline").GetComponent<Image>().color = tempMember.isOnline ? Color.green : Color.red;
            tempButton.gameObject.SetActive(true);
            instance.guildMemberButtonList.Add(tempButton);
        }

        for (int i = 0; i < instance.guildRanksButtonList.Count; i++)
        {
            DestroyImmediate(instance.guildRanksButtonList[i].gameObject);
        }
        instance.guildRanksButtonList.Clear();

        for (int i = 0; i < Game.GetPlayer().GetGuildInfo().ranks.Count; i++)
        {
            Player.GuildInfo.Rank tempRank = Game.GetPlayer().GetGuildInfo().ranks[i];

            Transform tempButton = Instantiate(instance.rankButtonPrefab);
            tempButton.SetParent(instance.rankContent);
            tempButton.GetComponent<Button>().onClick.AddListener(() => instance.OnClickRankButton(tempRank));
            tempButton.GetComponentInChildren<Text>().text = tempRank.name;
            tempButton.gameObject.SetActive(true);
            instance.guildRanksButtonList.Add(tempButton);
        }

        instance.infoPanelText.text = Game.GetPlayer().GetGuildInfo().info;
        instance.announcementText.text = Game.GetPlayer().GetGuildInfo().announcement;
    }
    public static void GuildListArrived(List<Player.GuildInfo> list)
    {
        instance.HideWaitPanel();
        instance.tempGuildApplicationList = list;
        instance.guildCreateName.text = "";

        for (int i = 0; i < instance.guildApplicationButtonList.Count; i++)
        {
            DestroyImmediate(instance.guildApplicationButtonList[i].gameObject);
        }
        instance.guildApplicationButtonList.Clear();

        for (int i = 0; i < list.Count; i++)
        {
            int guildId = list[i].id;
            Transform tempButton = Instantiate(instance.guildListPrefab);
            tempButton.SetParent(instance.guildListPanel);
            tempButton.Find("RequestButton").GetComponent<Button>().onClick.AddListener(() => instance.SendGuildRequest(guildId));
            tempButton.Find("Name").GetComponent<Text>().text = list[i].name;
            tempButton.Find("Owner").GetComponent<Text>().text = list[i].owner;
            tempButton.gameObject.SetActive(true);
            instance.guildApplicationButtonList.Add(tempButton);
        }
    }
    public static bool IsGuildVisible()
    {
        return instance.isGuildVisible;
    }
    #region public announcement
    public static void SetAnnouncement(string text)
    {

    }
    public void ShowAnnouncement()
    {
        announcementPanel.gameObject.SetActive(true);
    }
    public void HideAnnouncement()
    {
        announcementPanel.gameObject.SetActive(false);
    }
    public void SubmitAnnouncement()
    {
        NetworkMessageResolve.NetworkGuildAnnouncementTextChange(announcementText.text);
    }
    #endregion
    #region public inviteMember
    public void ShowInviteMember()
    {
        inviteMemberName.text = "";
        inviteMemberPanel.gameObject.SetActive(true);
    }
    public void HideInviteMember()
    {
        inviteMemberPanel.gameObject.SetActive(false);
    }
    public void InviteMember()
    {
        NetworkMessageResolve.NetworkGuildMemberInviteRequest(inviteMemberName.text);
        inviteMemberName.text = "";
    }
    #endregion
    #region public rankManager
    public void ShowRankManager()
    {
        rankManagerPanel.gameObject.SetActive(true);
    }
    public void HideRankManager()
    {
        rankManagerPanel.gameObject.SetActive(false);
        rankCreateButton.gameObject.SetActive(true);
    }
    public void CreateRank()
    {
        int nextId = 0;
        for(int i=0;i<Game.GetPlayer().GetGuildInfo().ranks.Count;i++)
        {
            if (Game.GetPlayer().GetGuildInfo().ranks[i].id > nextId)
                nextId = Game.GetPlayer().GetGuildInfo().ranks[i].id;
        }
        nextId++;

        rankToCreate = new Player.GuildInfo.Rank();
        rankToCreate.id = nextId;
        rankToCreate.name = "new rank";

        AdjustRankSettings(rankToCreate);
    }
    public void SubmitRank()
    {
        if (rankName.text.Replace(" ", "") == "")
            return;

        ShowWaitPanel();
        rankCreateButton.gameObject.SetActive(true);
        rankSettingsHolder.gameObject.SetActive(false);

        rankToCreate.name = rankName.text;
        rankToCreate.isBasic = rankIsBasic.isOn;
        rankToCreate.isMain = rankIsMain.isOn;
        rankToCreate.canMemberInvite = rankInviteMembers.isOn;
        rankToCreate.canMemberRemove = rankRemoveMembers.isOn;
        rankToCreate.canRankCreateRemove = rankCreateRemoveRanks.isOn;
        rankToCreate.canChangeInfo = rankChangeInfo.isOn;
        rankToCreate.canChangeAnnouncement = rankChangeAnnouncement.isOn;

        NetworkMessageResolve.NetworkGuildRankAdd(rankToCreate);
        rankToCreate = null;
    }
    public void RemoveRank()
    {
        if (rankToCreate == null)
            return;

        ShowWaitPanel();
        NetworkMessageResolve.NetworkGuildRankRemove(rankToCreate);
        rankSettingsHolder.gameObject.SetActive(false);
    }
    #endregion
    #region public infoPanel
    public void ShowInfoPanel()
    {
        infoPanel.gameObject.SetActive(true);
    }
    public void HideInfoPanel()
    {
        infoPanel.gameObject.SetActive(false);
    }
    public void SubmitInfo()
    {
        NetworkMessageResolve.NetworkGuildInfoTextChange(infoPanelText.text);
    }
    #endregion
    #region public memberManager
    private bool ignoreRankChange = false;
    public void ShowMemberManager()
    {
        if (selectedMember == null)
            return;

        int memberRankIndex = 0;
        List<string> stringRanks = new List<string>();

        memberName.text = selectedMember.name;
        ignoreRankChange = true;
        memberRankDropdown.ClearOptions();

        for (int i = 0; i < Game.GetPlayer().GetGuildInfo().ranks.Count; i++)
        {
            if (Game.GetPlayer().GetGuildInfo().ranks[i].id == selectedMember.rank.id)
                memberRankIndex = i;
            stringRanks.Add(Game.GetPlayer().GetGuildInfo().ranks[i].name);
        }

        memberRankDropdown.AddOptions(stringRanks);
        memberRankDropdown.value = memberRankIndex;
        memberPanel.gameObject.SetActive(true);
        ignoreRankChange = false;
    }
    public void HideMemberManager()
    {
        memberPanel.gameObject.SetActive(false);
        selectedMember = null;
    }
    public void ChangeMemberRank(int rankIndex)
    {
        if (ignoreRankChange || selectedMember == null)
            return;

        NetworkMessageResolve.NetworkGuildMemberRankChange(selectedMember.id, Game.GetPlayer().GetGuildInfo().ranks[rankIndex].id);
    }
    public void MemberRemoveFromGuild()
    {
        if (selectedMember == null)
            return;

        NetworkMessageResolve.NetworkGuildMemberRemoveRequest(selectedMember.id);
    }
    #endregion
    #region public waitPanel
    public void ShowWaitPanel()
    {
        waitPanel.gameObject.SetActive(true);
    }
    public void HideWaitPanel()
    {
        waitPanel.gameObject.SetActive(false);
    }
    #endregion
    #region public inactiveGuild
    public void CreateGuild()
    {
        if (guildCreateName.text.Replace(" ", "") == "")
            return;

        NetworkMessageResolve.NetworkGuildCreateRequest(guildCreateName.text);
    }
    public void SendGuildRequest(int guildId)
    {
        NetworkMessageResolve.NetworkGuildApplicationInviteRequest(guildId);
    }
    #endregion
    #endregion

    #region private
    private static GuildUI instance;
    private bool isGuildVisible;

    private List<Transform> guildMemberButtonList = new List<Transform>();
    private List<Transform> guildRanksButtonList = new List<Transform>();
    private List<Transform> guildApplicationButtonList = new List<Transform>();
    private List<Player.GuildInfo> tempGuildApplicationList = new List<Player.GuildInfo>();
    private Player.GuildInfo.Rank rankToCreate = null;
    private Player.GuildInfo.Member selectedMember = null;

    private void Awake()
    {
        instance = this;
    }
    private void OnClickGuildMemberButton(Player.GuildInfo.Member member)
    {
        selectedMember = member;
        ShowMemberManager();
    }
    private void OnClickRankButton(Player.GuildInfo.Rank rank)
    {
        rankToCreate = rank;
        AdjustRankSettings(rank);
    }
    private void AdjustRankSettings(Player.GuildInfo.Rank rank)
    {
        rankName.text = rank.name;
        rankIsMain.isOn = rank.isMain;
        rankIsBasic.isOn = rank.isBasic;
        rankInviteMembers.isOn = rank.canMemberInvite;
        rankRemoveMembers.isOn = rank.canMemberRemove;
        rankCreateRemoveRanks.isOn = rank.canRankCreateRemove;
        rankChangeInfo.isOn = rank.canChangeInfo;
        rankChangeAnnouncement.isOn = rank.canChangeAnnouncement;

        rankInviteMembers.interactable = !rank.isMain;
        rankRemoveMembers.interactable = !rank.isMain;
        rankCreateRemoveRanks.interactable = !rank.isMain;
        rankChangeInfo.interactable = !rank.isMain;
        rankChangeAnnouncement.interactable = !rank.isMain;

        rankSettingsHolder.gameObject.SetActive(true);
        //rankCreateButton.gameObject.SetActive(false);
    }
    #endregion
}
