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
    public void RemoveEntity(int id)
    {
        int index = entityList.FindIndex(x => x.GetId() == id);
        if (index == -1)
            return;

        DestroyImmediate(entityList[index].gameObject);
        entityList.RemoveAt(index);
    }
    public Suimono.Core.SuimonoModule GetSuimonoModule()
    {
        return moduleObject;
    }
    #endregion
    #region private
    private Suimono.Core.SuimonoModule moduleObject;
    private List<Entity> entityList = new List<Entity>();

    private void Start()
    {
        moduleObject = GameObject.Find("SUIMONO_Module").gameObject.GetComponent<Suimono.Core.SuimonoModule>();
    }

    private void OnDestroy()
    {
        
    }

    private void Update()
    {
        /*if (Game.GetPlayer().ControllableEntity == null)
            return;

        float[] heightValues = moduleObject.SuimonoGetHeightAll(Game.GetPlayer().ControllableEntity.transform.position);
        Debug.Log("calc");
        // 4 = 1 = true, 5 = - = UP
        for (int i = 0; i < heightValues.Length; i++)
        {
            Debug.Log(i+": "+heightValues[i]);
        }*/
    }
    #endregion
}
