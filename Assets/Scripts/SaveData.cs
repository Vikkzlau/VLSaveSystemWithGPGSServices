using System;

// Example class to be used as save object. Must be marked as Serializable.
[Serializable]
public class SaveData
{
    public int HP = 0;

    public SaveData()
    {
        HP = 0;
    }
    public SaveData(int hp)
    {
        HP = hp;
    }
}
