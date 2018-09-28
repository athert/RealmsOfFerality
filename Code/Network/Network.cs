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
        if (instance.serverNetPeer == null)
            return;

        instance.serverNetPeer.Send(data, option);
    }
    //call in fader after map loading
    public static void OnPostLoadReconstruction()
    {
        for (int i = 0; i < instance.messageWaitingList.Count; i++)
        {
            NetDataReader tempReader = instance.messageWaitingList[i];
            NetworkMessage.Type messageType = (NetworkMessage.Type)tempReader.GetByte();
            Game.AddThreadAction(() => NetworkMessageResolve.Resolve(messageType, tempReader));
        }
        instance.messageWaitingList.Clear();
    }
    public static void OnCreatedEntity(Entity entity)
    {
        if (!instance.entityMessageWaitingList.ContainsKey(entity.GetId()))
            return;

        for (int i = 0; i < instance.entityMessageWaitingList[entity.GetId()].Count; i++)
        {
            NetDataReader data = instance.entityMessageWaitingList[entity.GetId()][i];
            NetworkMessage.Type messageType = (NetworkMessage.Type)data.GetByte();
            Game.AddThreadAction(() => NetworkMessageResolve.Resolve(messageType, data));
        }
        instance.entityMessageWaitingList.Remove(entity.GetId());
    }
    public static void AddEntityWaitingMessage(int id, NetDataReader data)
    {
        if (!instance.entityMessageWaitingList.ContainsKey(id))
            instance.entityMessageWaitingList.Add(id, new List<NetDataReader>());

        NetDataReader tempReader = new NetDataReader();
        tempReader.SetSource(data.Data);
        instance.entityMessageWaitingList[id].Add(tempReader);
    }

    public static void Connect()
    {
        Debug.Log("connecting...");

        EventBasedNetListener listener = new EventBasedNetListener();
        instance.serverConnection = new NetManager(listener, "rofConnection");
        instance.serverConnection.UpdateTime = 10;
        instance.serverConnection.Start();
        instance.serverConnection.Connect("127.0.0.1", 5555);

        listener.NetworkReceiveEvent += instance.OnNetworkReceiveEvent;
        listener.PeerConnectedEvent += instance.OnPeerConnectedEvent;
        listener.PeerDisconnectedEvent += instance.OnPeerDisconnectedEvent;
        listener.NetworkLatencyUpdateEvent += instance.OnNetworkLatencyUpdateEvent;

        instance.networkThread = new Thread(instance.NetworkThread);
        instance.networkThread.Start();
    }
    public static void Disconnect()
    {
        instance.serverConnection.Stop();

        instance.networkThread.Interrupt();
        instance.networkThread = null;

        Debug.Log("disconnecting...");
        Game.AddThreadAction(() => LoginScreenUI.ShowInfo("disconnected"));
    }
    public static void SetServerTime(int serverTime)
    {
        instance.serverTime = serverTime;
    }
    public static int GetServerTime()
    {
        return instance.serverTime;
    }
    #endregion

    #region private
    private static Network instance;
    private NetManager serverConnection;
    private NetPeer serverNetPeer;
    private Thread networkThread;
    private int serverTime;
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
    private void OnNetworkLatencyUpdateEvent(NetPeer peer, int latency)
    {
        //Game.serverLatency = latency;
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
    private void NetworkThread()
    {
        while (true)
        {
            serverConnection.PollEvents();
            Thread.Sleep(20);
        }
    }
    private void OnConnect(NetPeer peer)
    {
        serverNetPeer = peer;
    }
    private void OnDisconnect()
    {
        serverNetPeer = null;
        Disconnect();
    }
    private void OnApplicationQuit()
    {
        if (serverConnection != null)
            serverConnection.Stop();
        if (networkThread != null)
            networkThread.Abort();

        serverConnection = null;
        networkThread = null;
    }
    private bool CanProcessMessage(NetworkMessage.Type type)
    {
        if (!Game.IsMapReady())
        {
            return (int)type <= (int)NetworkMessage.Type.obstacle_mapRequested;
        }
        return true;
    }
    #endregion
}
