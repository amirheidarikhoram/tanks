using System;
using System.Collections.Generic;

[Serializable]
public class IntroduceServerAction
{
    public string s_type = "introduce_server";
    public World world;
    public List<Player> players;
}
