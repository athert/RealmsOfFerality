﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    #region public
    public enum GameVersion
    {
        alpha_01a,
    }
    public int entityLayerIndex = 9;

    public static void AddThreadAction(Action action)
    {
        lock (instance.mainThreadActionList)
        {
            instance.mainThreadActionList.Add(action);
        }
    }
    public static void OnCreatedEntity(Entity entity)
    {
        if (GetMap() == null)
            return;

        GetMap().OnCreatedEntity(entity);
        Network.OnCreatedEntity(entity);
    }
    public static void LevelLoadRequest()
    {
        instance.PreLoadReconstruction();
        SceneManager.LoadSceneAsync("SampleScene", LoadSceneMode.Single);
    }
    public static Player GetPlayer()
    {
        if (instance == null)
            return null;

        return instance.localPlayer;
    }
    public static Map GetMap()
    {
        return instance.localMap;
    }
    public static bool IsMapReady()
    {
        return GetMap() != null;
    }
    public static GameVersion GetGameVersion()
    {
        return instance.gameVersion;
    }
    #endregion

    #region private
    private static Game instance;
    private Player localPlayer;
    private Map localMap;
    private List<Action> mainThreadActionList = new List<Action>();
    private GameVersion gameVersion = GameVersion.alpha_01a;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += PostLoadReconstruction;
    }
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(transform.gameObject);
    }
    private void Start ()
    {
        Physics.IgnoreLayerCollision(entityLayerIndex, entityLayerIndex, true);
        Network.Connect();
        //for debug only
        //PostLoadReconstruction(default(Scene), LoadSceneMode.Additive);
    }
	private void Update ()
    {
        /*if(Input.GetKeyDown(KeyCode.P))
        {
            if (debugBool)
                localPlayer.ControllableEntity = debugAttachableEntity;
            else
                localPlayer.ControllableEntity = debugAttachableEntity2;

            debugBool = !debugBool;
        }*/

        if (mainThreadActionList.Count > 0)
        {
            List<Action> localTempActions;
            lock (instance.mainThreadActionList)
            {
                localTempActions = new List<Action>(mainThreadActionList);
                mainThreadActionList.Clear();
            }
            for (int i = 0; i < localTempActions.Count; i++)
            {
                localTempActions[i].Invoke();
            }
        }
    }
    //called before loading of new map
    private void PreLoadReconstruction()
    {
        Debug.Log("pre");
        if(localMap != null)
        {
            GameObject.DestroyImmediate(localMap);
            localMap = null;
        }
    }
    //called after loading of new map
    private void PostLoadReconstruction(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("post");
        if (localPlayer == null)
            localPlayer = gameObject.AddComponent<Player>();

        localMap = GameObject.FindObjectOfType<Map>();
        if (localMap == null)
            localMap = gameObject.AddComponent<Map>();

        Network.OnPostLoadReconstruction();
    }
    #endregion
}
