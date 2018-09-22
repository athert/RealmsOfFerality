using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    #region public
    public void OnCreatedEntity(Entity entity)
    {
        Debug.Assert(entityList.Find(x => x.GetId() == entity.GetId()) == null, "There is already entity with ID '" + entity.GetId() + "' in mapEntityList!");
        entityList.Add(entity);
    }
    public Entity GetEntity(int id)
    {
        return entityList.Find(x => x.GetId() == id);
    }
    #endregion
    #region private
    private List<Entity> entityList = new List<Entity>();

    private void OnDestroy()
    {
        
    }
    #endregion
}
