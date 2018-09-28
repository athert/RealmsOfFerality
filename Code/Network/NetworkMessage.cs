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
        server_characterLoginInfo,
        client_requestCreateCharacter,
        client_requestRemoveCharacter,
        client_requestJoinToWorld,
        server_requestMapLoading,
        server_synchronizeServerTime,

        obstacle_mapRequested,

        server_createEntity,
        server_setControllable,
        server_entity_setVisual,
        server_entity_setAnimation,
        server_removeEntity,
        client_movementSnapshot,
        server_chatMessage,
        server_chatChannelInfo,
        client_chatMessage,
        server_entityTeleport,
        server_inventoryItem,

        client_guildInfoRequest,
        server_guildInfo,
        server_setGuild,
        client_guildRankCreateRequest,
        client_guildRankRemoveRequest,
        client_guildMemberInviteRequest,
        client_guildMemberRemoveRequest,
        client_guildMemberRankChangeRequest,
        client_guildAnnouncementChange,
        client_guildInfoChange,
        client_guildCreateRequest,
        client_guildListRequest,
        client_guildApplicationInviteRequest,
        server_guildListInfo,
    }

    public enum LoginType
    {
        correct,
        invalid,
        gameVersion,
        banned,
    }

    public Type type;
    public NetDataWriter data = new NetDataWriter();

    public void Send(SendOptions sendOptions = SendOptions.ReliableOrdered)
    {
        Debug.Assert(Game.GetPlayer() != null, "Trying to send message without player initialized!");

        NetDataWriter netData = new NetDataWriter();
        netData.Put((byte)type);
        netData.Put(data.Data);
        Network.SendMessage(netData, sendOptions);
    }
}
