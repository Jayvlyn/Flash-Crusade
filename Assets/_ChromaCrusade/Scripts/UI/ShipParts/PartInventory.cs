using System;
using System.Collections.Generic;

[Serializable]
public class PartInventory
{
    public List<PartStack> cabins = new();
    public List<PartStack> cores = new();
    public List<PartStack> wings = new();
    public List<PartStack> weapons = new();
    public List<PartStack> utilities = new();

    public void Add(string partName, PartType type, int amount = 1)
    {
        var list = GetList(type);
        var existing = list.Find(p => p.name == partName);

        if (existing != null)
        {
            existing.count += amount;
        }
        else
        {
            list.Add(new PartStack(partName, amount));
        }
    }

    public void Remove(string partName, PartType type, int amount = 1)
    {
        var list = GetList(type);
        var existing = list.Find(p => p.name == partName);

        if (existing == null) return;

        existing.count -= amount;
        if (existing.count <= 0)
            list.Remove(existing);
    }

    private List<PartStack> GetList(PartType type)
    {
        return type switch
        {
            PartType.Cabin => cabins,
            PartType.Core => cores,
            PartType.Wing => wings,
            PartType.Weapon => weapons,
            PartType.Utility => utilities,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
}

[Serializable]
public class PartStack
{
    public string name;
    public int count;

    public PartStack(string name, int count)
    {
        this.name = name;
        this.count = count;
    }
}
