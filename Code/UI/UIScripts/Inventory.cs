using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    #region public
    public Transform inventoryHolder;
    public Transform categoryPanel;
    public Transform itemPanel;
    public Transform buttonPrefab;
    public Transform inventoryCamera;
    public Transform itemTextInfoPanel;
    public RawImage inventoryModelImage;
    public Transform categoryPanelHolder;
    public Transform itemPanelHolder;

    public static void RefreshInventory()
    {
        ShowInventory(false);
        ShowInventory(true);
    }
    public static void ShowInventory(bool show)
    {
        instance.isInventoryVisible = show;
        if (show)
            instance.ReconstructionCategoryPanel();
        else
        {
            instance.itemTextInfoPanel.gameObject.SetActive(false);
            instance.itemPanelHolder.gameObject.SetActive(false);
            if (instance.modelToShow != null)
                DestroyImmediate(instance.modelToShow.gameObject);
        }

        instance.inventoryHolder.gameObject.SetActive(show);
    }
    public static bool IsInventoryVisible()
    {
        return instance.isInventoryVisible;
    }
    public static bool IsReady()
    {
        return instance != null;
    }

    public void CategoryItemClickButton(Database.db_item.Type type, int index)
    {
        categoryPanelHolder.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1f - (index / ((float)categoryPanelButtonList.Count-1));
        ReconstructionItemPanel(type);
    }
    public void ItemClickButton(Database.db_item item, int index)
    {
        itemPanelHolder.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1f - (index / ((float)itemPanelButtonList.Count-1));
        ReconstructionItemInfo(item);
    }
    #endregion

    #region private
    private static Inventory instance;
    private bool isInventoryVisible = false;
    private Transform modelToShow;
    private Vector3 previousMousePosition;
    private List<Database.db_item> itemsToShow = new List<Database.db_item>();
    private List<Transform> categoryPanelButtonList = new List<Transform>();
    private List<Transform> itemPanelButtonList = new List<Transform>();

    private void Awake()
    {
        instance = this;
    }
    private void Start ()
    {
        EventTrigger trigger = inventoryModelImage.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Drag;
        entry.callback.AddListener((data) => { RotateModel(); });
        trigger.triggers.Add(entry);

        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.BeginDrag;
        entry.callback.AddListener((data) => { RotateModelBegin(); });
        trigger.triggers.Add(entry);
    }
    private void ReconstructionCategoryPanel()
    {
        itemsToShow.Clear();

        categoryPanelHolder.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1f;

        for (int i = 0; i < categoryPanelButtonList.Count; i++)
        {
            DestroyImmediate(categoryPanelButtonList[i].gameObject);
        }
        categoryPanelButtonList.Clear();

        List<Database.db_item.Type> types = new List<Database.db_item.Type>();
        List<NetworkItem> tempItems = Game.GetPlayer().GetItems();
        
        for (int i = 0; i < tempItems.Count; i++)
        {
            int index = Database.GetDBItemList().FindIndex(x => x.id == tempItems[i].id);
            if(index == -1)
            {
                Debug.LogError("Inventory request for itemId '" + tempItems[i] + "' but this itemId is not in local database!");
                    continue;
            }
            itemsToShow.Add(Database.GetDBItemList()[index]);
        }

        //itemsToShow = Database.GetDBItemList().FindAll(x => Array.IndexOf(GetItemIDs(), x.id) != -1);
        for (int i = 0; i < itemsToShow.Count; i++)
        {
            if (types.Contains(itemsToShow[i].type))
                continue;

            types.Add(itemsToShow[i].type);

            int index = categoryPanelButtonList.Count;

            Database.db_item.Type tempType = itemsToShow[i].type;
            Transform tempButton = Instantiate(buttonPrefab);
            tempButton.SetParent(categoryPanel);
            tempButton.GetComponent<Button>().onClick.AddListener(() => CategoryItemClickButton(tempType, index));
            tempButton.GetComponentInChildren<Text>().text = itemsToShow[i].type.ToString();
            tempButton.gameObject.SetActive(true);
            categoryPanelButtonList.Add(tempButton);
        }
    }
    private void ReconstructionItemPanel(Database.db_item.Type type)
    {
        itemPanelHolder.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 0;

        if (!itemPanelHolder.gameObject.activeSelf)
            itemPanelHolder.gameObject.SetActive(true);

        for (int i = 0; i < itemPanelButtonList.Count; i++)
        {
            DestroyImmediate(itemPanelButtonList[i].gameObject);
        }
        itemPanelButtonList.Clear();

        for (int i = 0; i < itemsToShow.Count; i++)
        {
            if (itemsToShow[i].type != type)
                continue;

            int index = itemPanelButtonList.Count;

            Database.db_item tempItem = itemsToShow[i];
            Transform tempButton = Instantiate(buttonPrefab);
            tempButton.SetParent(itemPanel);
            tempButton.GetComponent<Button>().onClick.AddListener(() => ItemClickButton(tempItem, index));
            tempButton.GetComponentInChildren<Text>().text = itemsToShow[i].name;
            tempButton.gameObject.SetActive(true);
            itemPanelButtonList.Add(tempButton);
        }
    }
    private void ReconstructionItemInfo(Database.db_item item)
    {
        if(modelToShow != null)
        {
            DestroyImmediate(modelToShow.gameObject);
        }

        modelToShow = Game.InstantiateResource(item.path);
        modelToShow.position = new Vector3(0, item.yPosInInventory, 0);
        modelToShow.eulerAngles = item.inventoryRotation;
        modelToShow.localScale = new Vector3(item.sizeInInventory, item.sizeInInventory, item.sizeInInventory);
        ChangeLayer(modelToShow, 31);

        itemTextInfoPanel.Find("Name").GetComponent<Text>().text = item.name;
        itemTextInfoPanel.Find("Info").GetComponent<Text>().text = item.description;
        itemTextInfoPanel.Find("Count").GetComponent<Text>().text = "Count: " + Game.GetPlayer().GetItems().Find(x => x.id == item.id).count;

        if (!itemTextInfoPanel.gameObject.activeSelf)
            itemTextInfoPanel.gameObject.SetActive(true);
    }
    private void RotateModel()
    {
        if (modelToShow == null)
            return;

        Vector3 pos = Input.mousePosition - previousMousePosition;
        previousMousePosition = Input.mousePosition;
        modelToShow.eulerAngles += new Vector3(pos.y * 0.1f, -pos.x * 0.1f, 0);
    }
    private void RotateModelBegin()
    {
        previousMousePosition = Input.mousePosition;
    }
    private void ChangeLayer(Transform parent, int layer)
    {
        parent.gameObject.layer = layer;
        for (int i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).gameObject.layer = layer;
            ChangeLayer(parent.GetChild(i), layer);
        }
    }
    
    #endregion
}
