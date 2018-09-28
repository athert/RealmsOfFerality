using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public void Update()
    {
        if (!Chat.IsReady() || !Inventory.IsReady())
            return;

        if (Input.GetKeyDown(KeyCode.I) && CanShowInventory())
        {
            ShowInventory(!Inventory.IsInventoryVisible());
        }
    }

    public bool CanShowInventory()
    {
        return !Chat.IsChatFocused();
    }

    private void ShowInventory(bool show)
    {
        Inventory.ShowInventory(show);
        Chat.HideChat(show);
        UnityEngine.PostProcessing.PostProcessingProfile profile = PlayerCamera.instance.GetComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>().profile;
        profile.depthOfField.enabled = show;
    }
}
