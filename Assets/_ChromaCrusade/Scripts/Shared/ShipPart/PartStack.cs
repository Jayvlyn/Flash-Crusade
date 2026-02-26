using System;

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