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
                        Game.GetPlayer().SetPlayerId(data.GetInt());
                        LoginScreen.SetCharacterSelectionWaitingCount(data.GetByte());
                        return;
                    }
                    LoginScreen.ShowInfo(loginAnswer.ToString());
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
                        LoginScreen.AddCharacterSelectionEntity(ent);
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
                    Chat.MessageType tempChatMessageType = (Chat.MessageType)data.GetByte();
                    int senderId = data.GetInt();
                    string message = data.GetString(data.GetShort());

                    if (Game.GetMap() == null)
                        return;

                    Chat.ReceiveChatMessage(Game.GetMap().GetEntity(senderId), tempChatMessageType, message);
                    break;
                }
            case NetworkMessage.Type.server_chatChannelInfo:
                {
                    bool addChatChannel = data.GetBool();
                    string chatChannelName = data.GetString(data.GetByte());

                    if (addChatChannel)
                    {
                        Chat.AddChatChannel(chatChannelName);
                    }
                    else
                    {
                        Chat.RemoveChatChannel(chatChannelName);
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
    public static void NetworkChatMessageRequest(Chat.MessageType type, string text, string targetName)
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
}