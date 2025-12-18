using System;

[Serializable]
public class PlayerProgressData
{
    public PlayerProfile profile = new PlayerProfile();
    public int version = 1;

    public PlayerProgressData() {}
}
