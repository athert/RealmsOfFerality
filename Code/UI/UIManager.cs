using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public void Update()
    {
        if (!ChatUI.IsReady() || !InventoryUI.IsReady())
            return;

        if (Input.GetKeyDown(KeyCode.I) && CanShowInventory())
        {
            ShowInventory(!InventoryUI.IsInventoryVisible());
        }

        if (Input.GetKeyDown(KeyCode.G) && CanShowGuild())
        {
            ShowGuild(!GuildUI.IsGuildVisible());
        }
    }

    public bool CanShowInventory()
    {
        return !ChatUI.IsChatFocused();
    }

    public bool CanShowGuild()
    {
        return !ChatUI.IsChatFocused() && !InventoryUI.IsInventoryVisible();
    }

    private void ShowInventory(bool show)
    {
        InventoryUI.ShowInventory(show);
        ChatUI.HideChat(show);
        UnityEngine.PostProcessing.PostProcessingProfile profile = PlayerCamera.instance.GetComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>().profile;
        profile.depthOfField.enabled = show;
    }

    private void ShowGuild(bool show)
    {
        GuildUI.ShowGuild(show);
    }
}
