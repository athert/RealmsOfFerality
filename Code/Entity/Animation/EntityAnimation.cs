using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAnimation
{
    #region public
    public class ShapeKey
    {
        public ShapeKey(string keyName,int index, SkinnedMeshRenderer renderer)
        {
            this.keyName = keyName;
            this.index = index;
            this.renderer = renderer;
        }

        public void SetValue(float value)
        {
            renderer.SetBlendShapeWeight(index, value);
        }
        public float GetValue()
        {
            return renderer.GetBlendShapeWeight(index);
        }
        public string GetKeyName()
        {
            return keyName;
        }
        public string Identify()
        {
            return renderer.transform.name;
        }

        private string keyName;
        private int index;
        private SkinnedMeshRenderer renderer;
    }

    public EntityAnimation(Entity baseEntity)
    {
        this.baseEntity = baseEntity;
    }

    public void Update()
    {
    }

    //called everytime when model or submodel will change (visual will somehow changes)
    public void OnVisualChanged()
    {
        shapeKeyList.Clear();

        SkinnedMeshRenderer[] sMeshArray = baseEntity.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < sMeshArray.Length; i++)
        {
            for (int b = 0; b < sMeshArray[i].sharedMesh.blendShapeCount; b++)
            {
                string keyName = sMeshArray[i].sharedMesh.GetBlendShapeName(b);
                ShapeKey foundKey = shapeKeyList.Find(x => x.GetKeyName() == keyName);

                Debug.Assert(foundKey == null, "There is already shapeKey with name '" + keyName + "' (Collision between '" + 
                    (foundKey == null ? "invalid!" : foundKey.Identify()) + "' and '" + sMeshArray[i].transform.name + "')!");

                ShapeKey tempKey = new ShapeKey(keyName, b, sMeshArray[i]);
                shapeKeyList.Add(tempKey);
            }
        }
        animator = baseEntity.GetComponentInChildren<Animator>();
    }
    #endregion

    #region private
    private Entity baseEntity;
    private List<ShapeKey> shapeKeyList = new List<ShapeKey>();
    private Animator animator;
    #endregion
}
