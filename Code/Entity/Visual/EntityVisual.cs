using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityVisual
{
    #region public
    public EntityVisual(Entity baseEntity)
    {
        this.baseEntity = baseEntity;
    }
    #endregion

    #region private
    private Entity baseEntity;
    #endregion
}
