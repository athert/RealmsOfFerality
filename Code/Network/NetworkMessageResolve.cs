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
                    bool loginAccepted = data.GetBool();
                    int userId = data.GetInt();

                    break;
                }
            case NetworkMessage.Type.server_createEntity:
                {
                    int id = data.GetInt();
                    string name = data.GetString(data.GetByte());
                    Vector3 position = new Vector3(data.GetFloat(), data.GetFloat(), data.GetFloat());
                    float orientation = data.GetFloat();

                    Entity ent = GameObject.FindObjectOfType<Database>().CreateEntity();
                    ent.SetId(id);
                    ent.name = name;
                    ent.SetPostStartAction(() => 
                    {
                        ent.SetPosition(position);
                        ent.SetOrientation(orientation);
                    }); 

                    break;
                }
            case NetworkMessage.Type.server_removeEntity:
                {

                    break;
                }
            case NetworkMessage.Type.server_setControllable:
                {
                    int controllable = data.GetInt();

                    List<Entity> entityList = new List<Entity>(GameObject.FindObjectsOfType<Entity>());
                    int index = entityList.FindIndex(x => x.GetId() == controllable);
                    if (index == -1)
                    {
                        Network.AddEntityWaitingMessage(controllable, data);
                        return;
                    }

                    Game.GetPlayer().ControllableEntity = entityList[index];
                    break;
                }
        }
    }

    public static void NetworkLoginRequest(string username, string password)
    {
        NetworkMessage msg = new NetworkMessage();
        msg.type = NetworkMessage.Type.client_loginRequest;
        msg.data.Put(username.Length);
        msg.data.Put(username);
        msg.data.Put(password.Length);
        msg.data.Put(password);
        msg.Send();
    }
}