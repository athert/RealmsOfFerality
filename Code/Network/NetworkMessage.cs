using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkMessage
{
    public enum Type
    {
        client_loginRequest,
        server_loginAnswer,

        server_requestMapLoading,
        server_createEntity,
        server_entity_setVisual,
        server_entity_setInventory,
        server_entity_setAnimation,
        server_removeEntity,
        server_movementUpdate,
    }

    public Type type;
    public NetDataWriter data = new NetDataWriter();

    public void Send(SendOptions sendOptions = SendOptions.ReliableOrdered)
    {
        Debug.Assert(Game.GetPlayer() != null, "Trying to send message without player initialized!");

        NetDataWriter netData = new NetDataWriter();
        int playerId = Game.GetPlayer().GetPlayerId();

        netData.Put((byte)type);
        netData.Put(playerId);
        netData.Put(data.Data);
        Network.SendMessage(netData, sendOptions);
    }
}
