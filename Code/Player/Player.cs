using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region public
    public class GuildInfo
    {
        public class Rank
        {
            public int id;
            public string name;
            public bool isMain;
            public bool isBasic;
            public bool canMemberInvite;
            public bool canMemberRemove;
            public bool canRankCreateRemove;
            public bool canChangeInfo;
            public bool canChangeAnnouncement;
        }
        public class Member
        {
            public int id;
            public string name;
            public string zone;
            public Rank rank;
            public bool isOnline;
        }
        public enum TimeInfo
        {
            members,
            info,
            announcement,
            ranks,
            name,
            max,
        }

        //time info
        public int[] timeInfo = new int[(int)TimeInfo.max];
        //info
        public int id = -1;
        public string name = "";
        public string info = "";
        public string announcement = "";
        public string owner = "";
        public List<Rank> ranks = new List<Rank>();
        public List<Member> members = new List<Member>();

        public GuildInfo()
        {
            for (int i = 0; i < timeInfo.Length; i++)
            {
                timeInfo[i] = -1;
            }
        }
    }

    public void SetPlayerId(int id)
    {
        playerId = id;
    }
    public void SetFreezeInput(string reason, bool set)
    {
        if(set)
        {
            if (!inputFreezeList.Contains(reason))
                inputFreezeList.Add(reason);
        }
        else
        {
            if (inputFreezeList.Contains(reason))
                inputFreezeList.Remove(reason);
        }
    }
    public void SetItem(int id, int count)
    {
        int index = networkItemList.FindIndex(x => x.id == id);
        if(index == -1 && count > 0)
        {
            networkItemList.Add(new NetworkItem(id, count));
            return;
        }

        networkItemList[index].count = count;
        if (networkItemList[index].count == 0)
            networkItemList.RemoveAt(index);

        InventoryUI.RefreshInventory();
    }

    public Entity ControllableEntity
    {
        get { return controllableEntity; }
        set
        {
            controllableEntity = value;
            controllableEntity.GetMovementModule().SetRequestInputs(Vector3.zero);
        }
    }
    public int GetPlayerId()
    {
        return playerId;
    }
    public Vector3 GetInputs()
    {
        return new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }
    public List<NetworkItem> GetItems()
    {
        return networkItemList;
    }
    public GuildInfo GetGuildInfo()
    {
        return playerGuildInfo;
    }
    #endregion

    #region private
    private Entity controllableEntity;
    private int playerId = -1;
    private List<string> inputFreezeList = new List<string>();
    private List<NetworkItem> networkItemList = new List<NetworkItem>();
    private GuildInfo playerGuildInfo = new GuildInfo();

    private void Start ()
    {
		
	}
    private void Update ()
    {
        if (controllableEntity == null)
            return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            ControllableEntity.GetMovementModule().SetFlyingPossibility(!ControllableEntity.GetMovementModule().CanFly());
        }

        /*if(Input.GetKeyDown(KeyCode.O))
        {
            controllableEntity.GetMovementModule().AddForce(Vector3.one, 3);
        }*/

        Vector3 requestInput = Vector3.zero;
        if (inputFreezeList.Count == 0)
        {
            Vector3 inputs = GetInputs();
            float vertical = 0;

            if (ControllableEntity.GetMovementModule().IsInWalkableMovementType())
            {
                vertical = Input.GetKeyDown(KeyCode.Space) ? 1 : Input.GetKeyDown(KeyCode.LeftControl) ? -1 : 0;

                if(Input.GetKeyDown(KeyCode.LeftShift))
                {
                    ControllableEntity.GetMovementModule().SetCurrentMovementType(
                        ControllableEntity.GetMovementModule().CurrentMovementType == EntityMovement.MovementType.walk ? EntityMovement.MovementType.run : EntityMovement.MovementType.walk);
                }
            }
            else
                vertical = Input.GetKey(KeyCode.Space) ? 1 : Input.GetKey(KeyCode.LeftControl) ? -1 : 0;

            requestInput = new Vector3(0, vertical, 0);

            if (inputs.x != 0 || inputs.z != 0)
            {
                requestInput.z = 1;

                float angleToRotate = Vector3.Angle(controllableEntity.transform.forward, controllableEntity.transform.TransformVector(inputs));
                if (inputs.x < 0)
                    angleToRotate = -angleToRotate;
                angleToRotate += PlayerCamera.instance.transform.eulerAngles.y;
                controllableEntity.GetMovementModule().SetRequestRotation(angleToRotate);
            }
        }
        controllableEntity.GetMovementModule().SetRequestInputs(requestInput);
    }
    #endregion
}
