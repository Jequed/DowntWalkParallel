using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class MobileIOUtility
{
    private const int VERSION = 3;

    public static void SaveLevel(string path, LevelContainer levelContainer)
    {
        var writer = new BinaryWriter(File.OpenWrite(path));

        writer.Write(VERSION);

        WriteVector3(levelContainer.Phone.transform.localScale, writer);
        WriteVector3(levelContainer.Phone.transform.position, writer);

        var blocks = levelContainer.BlocksContainer.GetComponentsInChildren<Block>();
        var people = levelContainer.PeopleContainer.GetComponentsInChildren<Person>();
        var miscItems = levelContainer.MiscContainer.GetComponentsInChildren<DWPObject>();

        var allItems = new List<DWPObject>();
        allItems.AddRange(blocks);
        allItems.AddRange(people);
        allItems.AddRange(miscItems);

        writer.Write(allItems.Count);
        foreach (var item in allItems)
        {
            writer.Write((int)item.ItemType);

            WriteVector3(item.transform.position, writer);

            switch (item.ItemType)
            {
                case ItemInfo.Type.Block:
                    SaveBlock(item as Block, writer);
                    break;
                case ItemInfo.Type.NPC:
                case ItemInfo.Type.Player:
                    SavePerson(item as Person, writer);
                    break;
                case ItemInfo.Type.Cloner:
                    SaveCloner(item as PersonCloner, writer);
                    break;
                default:
                    break;
            }
        }

        writer.Close();
    }
    private static void SaveBlock(Block block, BinaryWriter writer)
    {
        writer.Write(block.CanSeeOver);
    }
    private static void SavePerson(Person person, BinaryWriter writer)
    {
        writer.Write((int)person.InitialDirection);

        if (person.ItemType == ItemInfo.Type.NPC)
        {
            var npc = person as NPC;
            writer.Write(npc.Actions.Count);
            foreach (var action in npc.Actions)
            {
                writer.Write(action.expanded);
                writer.Write((int)action.direction);
                writer.Write(action.wait);
                writer.Write(action.waitTime);
                writer.Write(action.distance);
                writer.Write(action.speedMultiplier);
                writer.Write(action.overrideAnimation);
                writer.Write(action.playOnce);
            }
        }

        WriteColor(person.MainSpriteRenderer.color, writer);

        writer.Write(person.AppearanceManager.Accessories.Count);
        foreach (var accessoryInstance in person.AppearanceManager.Accessories)
        {
            writer.Write(accessoryInstance.expanded);
            writer.Write(accessoryInstance.Accessory.name);
            WriteColor(accessoryInstance.color, writer);
        }

        var properties = person.Properties;
        writer.Write(person.Properties.Length);
        foreach (var property in properties)
        {
            var type = property.PropertyType;
            writer.Write((int)type);
            switch (type)
            {
                case PersonProperty.Type.ViewCone:
                    var viewCone = property as ViewCone;
                    writer.Write(viewCone.Radius);
                    writer.Write(viewCone.Arc);
                    writer.Write(viewCone.TurnRate);
                    writer.Write(viewCone.DrainRate);
                    break;
                default:
                    break;
            }
        }
    }
    private static void SaveCloner(PersonCloner cloner, BinaryWriter writer)
    {
        writer.Write(cloner.SpawnRate);
        writer.Write(cloner.Pattern);
        writer.Write(cloner.Delay);
        writer.Write(cloner.FillPath);
    }
    private static void WriteVector3(Vector3 vec, BinaryWriter writer)
    {
        writer.Write(vec.x);
        writer.Write(vec.y);
        writer.Write(vec.z);
    }
    private static void WriteColor(Color color, BinaryWriter writer)
    {
        writer.Write(color.r);
        writer.Write(color.g);
        writer.Write(color.b);
        writer.Write(color.a);
    }

    public static void OpenLevel(string path, LevelContainer levelContainer)
    {
        var items = levelContainer.GetComponentsInChildren<DWPObject>();
        foreach (var item in items)
            GameObject.Destroy(item.gameObject);

        var reader = new BinaryReader(File.OpenRead(path));

        var fileVersion = reader.ReadInt32();

        if (fileVersion >= 2)
        {
            levelContainer.Phone.transform.localScale = ReadVector3(reader);
            levelContainer.Phone.transform.position = ReadVector3(reader);
        }

        var itemCount = reader.ReadInt32();

        for (int i = 0; i < itemCount; i++)
        {
            var itemType = (ItemInfo.Type)reader.ReadInt32();

            var itemPosition = ReadVector3(reader);

            DWPObject item = GameObject.Instantiate(Resources.Load<DWPObject>("Prefabs/GamePrefabs/" + itemType.ToString()));

            switch (itemType)
            {
                case ItemInfo.Type.Player:
                    if (fileVersion >= 2)
                        OpenPerson(item as Person, reader, fileVersion);
                    break;
                case ItemInfo.Type.Block:
                    OpenBlock(item as Block, reader, fileVersion);
                    break;
                case ItemInfo.Type.NPC:
                    OpenPerson(item as Person, reader, fileVersion);
                    break;
                case ItemInfo.Type.Cloner:
                    OpenCloner(item as PersonCloner, reader, fileVersion);
                    break;
                default:
                    break;
            }
            
            item.transform.position = itemPosition;

            switch (GlobalData.GetInfo(item.ItemType).Layer)
            {
                case LevelContainer.LayerType.blocks:
                    item.transform.parent = levelContainer.BlocksContainer.transform;
                    break;
                case LevelContainer.LayerType.people:
                    item.transform.parent = levelContainer.PeopleContainer.transform;
                    break;
                case LevelContainer.LayerType.misc:
                    item.transform.parent = levelContainer.MiscContainer.transform;
                    break;
            }
        }

        reader.Close();
    }
    private static void OpenBlock(Block block, BinaryReader reader, int fileVersion)
    {
        block.CanSeeOver = reader.ReadBoolean();
    }
    private static void OpenPerson(Person person, BinaryReader reader, int fileVersion)
    {
        person.InitialDirection = (ObjectAction.MovementDirection)reader.ReadInt32();

        if (person.ItemType == ItemInfo.Type.NPC)
        {
            List<ObjectAction> actions = new List<ObjectAction>();

            int actionCount = reader.ReadInt32();
            for (int i = 0; i < actionCount; i++)
            {
                ObjectAction action = new ObjectAction();
                action.expanded = reader.ReadBoolean();
                action.direction = (ObjectAction.MovementDirection)reader.ReadInt32();
                action.wait = reader.ReadBoolean();
                action.waitTime = reader.ReadSingle();
                action.distance = reader.ReadSingle();
                action.speedMultiplier = reader.ReadSingle();
                action.overrideAnimation = reader.ReadBoolean();
                action.playOnce = reader.ReadBoolean();
                actions.Add(action);
            }
            (person as NPC).Actions = actions;
        }

        if (fileVersion >= 1)
        {
            person.MainSpriteRenderer.color = ReadColor(reader);

            int accessoryCount = reader.ReadInt32();
            for (int i = 0; i < accessoryCount; i++)
            {
                bool expanded = reader.ReadBoolean();
                string accessoryName = reader.ReadString();
                Color accessoryColor = ReadColor(reader);

                PersonAccessory foundAccessory = null;
                foreach (var accessory in PersonData.accessories)
                {
                    if (accessory.name == accessoryName)
                    {
                        foundAccessory = accessory;
                        break;
                    }
                }

                if (foundAccessory != null)
                {
                    var instance = person.AppearanceManager.AddAccessory(foundAccessory);
                    instance.expanded = expanded;
                    instance.color = accessoryColor;
                }
            }
        }

        if (fileVersion >= 2)
        {
            int propertyCount = reader.ReadInt32();

            for (int i = 0; i < propertyCount; i++)
            {
                var propertyType = (PersonProperty.Type)reader.ReadInt32();

                var property = person.AddProperty(propertyType);

                switch (propertyType)
                {
                    case PersonProperty.Type.ViewCone:
                        var viewCone = property as ViewCone;
                        viewCone.Radius = reader.ReadSingle();
                        viewCone.Arc = reader.ReadSingle();
                        viewCone.TurnRate = reader.ReadSingle();
                        if (fileVersion >= 3)
                            viewCone.DrainRate = reader.ReadSingle();
                        break;
                    default:
                        break;
                }
            }
        }
    }
    private static void OpenCloner(PersonCloner cloner, BinaryReader reader, int fileVersion)
    {
        cloner.SpawnRate = reader.ReadSingle();
        cloner.Pattern = reader.ReadString();
        cloner.Delay = reader.ReadSingle();
        if (fileVersion >= 2)
            cloner.FillPath = reader.ReadBoolean();
    }
    private static Vector3 ReadVector3(BinaryReader reader)
    {
        Vector3 vec;
        vec.x = reader.ReadSingle();
        vec.y = reader.ReadSingle();
        vec.z = reader.ReadSingle();
        return vec;
    }
    private static Color ReadColor(BinaryReader reader)
    {
        Color col;
        col.r = reader.ReadSingle();
        col.g = reader.ReadSingle();
        col.b = reader.ReadSingle();
        col.a = reader.ReadSingle();
        return col;
    }
}