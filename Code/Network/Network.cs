using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Network : MonoBehaviour
{
    #region public
    public static void SendMessage(NetDataWriter data, SendOptions option)
    {
        
    }
    //call in fader after map loading
    public static void OnPostLoadReconstruction()
    {
        for (int i = 0; i < instance.messageWaitingList.Count; i++)
        {
            NetworkMessage.Type messageType = (NetworkMessage.Type)instance.messageWaitingList[i].GetByte();
            Game.AddThreadAction(() => NetworkMessageResolve.Resolve(messageType, instance.messageWaitingList[i]));
        }
    }
    public static void OnCreatedEntity(Entity entity)
    {
        if (!instance.entityMessageWaitingList.ContainsKey(entity.GetId()))
            return;

        for (int i = 0; i < instance.entityMessageWaitingList[entity.GetId()].Count; i++)
        {
            NetworkMessage.Type messageType = (NetworkMessage.Type)instance.entityMessageWaitingList[entity.GetId()][i].GetByte();
            Game.AddThreadAction(() => NetworkMessageResolve.Resolve(messageType, instance.entityMessageWaitingList[entity.GetId()][i]));
        }
    }
    public static void AddEntityWaitingMessage(int id, NetDataReader data)
    {
        if (!instance.entityMessageWaitingList.ContainsKey(id))
            instance.entityMessageWaitingList.Add(id, new List<NetDataReader>());

        instance.entityMessageWaitingList[id].Add(data);
    }
    #endregion

    #region private
    private static Network instance;
    private NetManager serverConnection;
    private NetPeer serverNetPeer;
    private Thread networkThread;
    private List<NetDataReader> messageWaitingList = new List<NetDataReader>();
    private Dictionary<int, List<NetDataReader>> entityMessageWaitingList = new Dictionary<int, List<NetDataReader>>();

    private void Awake()
    {
        instance = this;
    }
    private void OnNetworkReceiveEvent(NetPeer peer, NetDataReader reader)
    {
        Debug.Assert(reader != null && reader.Data != null, "There is null as reader or reader data!");
        NetDataReader data = new NetDataReader(reader.Data);
        if (data == null || data.Data == null || data.Data.Length == 0)
            return;

        NetworkMessage.Type messageType = (NetworkMessage.Type)data.GetByte();
        if(CanProcessMessage(messageType))
        {
            Game.AddThreadAction(() => NetworkMessageResolve.Resolve(messageType, data));
        }
        else
        {
            messageWaitingList.Add(new NetDataReader(data.Data));
        }
    }
    private bool CanProcessMessage(NetworkMessage.Type type)
    {
        if (!Game.IsMapReady())
        {
            switch (type)
            {
                case NetworkMessage.Type.server_createEntity:
                case NetworkMessage.Type.server_removeEntity:
                    return false;
            }
        }
        return true;
    }
    #endregion

    
    public void Connect()
    {
        EventBasedNetListener listener = new EventBasedNetListener();
        serverConnection = new NetManager(listener, "feralHeartConnection");
        serverConnection.UpdateTime = 10;
        listener.NetworkReceiveEvent += OnNetworkReceiveEvent;
        listener.PeerConnectedEvent += OnPeerConnectedEvent;
        listener.PeerDisconnectedEvent += OnPeerDisconnectedEvent;
        listener.NetworkLatencyUpdateEvent += OnNetworkLatencyUpdateEvent;

        networkThread = new Thread(NetworkThread);
        Debug.Log("connecting...");
        serverConnection.Start();
        serverConnection.Connect("127.0.0.1", 5555);
        //serverConnection.Connect(Database.GetConfig().netAddress, Database.GetConfig().netPort);
        networkThread.Start();
    }

    public void OnApplicationQuit()
    {
        serverConnection.Stop();
        networkThread.Abort();

        serverConnection = null;
        networkThread = null;
    }

    private void OnNetworkLatencyUpdateEvent(NetPeer peer, int latency)
    {
        //Game.serverLatency = latency;
    }

    public void Disconnect()
    {
        Debug.Log("disconnecting...");
        serverConnection.Stop();
        networkThread.Interrupt();
        networkThread = null;

        OnDisconnect();
    }

    private void NetworkThread()
    {
        while (true)
        {
            serverConnection.PollEvents();
            Thread.Sleep(20);
        }
    }
    private void OnPeerConnectedEvent(NetPeer peer)
    {
        Debug.Log("Connected to: " + peer.EndPoint);
        OnConnect(peer);
    }
    private void OnPeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("Disconnect from: " + peer.EndPoint);
        OnDisconnect();
    }

    void OnConnect(NetPeer peer)
    {
        serverNetPeer = peer;
    }
    void OnDisconnect()
    {
        serverNetPeer = null;
        Disconnect();
    }
}
