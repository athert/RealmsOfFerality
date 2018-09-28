using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkMessageResolve
{
    public static void Resolve(NetworkMessage.Type messageType, NetDataReader data)
    {
        switch (messageType)
        {
            case NetworkMessage.Type.server_loginAnswer:
                {
                    NetworkMessage.LoginType loginAnswer = (NetworkMessage.LoginType)data.GetByte();
                    if (loginAnswer == NetworkMessage.LoginType.correct)
                    {
                        //Game.GetPlayer().SetPlayerId(data.GetInt());
                        LoginScreenUI.SetCharacterSelectionWaitingCount(data.GetByte());
                        return;
                    }
                    LoginScreenUI.ShowInfo(loginAnswer.ToString());
                    break;
                }
            case NetworkMessage.Type.server_characterLoginInfo:
                {
                    int id = data.GetInt();
                    string name = data.GetString(data.GetByte());
                    Vector3 position = new Vector3(data.GetFloat(), data.GetFloat(), data.GetFloat());
                    float orientation = data.GetFloat();
                    int modelId = data.GetByte();

                    int index = Database.GetDBCharacterModelList().FindIndex(x => x.id == modelId);
                    if(index == -1)
                    {
                        Debug.LogError("Cannot find modelId '" + modelId + "'");
                        index = 0;
                    }

                    Entity ent = Database.CreateEntity();
                    ent.DisableMovement(true);
                    ent.SetId(id);
                    ent.name = name;
                    ent.SetPostStartAction(() =>
                    {
                        GameObject obj = GameObject.Find("SpawnPoint");
                        ent.transform.SetParent(obj.transform);
                        ent.transform.localPosition = Vector3.zero;
                        ent.SetOrientation(obj.transform.eulerAngles.y);
                        ent.GetVisualModule().AssignCharacterModelToEntity(Database.GetDBCharacterModelList()[index]);
                        LoginScreenUI.AddCharacterSelectionEntity(ent);
                    });

                    break;
                }
            case NetworkMessage.Type.server_requestMapLoading:
                {
                    //load some map
                    break;
                }
            case NetworkMessage.Type.server_createEntity:
                {
                    int id = data.GetInt();
                    string name = data.GetString(data.GetByte());
                    Vector3 position = new Vector3(data.GetFloat(), data.GetFloat(), data.GetFloat());
                    float orientation = data.GetFloat();
                    int modelId = data.GetByte();
                    Vector3 userInput = new Vector3(data.GetFloat(), data.GetFloat(), data.GetFloat());

                    int index = Database.GetDBCharacterModelList().FindIndex(x => x.id == modelId);
                    if (index == -1)
                    {
                        Debug.LogError("Cannot find modelId '" + modelId + "'");
                        index = 0;
                    }

                    Entity ent = Database.CreateEntity();
                    ent.SetId(id);
                    ent.name = name;
                    ent.SetPostStartAction(() =>
                    {
                        ent.SetPosition(position);
                        ent.SetOrientation(orientation);
                        ent.GetMovementModule().SetRequestInputs(userInput);
                        ent.GetVisualModule().AssignCharacterModelToEntity(Database.GetDBCharacterModelList()[index]);
                    });

                    break;
                }
            case NetworkMessage.Type.server_removeEntity:
                {
                    int entityId = data.GetInt();

                    Map tempMap = Game.GetMap();
                    if (tempMap == null)
                        return;

                    tempMap.RemoveEntity(entityId);
                    break;
                }
            case NetworkMessage.Type.server_setControllable:
                {
                    int controllableEntityId = data.GetInt();
                    bool isMainEntity = data.GetBool();

                    Map tempMap = Game.GetMap();
                    if (tempMap == null)
                    {
                        Network.AddEntityWaitingMessage(controllableEntityId, data);
                        return;
                    }

                    Entity tempEntity = tempMap.GetEntity(controllableEntityId);
                    if (tempEntity == null)
                    {
                        Network.AddEntityWaitingMessage(controllableEntityId, data);
                        return;
                    }

                    PlayerCamera.instance.transform.position = tempEntity.transform.position;
                    Game.GetPlayer().ControllableEntity = tempEntity;
                    if (isMainEntity)
                        Game.GetPlayer().SetPlayerId(tempEntity.GetId());
                    break;
                }
            case NetworkMessage.Type.client_movementSnapshot:
                {
                    int entityId = data.GetInt();

                    if (Game.GetMap() == null)
                        return;

                    Entity entity = Game.GetMap().GetEntity(entityId);

                    if (entity == null)
                        return;

                    int time = data.GetInt();
                    Vector3 position = new Vector3(data.GetFloat(), data.GetFloat(), data.GetFloat());
                    float rotation = data.GetFloat();
                    Vector3 inputs = new Vector3(data.GetSByte(), data.GetSByte(), data.GetSByte());

                    EntityMovement.MovementSnapshot snapshot = new EntityMovement.MovementSnapshot(time, entityId, position, rotation, inputs);
                    entity.GetMovementModule().AddSnapshot(snapshot);
                    break;
                }
            case NetworkMessage.Type.server_chatMessage:
                {
                    ChatUI.MessageType tempChatMessageType = (ChatUI.MessageType)data.GetByte();
                    int senderId = data.GetInt();
                    string message = data.GetString(data.GetShort());

                    if (Game.GetMap() == null)
                        return;

                    ChatUI.ReceiveChatMessage(Game.GetMap().GetEntity(senderId), tempChatMessageType, message);
                    break;
                }
            case NetworkMessage.Type.server_chatChannelInfo:
                {
                    bool addChatChannel = data.GetBool();
                    string chatChannelName = data.GetString(data.GetByte());

                    if (addChatChannel)
                    {
                        ChatUI.AddChatChannel(chatChannelName);
                    }
                    else
                    {
                        ChatUI.RemoveChatChannel(chatChannelName);
                    }
                    break;
                }
            case NetworkMessage.Type.server_entityTeleport:
                {
                    Vector3 position = new Vector3(data.GetFloat(), data.GetFloat(), data.GetFloat());
                    float rotation = data.GetFloat();

                    Game.GetPlayer().ControllableEntity.transform.position = position;
                    Game.GetPlayer().ControllableEntity.GetMovementModule().SetRequestRotation(rotation);
                    break;
                }
            case NetworkMessage.Type.server_inventoryItem:
                {
                    int id = data.GetShort();
                    int count = data.GetByte();

                    Game.GetPlayer().SetItem(id, count);
                    break;
                }
            case NetworkMessage.Type.server_guildInfo:
                {
                    int steps = 0;
                    while (!data.EndOfData && steps <= 10)
                    {
                        Player.GuildInfo.TimeInfo type = (Player.GuildInfo.TimeInfo)data.GetByte();
                        switch(type)
                        {
                            case Player.GuildInfo.TimeInfo.info:
                                {
                                    Game.GetPlayer().GetGuildInfo().info = data.GetString(data.GetShort());
                                    break;
                                }
                            case Player.GuildInfo.TimeInfo.announcement:
                                {
                                    Game.GetPlayer().GetGuildInfo().announcement = data.GetString(data.GetShort());
                                    break;
                                }
                            case Player.GuildInfo.TimeInfo.name:
                                {
                                    Game.GetPlayer().GetGuildInfo().name = data.GetString(data.GetByte());
                                    break;
                                }
                            case Player.GuildInfo.TimeInfo.members:
                                {
                                    Player.GuildInfo.Member[] tempMembers = new Player.GuildInfo.Member[data.GetByte()];
                                    for (int i = 0; i < tempMembers.Length; i++)
                                    {
                                        tempMembers[i] = new Player.GuildInfo.Member();
                                        tempMembers[i].id = data.GetInt();
                                        tempMembers[i].name = data.GetString(data.GetByte());
                                    }
                                    Game.GetPlayer().GetGuildInfo().members = new List<Player.GuildInfo.Member>(tempMembers);
                                    break;
                                }
                            case Player.GuildInfo.TimeInfo.ranks:
                                {
                                    Player.GuildInfo.Rank[] tempRanks = new Player.GuildInfo.Rank[data.GetByte()];
                                    for (int i = 0; i < tempRanks.Length; i++)
                                    {
                                        tempRanks[i] = new Player.GuildInfo.Rank();
                                        tempRanks[i].id = data.GetInt();
                                        tempRanks[i].name = data.GetString(data.GetByte());
                                        tempRanks[i].isMain = data.GetBool();
                                        tempRanks[i].isBasic = data.GetBool();
                                        tempRanks[i].canMemberInvite = data.GetBool();
                                        tempRanks[i].canMemberRemove = data.GetBool();
                                        tempRanks[i].canRankCreateRemove = data.GetBool();
                                        tempRanks[i].canChangeInfo = data.GetBool();
                                        tempRanks[i].canChangeAnnouncement = data.GetBool();
                                    }
                                    Game.GetPlayer().GetGuildInfo().ranks = new List<Player.GuildInfo.Rank>(tempRanks);
                                    break;
                                }
                            case Player.GuildInfo.TimeInfo.max:
                                {
                                    int memberCount = data.GetByte();
                                    if(memberCount != Game.GetPlayer().GetGuildInfo().members.Count)
                                    {
                                        //todo error?
                                    }
                                    for (int i = 0; i < Game.GetPlayer().GetGuildInfo().members.Count; i++)
                                    {
                                        Game.GetPlayer().GetGuildInfo().members[i].isOnline = data.GetBool();
                                        int rankId = data.GetInt();
                                        Game.GetPlayer().GetGuildInfo().members[i].rank = Game.GetPlayer().GetGuildInfo().ranks.Find(x => x.id == rankId);
                                    }
                                    break;
                                }
                            default:
                                {
                                    Debug.LogError("thefuck?");
                                    break;
                                }
                        }
                        steps++;
                    }
                    if(steps >= 10)
                    {
                        Debug.LogError("critical error happend in guildInfo Message. Maximum steps reached!");
                    }

                    GuildUI.GuildMessageArrived();
                    break;
                }
            case NetworkMessage.Type.server_setGuild:
                {
                    int currentId = Game.GetPlayer().GetGuildInfo().id;
                    Game.GetPlayer().GetGuildInfo().id = data.GetInt();

                    if (Game.GetPlayer().GetGuildInfo().id == -1)
                        GuildUI.ShowGuild(false);
                    else if (Game.GetPlayer().GetGuildInfo().id != -1 && currentId == -1 && GuildUI.IsGuildVisible())
                    {
                        GuildUI.ShowGuild(false);
                        GuildUI.ShowGuild(true);
                    }
                    break;
                }
            case NetworkMessage.Type.server_guildListInfo:
                {
                    Player.GuildInfo[] guildList = new Player.GuildInfo[data.GetShort()];
                    for (int i = 0; i < guildList.Length; i++)
                    {
                        guildList[i] = new Player.GuildInfo();
                        guildList[i].id = data.GetInt();
                        guildList[i].name = data.GetString(data.GetShort());
                        guildList[i].owner = data.GetString(data.GetByte());
                    }
                    GuildUI.GuildListArrived(new List<Player.GuildInfo>(guildList));
                    break;
                }
        }
    }

    public static void NetworkLoginRequest(string username, string password)
    {
        NetworkMessage msg = new NetworkMessage();
        msg.type = NetworkMessage.Type.client_loginRequest;
        msg.data.Put((byte)username.Length);
        msg.data.Put(username);
        msg.data.Put((byte)password.Length);
        msg.data.Put(password);
        msg.data.Put((byte)Game.GetGameVersion());
        msg.Send();
    }
    public static void NetworkJoinWorldRequest(int characterId)
    {
        NetworkMessage msg = new NetworkMessage();
        msg.type = NetworkMessage.Type.client_requestJoinToWorld;
        msg.data.Put(characterId);
        msg.Send();
    }
    public static void NetworkCharacterRemoveRequest(int characterId)
    {
        NetworkMessage msg = new NetworkMessage();
        msg.type = NetworkMessage.Type.client_requestRemoveCharacter;
        msg.data.Put(characterId);
        msg.Send();
    }
    public static void NetworkCharacterCreateRequest(string name, int raceId, int modelId)
    {
        NetworkMessage msg = new NetworkMessage();
        msg.type = NetworkMessage.Type.client_requestCreateCharacter;
        msg.data.Put((byte)name.Length);
        msg.data.Put(name);
        msg.data.Put((byte)raceId);
        msg.data.Put((byte)modelId);
        msg.Send();
    }
    public static void NetworkMovementSnapshotRequest(EntityMovement.MovementSnapshot snapshot)
    {
        NetworkMessage msg = new NetworkMessage();
        msg.type = NetworkMessage.Type.client_movementSnapshot;
        msg.data.Put(snapshot.id);
        msg.data.Put(snapshot.time);
        msg.data.Put(snapshot.position.x);
        msg.data.Put(snapshot.position.y);
        msg.data.Put(snapshot.position.z);
        msg.data.Put(snapshot.rotation);
        msg.data.Put((sbyte)snapshot.inputs.x);
        msg.data.Put((sbyte)snapshot.inputs.y);
        msg.data.Put((sbyte)snapshot.inputs.z);
        msg.Send();
    }
    public static void NetworkChatMessageRequest(ChatUI.MessageType type, string text, string targetName)
    {
        NetworkMessage msg = new NetworkMessage();
        msg.type = NetworkMessage.Type.client_chatMessage;
        msg.data.Put((byte)type);
        msg.data.Put((short)text.Length);
        msg.data.Put(text);
        msg.data.Put((byte)targetName.Length);
        msg.data.Put(targetName);
        msg.Send();
    }
    public static void NetworkGuildRequest(int id)
    {
        int[] times = Database.GetGuildTimes(id);

        NetworkMessage msg = new NetworkMessage();
        msg.type = NetworkMessage.Type.client_guildInfoRequest;
        msg.data.Put(id);
        for (int i = 0; i < times.Length; i++)
            msg.data.Put(times[i]);
        msg.Send();
    }
    public static void NetworkGuildRankAdd(Player.GuildInfo.Rank rank)
    {
        NetworkMessage msg = new NetworkMessage();
        msg.type = NetworkMessage.Type.client_guildRankCreateRequest;
        msg.data.Put(rank.id);
        msg.data.Put((byte)rank.name.Length);
        msg.data.Put(rank.name);
        msg.data.Put(rank.isMain);
        msg.data.Put(rank.isBasic);
        msg.data.Put(rank.canMemberInvite);
        msg.data.Put(rank.canMemberRemove);
        msg.data.Put(rank.canRankCreateRemove);
        msg.data.Put(rank.canChangeInfo);
        msg.data.Put(rank.canChangeAnnouncement);
        msg.Send();
    }
    public static void NetworkGuildRankRemove(Player.GuildInfo.Rank rank)
    {
        NetworkMessage msg = new NetworkMessage();
        msg.type = NetworkMessage.Type.client_guildRankRemoveRequest;
        msg.data.Put(rank.id);
        msg.Send();
    }
    public static void NetworkGuildAnnouncementTextChange(string text)
    {
        NetworkMessage msg = new NetworkMessage();
        msg.type = NetworkMessage.Type.client_guildAnnouncementChange;
        msg.data.Put((short)text.Length);
        msg.data.Put(text);
        msg.Send();
    }
    public static void NetworkGuildInfoTextChange(string text)
    {
        NetworkMessage msg = new NetworkMessage();
        msg.type = NetworkMessage.Type.client_guildInfoChange;
        msg.data.Put((short)text.Length);
        msg.data.Put(text);
        msg.Send();
    }
    public static void NetworkGuildMemberInviteRequest(string name)
    {
        NetworkMessage msg = new NetworkMessage();
        msg.type = NetworkMessage.Type.client_guildMemberInviteRequest;
        msg.data.Put((byte)name.Length);
        msg.data.Put(name);
        msg.Send();
    }
    public static void NetworkGuildMemberRemoveRequest(int id)
    {
        NetworkMessage msg = new NetworkMessage();
        msg.type = NetworkMessage.Type.client_guildMemberRemoveRequest;
        msg.data.Put(id);
        msg.Send();
    }
    public static void NetworkGuildMemberRankChange(int id, int rankId)
    {
        NetworkMessage msg = new NetworkMessage();
        msg.type = NetworkMessage.Type.client_guildMemberRankChangeRequest;
        msg.data.Put(id);
        msg.data.Put(rankId);
        msg.Send();
    }
    public static void NetworkGuildCreateRequest(string name)
    {
        NetworkMessage msg = new NetworkMessage();
        msg.type = NetworkMessage.Type.client_guildCreateRequest;
        msg.data.Put((byte)name.Length);
        msg.data.Put(name);
        msg.Send();
    }
    public static void NetworkGuildRequestGuildList()
    {
        NetworkMessage msg = new NetworkMessage();
        msg.type = NetworkMessage.Type.client_guildListRequest;
        msg.Send();
    }
    public static void NetworkGuildApplicationInviteRequest(int id)
    {
        NetworkMessage msg = new NetworkMessage();
        msg.type = NetworkMessage.Type.client_guildApplicationInviteRequest;
        msg.data.Put(id);
        msg.Send();
    }
}