using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAnimation
{
    #region public
    public EntityAnimation(Entity baseEntity)
    {
        this.baseEntity = baseEntity;
    }
    #endregion

    #region private
    private Entity baseEntity;
    #endregion
}
