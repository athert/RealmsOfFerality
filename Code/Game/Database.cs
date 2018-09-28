using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Database : MonoBehaviour
{
    #region public
    public struct db_character_age
    {
        public int id;
        public string name;
    }
    public struct db_character_gender
    {
        public int id;
        public string name;
    }
    public struct db_character_model
    {
        public int id;
        public string path;
        
    }
    public struct db_character_submodel
    {
        public enum Type
        {
            mane,
            ears,
            tail,
            tufts,
        }
        public int id;
        public string name;
        public string path;
        public Type type;
        public db_character_model[] models;
    }
    public struct db_character_race
    {
        public int id;
        public string name;
        public db_character_age[] ages;
        public db_character_gender[] genders;
    }
    public struct db_character_modelCondition
    {
        public db_character_model model;
        public db_character_race race;
        public db_character_age age;
        public db_character_gender gender;
    }
    public struct db_item
    {
        public enum Type
        {
            hat,
            weapon,
            shoulders,
            drinks,
            cookies,
        }

        public int id;
        public string name;
        public string path;
        public string boneNameAttach;
        public string description;
        public float sizeInInventory;
        public float yPosInInventory;
        public Vector3 inventoryRotation;
        public Type type;
    }

    public static Entity CreateEntity()
    {
        return Game.InstantiateResource(pathToEntity).GetComponent<Entity>();
    }
    public static List<db_character_age> GetDBAgeList()
    {
        return instance.dbCharacterAgeList;
    }
    public static List<db_character_gender> GetDBGenderList()
    {
        return instance.dbCharacterGenderList;
    }
    public static List<db_character_model> GetDBCharacterModelList()
    {
        return instance.dbCharacterModelList;
    }
    public static List<db_character_modelCondition> GetDBCharacterModelConditionList()
    {
        return instance.dbCharacterModelConditionList;
    }
    public static List<db_character_race> GetDBRaceList()
    {
        return instance.dbCharacterRaceList;
    }
    public static List<db_character_submodel> GetDBCharacterSubModelList()
    {
        return instance.dbCharacterSubModelList;
    }
    public static List<db_item> GetDBItemList()
    {
        return instance.dbItemList;
    }
    public static int[] GetGuildTimes(int guildId)
    {
        return Game.GetPlayer().GetGuildInfo().timeInfo;
    }

    //specific
    public static List<db_character_submodel> GetDBCharacterSubModelBySpecific(db_character_model model, db_character_submodel.Type type)
    {
        return instance.dbCharacterSubModelList.FindAll(x => x.type == type && Array.IndexOf(x.models, model) != -1);
    }
    #endregion

    #region private
    private static readonly string pathToEntity = "Prefabs/Entities/Entity";
    private static Database instance;
    private List<db_character_age> dbCharacterAgeList                           = new List<db_character_age>();
    private List<db_character_gender> dbCharacterGenderList                     = new List<db_character_gender>();
    private List<db_character_model> dbCharacterModelList                       = new List<db_character_model>();
    private List<db_character_modelCondition> dbCharacterModelConditionList     = new List<db_character_modelCondition>();
    private List<db_character_race> dbCharacterRaceList                         = new List<db_character_race>();
    private List<db_character_submodel> dbCharacterSubModelList                 = new List<db_character_submodel>();
    private List<db_item> dbItemList                                            = new List<db_item>();

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        DBLoad_Character_Ages();
        DBLoad_Character_Gender();
        DBLoad_Character_Model();
        DBLoad_Character_Race();
        DBLoad_Character_ModelCondition();
        DBLoad_Character_SubModel();
        DBLoad_Character_Item();
    }
    private void DBLoad_Character_Ages()
    {
        DatabaseLoading("character_ages", (reader) =>
        {
            switch (reader.Name)
            {
                case "character_age":
                    {
                        int id = int.Parse(reader.GetAttribute("id"));
                        string name = reader.GetAttribute("name");

                        db_character_age age = new db_character_age();
                        age.id = id;
                        age.name = name;
                        dbCharacterAgeList.Add(age);
                    }
                    break;
            }
        });
    }
    private void DBLoad_Character_Gender()
    {
        DatabaseLoading("character_genders", (reader) =>
        {
            switch (reader.Name)
            {
                case "character_gender":
                    {
                        int id = int.Parse(reader.GetAttribute("id"));
                        string name = reader.GetAttribute("name");

                        db_character_gender gender = new db_character_gender();
                        gender.id = id;
                        gender.name = name;
                        dbCharacterGenderList.Add(gender);
                    }
                    break;
            }
        });
    }
    private void DBLoad_Character_Model()
    {
        DatabaseLoading("character_models", (reader) =>
        {
            switch (reader.Name)
            {
                case "character_model":
                    {
                        int id = int.Parse(reader.GetAttribute("id"));
                        string path = reader.GetAttribute("path");

                        db_character_model model = new db_character_model();
                        model.id = id;
                        model.path = path;
                        dbCharacterModelList.Add(model);
                    }
                    break;
            }
        });
    }
    private void DBLoad_Character_ModelCondition()
    {
        DatabaseLoading("character_modelsConditions", (reader) =>
        {
            switch (reader.Name)
            {
                case "character_modelCondition":
                    {
                        int model = int.Parse(reader.GetAttribute("model"));
                        int race = int.Parse(reader.GetAttribute("race"));
                        int age = int.Parse(reader.GetAttribute("age"));
                        int gender = int.Parse(reader.GetAttribute("gender"));

                        int ageIndex = dbCharacterAgeList.FindIndex(x => x.id == age);
                        int genderIndex = dbCharacterGenderList.FindIndex(x => x.id == gender);
                        int modelIndex = dbCharacterModelList.FindIndex(x => x.id == model);
                        int raceIndex = dbCharacterRaceList.FindIndex(x => x.id == race);
                        Debug.Assert(ageIndex != -1, "modelCondition for model " + model + " is trying to access invalid ageId " + age + "!");
                        Debug.Assert(genderIndex != -1, "modelCondition for model " + model + " is trying to access invalid genderId " + gender + "!");
                        Debug.Assert(modelIndex != -1, "modelCondition for model " + model + " is trying to access invalid modelId " + model + "!");
                        Debug.Assert(raceIndex != -1, "modelCondition for model " + model + " is trying to access invalid raceId " + race + "!");

                        db_character_modelCondition condition = new db_character_modelCondition();
                        condition.model = dbCharacterModelList[modelIndex];
                        condition.race = dbCharacterRaceList[raceIndex];
                        condition.gender = dbCharacterGenderList[genderIndex];
                        condition.age = dbCharacterAgeList[ageIndex];
                        dbCharacterModelConditionList.Add(condition);
                    }
                    break;
            }
        });
    }
    private void DBLoad_Character_Race()
    {
        DatabaseLoading("character_races", (reader) =>
        {
            switch (reader.Name)
            {
                case "character_race":
                    {
                        int id = int.Parse(reader.GetAttribute("id"));
                        string name = reader.GetAttribute("name");
                        string ages = reader.GetAttribute("ages");
                        string genders = reader.GetAttribute("genders");

                        int[] ageArray;
                        if (!MathX.ParseIntArrayFromString(ages, out ageArray))
                            Debug.LogError("ages cannot be parsed from character_races id: " + id + "!");

                        int[] genderArray;
                        if (!MathX.ParseIntArrayFromString(genders, out genderArray))
                            Debug.LogError("genders cannot be parsed from character_races id: " + id + "!");

                        List<db_character_age> tempAgeList = new List<db_character_age>();
                        for (int i = 0; i < ageArray.Length; i++)
                        {
                            int index = dbCharacterAgeList.FindIndex(x => x.id == ageArray[i]);
                            Debug.Assert(index != -1, "character_races id:" + id + " is trying to access invalid ageId: " + ageArray[i] + "!");
                            tempAgeList.Add(dbCharacterAgeList[index]);
                        }

                        List<db_character_gender> tempGenderList = new List<db_character_gender>();
                        for (int i = 0; i < genderArray.Length; i++)
                        {
                            int index = dbCharacterGenderList.FindIndex(x => x.id == genderArray[i]);
                            Debug.Assert(index != -1, "character_races id:" + id + " is trying to access invalid genderId: " + ageArray[i] + "!");
                            tempGenderList.Add(dbCharacterGenderList[index]);
                        }

                        db_character_race race = new db_character_race();
                        race.id = id;
                        race.name = name;
                        race.ages = tempAgeList.ToArray();
                        race.genders = tempGenderList.ToArray();
                        dbCharacterRaceList.Add(race);
                    }
                    break;
            }
        });
    }
    private void DBLoad_Character_SubModel()
    {
        DatabaseLoading("character_subModels", (reader) =>
        {
            switch (reader.Name)
            {
                case "character_subModel":
                    {
                        int id = int.Parse(reader.GetAttribute("id"));
                        string name = reader.GetAttribute("name");
                        string path = reader.GetAttribute("path");
                        int type = int.Parse(reader.GetAttribute("type"));
                        string models = reader.GetAttribute("models");

                        int[] modelIds;
                        MathX.ParseIntArrayFromString(models, out modelIds);

                        List<db_character_model> tempModels = new List<db_character_model>();
                        for(int i=0;i<modelIds.Length;i++)
                        {
                            int index = dbCharacterModelList.FindIndex(x => x.id == modelIds[i]);
                            if(index == -1)
                            {
                                Debug.Log("subModel '" + id + "' is trying to access unknown model '" + modelIds[i] + "'");
                                continue;
                            }
                            tempModels.Add(dbCharacterModelList[index]);
                        }

                        db_character_submodel subModel = new db_character_submodel();
                        subModel.id = id;
                        subModel.name = name;
                        subModel.path = path;
                        subModel.type = (db_character_submodel.Type)type;
                        subModel.models = tempModels.ToArray();
                        dbCharacterSubModelList.Add(subModel);
                    }
                    break;
            }
        });
    }
    private void DBLoad_Character_Item()
    {
        DatabaseLoading("items", (reader) =>
        {
            switch (reader.Name)
            {
                case "item":
                    {
                        int id = int.Parse(reader.GetAttribute("id"));
                        string name = reader.GetAttribute("name");
                        string path = reader.GetAttribute("path");
                        string description = reader.GetAttribute("description");
                        int type = int.Parse(reader.GetAttribute("type"));
                        float sizeInInventory = float.Parse(reader.GetAttribute("sizeInInventory"));
                        float yPosInInventory = float.Parse(reader.GetAttribute("yPosInInventory"));

                        float[] values;
                        if (!MathX.ParseFloatArrayFromString(reader.GetAttribute("inventoryPosition"), out values))
                        {
                            values = new float[3];
                        }
                        Vector3 inventoryPosition = new Vector3(values[0], values[1], values[2]);

                        if (!MathX.ParseFloatArrayFromString(reader.GetAttribute("inventoryPosition"), out values))
                        {
                            values = new float[3];
                        }
                        Vector3 inventoryRotation = new Vector3(values[0], values[1], values[2]);

                        db_item item = new db_item();
                        item.id = id;
                        item.name = name;
                        item.path = path;
                        item.sizeInInventory = sizeInInventory;
                        item.type = (db_item.Type)type;
                        item.inventoryRotation = inventoryRotation;
                        item.description = description;
                        item.yPosInInventory = yPosInInventory;
                        dbItemList.Add(item);
                    }
                    break;
            }
        });
    }
    private void DatabaseLoading(string dbName, Action<XmlReader> call)
    {
        XmlReaderSettings settings = new XmlReaderSettings();
        using (XmlReader reader = XmlReader.Create(new System.IO.StringReader(LoadDatabaseContent(dbName)), settings))
        {
            reader.MoveToContent();
            while (reader.Read())
            {
                try
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        call(reader);
                    }
                }
                catch (Exception e)
                { }
            }
        }
    }
    private string LoadDatabaseContent(string dbName)
    {
        TextAsset db = (TextAsset)Resources.Load("Database/" + dbName);
        return db.text;
    }
    #endregion
}
