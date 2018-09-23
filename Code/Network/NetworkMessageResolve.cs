using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkMessageResolve
{
    public static void Resolve(NetworkMessage.Type messageType, NetDataReader data)
    {
        Debug.Log(messageType.ToString());
        switch (messageType)
        {
            case NetworkMessage.Type.server_loginAnswer:
                {
                    NetworkMessage.LoginType loginAnswer = (NetworkMessage.LoginType)data.GetByte();
                    if(loginAnswer == NetworkMessage.LoginType.correct)
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

                    Entity ent = GameObject.FindObjectOfType<Database>().CreateEntity();
                    ent.DisableMovement(true);
                    ent.SetId(id);
                    ent.name = name;
                    ent.SetPostStartAction(() =>
                    {
                        GameObject obj = GameObject.Find("SpawnPoint");
                        ent.transform.SetParent(obj.transform);
                        ent.transform.localPosition = Vector3.zero;
                        ent.SetOrientation(obj.transform.eulerAngles.y);
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
                    Vector3 userInput = new Vector3(data.GetFloat(), data.GetFloat(), data.GetFloat());

                    Entity ent = GameObject.FindObjectOfType<Database>().CreateEntity();
                    ent.SetId(id);
                    ent.name = name;
                    ent.SetPostStartAction(() => 
                    {
                        ent.SetPosition(position);
                        ent.SetOrientation(orientation);
                        ent.GetMovementModule().SetRequestInputs(userInput);
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
                    if(tempMap == null)
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
    public static void NetworkRemoveCharacterRequest(int characterId)
    {

    }
}