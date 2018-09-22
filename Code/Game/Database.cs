using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Database : MonoBehaviour
{
    public Transform debugEntity;
    public Vector3 debugVector = new Vector3(50, 1, 50);

    private void Start()
    {
        
    }

    public Entity CreateEntity()
    {
        Transform ent = Instantiate(debugEntity);
        return ent.GetComponent<Entity>();
    }
}
