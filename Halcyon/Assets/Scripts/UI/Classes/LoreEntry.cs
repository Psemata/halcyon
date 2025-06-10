using System;

[Serializable]
public class LoreEntry
{
    public int id;
    public string story;
    public bool type; // true for mountain lore, false for climbers lore

    public LoreEntry(int id, string story, bool type = true)
    {
        this.id = id;
        this.story = story;
        this.type = type;
    }
}

[Serializable]
public class LoreEntryArray
{
    public LoreEntry[] array;
}
