using System;
using System.Collections.Generic;

[Serializable]
public class ScoreEntry
{
    public string playerName;
    public int score;
    public string date;

    public ScoreEntry(string name, int score)
    {
        this.playerName = name;
        this.score = score;
        this.date = DateTime.Now.ToString("MM/dd/yyyy");
    }
}

[Serializable]
public class LeaderboardSaveData
{
    public List<ScoreEntry> entries = new List<ScoreEntry>();
    public List<string> knownPlayers = new List<string>();
    public string lastPlayerName = "";
}