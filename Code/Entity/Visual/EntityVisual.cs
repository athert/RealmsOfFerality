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

    /// <summary>
    /// Assign character model (feline, canine, dragon, pub...)
    /// </summary>
    /// <param name="model"></param>
    public void AssignCharacterModelToEntity(Database.db_character_model model)
    {
        characterModel = model;
        Transform modelHolder = baseEntity.transform.Find("_model");
        for (int i = 0; i < modelHolder.childCount; i++)
        {
            GameObject.DestroyImmediate(modelHolder.GetChild(i).gameObject);
        }

        Transform tempModel = Game.InstantiateResource(model.path);
        tempModel.SetParent(modelHolder);
        tempModel.localPosition = Vector3.zero;
        tempModel.localEulerAngles = Vector3.zero;

        mainModelBoneList.Clear();
        FindBonesInParent(tempModel);

        baseEntity.OnVisualChanged();
    }
    /// <summary>
    /// Assign character subpart (ears, tail, wings)
    /// </summary>
    /// <param name="model"></param>
    public void AssignCharacterSubModelToEntity(Database.db_character_submodel model)
    {
        RemoveCharacterSubModel(model.type);

        Transform modelHolder = baseEntity.transform.Find("_model");
        Transform tempModel = Game.InstantiateResource(model.path);
        tempModel.SetParent(modelHolder);
        tempModel.localPosition = Vector3.zero;
        tempModel.localEulerAngles = Vector3.zero;
        characterSubmodelList.Add(model.type, tempModel);

        baseEntity.OnVisualChanged();
    }
    public void RemoveCharacterSubModel(Database.db_character_submodel.Type type)
    {
        if (characterSubmodelList.ContainsKey(type))
        {
            GameObject.DestroyImmediate(characterSubmodelList[type]);
            characterSubmodelList.Remove(type);
        }
    }
    public void AssignItemToEntity(Database.db_item item)
    {
        if(attachedItemList.ContainsKey(item.id))
        {
            Debug.LogError("There is already attached itemId " + item.id + " !");
            return;
        }

        Transform tempModel = Game.InstantiateResource(item.path);

        if (item.boneNameAttach != "" && mainModelBoneList.ContainsKey(item.boneNameAttach))
        {
            tempModel.SetParent(mainModelBoneList[item.boneNameAttach]);
        }
        else
        {
            Transform modelHolder = baseEntity.transform.Find("_model");

            if (item.boneNameAttach != "")
                Debug.LogError("cannot find bone '" + item.boneNameAttach + "' requested by model '" + item.id + "' in body '" + modelHolder.GetChild(0).name + "'");

            tempModel.SetParent(modelHolder);
        }

        tempModel.localPosition = Vector3.zero;
        tempModel.localEulerAngles = Vector3.zero;
        attachedItemList.Add(item.id, tempModel);
    }
    public void RemoveItemFromEntity(int itemId)
    {
        if (attachedItemList.ContainsKey(itemId))
        {
            GameObject.DestroyImmediate(attachedItemList[itemId]);
            attachedItemList.Remove(itemId);
        }
    }
    public Database.db_character_model GetCharacterModel()
    {
        return characterModel;
    }
    #endregion

    #region private
    private Entity baseEntity;
    private Dictionary<Database.db_character_submodel.Type, Transform> characterSubmodelList = new Dictionary<Database.db_character_submodel.Type, Transform>();
    private Dictionary<string, Transform> mainModelBoneList = new Dictionary<string, Transform>();
    private Dictionary<int, Transform> attachedItemList = new Dictionary<int, Transform>();

    private Database.db_character_model characterModel;

    private void FindBonesInParent(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Debug.Assert(!mainModelBoneList.ContainsKey(parent.GetChild(i).name), "There is multiple bones with same name (" + parent.GetChild(i).name + ") in '" + parent.root + "'!");

            mainModelBoneList.Add(parent.GetChild(i).name, parent.GetChild(i));
            FindBonesInParent(parent.GetChild(i));
        }
    }
    #endregion
}
