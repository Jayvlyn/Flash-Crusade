using System.Collections.Generic;

public class PartInventoryModel
{
    [System.Serializable]
    public class Entry
    {
        public ShipPartData data;
        public int count;
    }

    private readonly Dictionary<PartType, List<Entry>> inventory = new Dictionary<PartType, List<Entry>>();

    public PartInventoryModel(PartInventory inv)
    {
        inventory[PartType.Cabin] = Resolve(inv.cabins);
        inventory[PartType.Core] = Resolve(inv.cores);
        inventory[PartType.Wing] = Resolve(inv.wings);
        inventory[PartType.Weapon] = Resolve(inv.weapons);
        inventory[PartType.Utility] = Resolve(inv.utilities);
    }

    private List<Entry> Resolve(List<PartStack> stackList)
    {
        var resolved = new List<Entry>();

        foreach (var stack in stackList)
        {
            var data = PartDatabase.Instance.Get(stack.name);
            if (data == null) continue;

            resolved.Add(new Entry { data = data, count = stack.count });
        }

        return resolved;
    }

    public bool TryTake(ShipPartData data)
    {
        var list = inventory[data.PartType];
        var entry = list.Find(e => e.data == data);

        if (entry == null || entry.count <= 0)
            return false;

        entry.count--;

        if (entry.count == 0)
            list.Remove(entry);

        return true;
    }

    public void Add(ShipPartData data)
    {
        var list = inventory[data.PartType];
        var entry = list.Find(e => e.data == data);

        if (entry != null)
            entry.count++;
        else
            list.Add(new Entry { data = data, count = 1 });
    }

    public IReadOnlyList<Entry> GetParts(PartType type)
    {
        return inventory[type];
    }


}
