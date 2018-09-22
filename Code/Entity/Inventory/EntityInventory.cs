using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityInventory
{
    #region public
    public EntityInventory(Entity baseEntity)
    {
        this.baseEntity = baseEntity;
    }
    #endregion

    #region private
    private Entity baseEntity;
    #endregion
}
