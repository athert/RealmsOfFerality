using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{
    private class ChatBubble
    {
        public Transform who;
        public Transform bubble;
        public float timer;
        public ChatBubble(Transform who, Transform bubble, float timer)
        {
            this.who = who;
            this.bubble = bubble;
            this.timer = timer;
        }
    }

    public enum MessageType
    {
        say,
        emote,
        yell,
        whisper,
        gm,
        channel,
        party,
        guild,
        system,
    }

    public Color[] chatTextColor = new Color[]
    {
         Color.white,
         new Color(253f / 255f, 118f / 255f, 0f / 255f),
         new Color(255f / 255f, 56f / 255f, 56f / 255f),
         new Color(255f / 255f, 88f/255f, 250f / 255f),
         new Color(229f / 255f, 0f/255f, 0f / 255f),
         new Color(234f / 255f, 186f / 255f, 186f / 255f),
         new Color(11f / 255f, 240f / 255f, 231f / 255f),
         new Color(10f / 255f, 245f / 255f, 10f / 255f),
         Color.yellow,
    };

    private struct ChatText
    {
        public string text;
        public Transform visual;
    }

    private struct ChatChannel
    {
        public string name;
    }

    public Transform chatTextPrefab;
    public Transform chatTextHolder;
    public InputField chatInputField;
    public Transform chatMessageType;
    public Transform chatBubblePrefab;
    public Transform chatBubbleHolder;
    public Transform chatScrollbar;
    public Image chatResizeImage;
    public RectTransform chatPanel;

    public int textSizeInChat = 14;
    public float chatBubbleHeight = 2.9f;

    public static void ReceiveChatMessage(Entity who, MessageType type, string text)
    {
        if (instance == null)
            return;

        if (who != null && (instance.currentMessageType == MessageType.say || instance.currentMessageType == MessageType.yell))
        {
            float width, height;
            int index = text.IndexOf(':') + 1;

            Transform tempChatBubble = Instantiate(instance.chatBubblePrefab);
            tempChatBubble.SetParent(instance.chatBubbleHolder, false);
            Text bubbleText = tempChatBubble.GetChild(0).GetComponent<Text>();

            bubbleText.text = text.Substring(index, text.Length - index);
            float readTime = Mathf.Clamp(bubbleText.text.Length * instance.charCharacterReadTime, instance.chatBubbleMinimalReadTime, instance.chatBubbleMaximalReadTime);

            width = bubbleText.preferredWidth;
            height = bubbleText.preferredHeight;

            width += 20;   //magical number that perfectly fit
            if (width > 200)
                width = 200;

            height += 20;

            bubbleText.rectTransform.sizeDelta = new Vector2(bubbleText.rectTransform.sizeDelta.x, height);
            tempChatBubble.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);

            Vector3 ownerPos = PlayerCamera.instance.GetComponent<Camera>().WorldToScreenPoint(who.transform.position + new Vector3(0, instance.chatBubbleHeight, 0));
            tempChatBubble.position = ownerPos.z > 0 ? new Vector2(ownerPos.x, ownerPos.y) : new Vector2(-1000, -1000);
            tempChatBubble.gameObject.SetActive(true);


            ChatBubble tempBubble = new ChatBubble(who.transform, tempChatBubble, readTime);

            //remove old bubbles for same target
            for (int i = 0; i < instance.chatBubblesList.Count; i++)
            {
                if (instance.chatBubblesList[i].who == who.transform)
                    instance.chatBubblesList[i].timer = 0;
            }

            instance.chatBubblesList.Add(tempBubble);

            //apply lipSync
            //who.GetComponent<EntityAnimations>().LipSync(EntityAnimations.GenerateLipSync(bubbleText.text));
        }

        ChatText tempChatText = new ChatText();
        tempChatText.text = text;

        Transform tempChatTextPrefab = Instantiate(instance.chatTextPrefab);
        tempChatTextPrefab.SetParent(instance.chatTextHolder, false);
        tempChatTextPrefab.GetComponent<Text>().text = text;
        tempChatTextPrefab.GetComponent<Text>().fontSize = instance.textSizeInChat;
        tempChatTextPrefab.GetComponent<Text>().color = instance.chatTextColor[(int)type];
        tempChatTextPrefab.gameObject.SetActive(true);

        tempChatText.visual = tempChatTextPrefab;
        instance.chatTextList.Add(tempChatText);
        instance.EnsureChatTextCount();
    }
    public static void AddChatChannel(string name)
    {
        ChatChannel channel = new ChatChannel();
        channel.name = name;
        instance.chatChannelList.Add(channel);
        ReceiveChatMessage(null, MessageType.system, "Joined chat channel '" + name + "'!");
    }
    public static void RemoveChatChannel(string name)
    {
        int index = instance.chatChannelList.FindIndex(x => x.name == name);
        if (index == -1)
            return;

        instance.chatChannelList.RemoveAt(index);
        ReceiveChatMessage(null, MessageType.system, "Leaved chat channel '" + name + "'!");
    }
    public static void HideChat(bool hide)
    {
        instance.chatPanel.gameObject.SetActive(!hide);
    }
    public static bool IsChatFocused()
    {
        return instance.chatInputField.isFocused;
    }
    public static bool IsReady()
    {
        return instance != null;
    }

    public void SendChatMessage()
    {
        string message = instance.chatInputField.text;
        instance.chatInputField.text = "";

        if (message.Length == 0 || message.Replace(" ", "") == "")
            return;

        NetworkMessageResolve.NetworkChatMessageRequest(currentMessageType, message, chatMessageType.GetComponentInChildren<Text>().text);

        //enterInputIgnore = true;
        EventSystem.current.SetSelectedGameObject(null);
    }
    public void OnChatMessageChange(string text)
    {
        if (text.Length == 0)
            return;

        MessageType previousMode = currentMessageType;

        if (text[0] == '/')
        {
            string[] messageCmd = text.Split(' ');
            if (messageCmd[0] == "/s")
            {
                chatInputField.text = "";
                currentMessageType = MessageType.say;
                ChangeMessageTypeString("Say");
            }
            else if (messageCmd[0] == "/y")
            {
                chatInputField.text = "";
                currentMessageType = MessageType.yell;
                ChangeMessageTypeString("Yell");
            }
            else if (messageCmd[0] == "/w")
            {
                if (messageCmd.Length > 2)
                {
                    chatInputField.text = "";
                    currentMessageType = MessageType.whisper;
                    ChangeMessageTypeString(messageCmd[1]);
                }
            }
            else if (messageCmd[0] == "/e")
            {
                chatInputField.text = "";
                currentMessageType = MessageType.emote;
                ChangeMessageTypeString("Emote");
            }
            else if (messageCmd[0] == "/gm")
            {
                chatInputField.text = "";
                currentMessageType = MessageType.gm;
                ChangeMessageTypeString("GM");
            }
            else if (messageCmd[0] == "/ch")
            {
                if (messageCmd.Length > 2)
                {
                    int index = chatChannelList.FindIndex(x => x.name.ToLower() == messageCmd[1].ToLower());
                    if (index == -1)
                    {
                        ReceiveChatMessage(null, MessageType.system, "You are not in channel '" + messageCmd[1] + "'!");
                        chatInputField.text = "";
                        return;
                    }

                    chatInputField.text = "";
                    currentMessageType = MessageType.channel;
                    ChangeMessageTypeString(chatChannelList[index].name);
                }
            }
        }

        if (currentMessageType != previousMode)
        {
            instance.chatInputField.transform.Find("Text").GetComponent<Text>().color = instance.chatTextColor[(int)currentMessageType];
        }
    }

    private static Chat instance;
    private readonly float chatBubbleMinimalReadTime = 3;
    private readonly float chatBubbleMaximalReadTime = 10;
    private readonly float charCharacterReadTime = 0.25f;
    private int maximumChatTextsOnScreen = 30;
    private MessageType currentMessageType = MessageType.say;
    private List<ChatText> chatTextList = new List<ChatText>();
    private List<ChatBubble> chatBubblesList = new List<ChatBubble>();
    private List<ChatChannel> chatChannelList = new List<ChatChannel>();
    private string whisperName;
    private bool enterInputIgnore;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
    }
    private void Start()
    {
        EventTrigger trigger = chatResizeImage.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Drag;
        entry.callback.AddListener((data) => { ResizeChatDrag(); });
        trigger.triggers.Add(entry);

        chatInputField.onEndEdit.AddListener(delegate { SendChatMessageOnEnter(); });
        trigger = chatInputField.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerSelect = new EventTrigger.Entry();
        pointerSelect.eventID = EventTriggerType.Select;
        pointerSelect.callback.AddListener((e) => ChatInputSelect(true));
        trigger.triggers.Add(pointerSelect);

        EventTrigger.Entry pointerDeselect = new EventTrigger.Entry();
        pointerDeselect.eventID = EventTriggerType.Deselect;
        pointerDeselect.callback.AddListener((e) => ChatInputSelect(false));
        trigger.triggers.Add(pointerDeselect);

        EventTrigger scrollbarTrigger = chatScrollbar.gameObject.AddComponent<EventTrigger>();
        scrollbarTrigger.triggers.Add(pointerSelect);
        scrollbarTrigger.triggers.Add(pointerDeselect);

        currentMessageType = MessageType.say;
        ChangeMessageTypeString("Say");
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !enterInputIgnore)
        {
            if (!chatInputField.isFocused)
                chatInputField.Select();
        }

        for (int i = chatBubblesList.Count - 1; i >= 0; i--)
        {
            if (chatBubblesList[i].timer <= 0)
            {
                DestroyImmediate(chatBubblesList[i].bubble.gameObject);
                chatBubblesList.RemoveAt(i);
            }
            else
            {
                chatBubblesList[i].timer -= Time.deltaTime;
            }
        }
    }
    private void LateUpdate()
    {
        #region chatBubbles
        //enterInputIgnore is here becouse i just dont know how to remove this event after use.
        if (enterInputIgnore)
        {
            enterInputIgnore = false;
            chatInputField.interactable = true;
        }
        for (int i = 0; i < chatBubblesList.Count; i++)
        {
            Vector3 ownerPos = PlayerCamera.instance.GetComponent<Camera>().WorldToScreenPoint(chatBubblesList[i].who.position + new Vector3(0, chatBubbleHeight, 0));
            chatBubblesList[i].bubble.position = ownerPos.z > 0 ? new Vector2(ownerPos.x, ownerPos.y) : new Vector2(-1000, -1000);
        }
        #endregion
    }
    private void ChangeMessageTypeString(string typeName)
    {
        chatMessageType.GetComponentInChildren<Text>().text = typeName;
    }
    private void ResizeChatDrag()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 diff = mousePos - chatPanel.position;
        chatPanel.sizeDelta = new Vector3(Mathf.Clamp(diff.x, 200, 1500), Mathf.Clamp(diff.y, 100, 1000), 1);

        float textWidth = chatPanel.sizeDelta.x - 40;
        chatTextPrefab.GetComponent<LayoutElement>().minWidth = textWidth;
        chatTextPrefab.GetComponent<LayoutElement>().preferredWidth = textWidth;

        for (int i = 0; i < chatTextList.Count; i++)
        {
            chatTextList[i].visual.GetComponent<LayoutElement>().minWidth = textWidth;
            chatTextList[i].visual.GetComponent<LayoutElement>().preferredWidth = textWidth;
        }
    }
    private void EnsureChatTextCount()
    {
        if (chatTextList.Count > maximumChatTextsOnScreen)
        {
            for (int i = chatTextList.Count - 1; i >= maximumChatTextsOnScreen; i--)
            {
                DestroyImmediate(chatTextList[i].visual.gameObject);
                chatTextList.RemoveAt(i);
            }
        }
    }
    private void ChatInputSelect(bool value)
    {
        if (value)
        {
            Game.GetPlayer().SetFreezeInput("chatInputSelect", true);
        }
        else
        {
            Game.GetPlayer().SetFreezeInput("chatInputSelect", false);
        }
    }
    private void SendChatMessageOnEnter()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendChatMessage();
        }
    }
}
