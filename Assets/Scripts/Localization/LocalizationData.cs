using System;
using System.Collections.Generic;

// Helper class that represents a single localization entry (One line of dialog).
[System.Serializable]
public class LocalizationEntry
{
    public string key; // Identifier for the dialogue line.
    public string value; // Actual dialogue output.
}

// Class that holds a list of localization entries. Serializable so its easy to save and load from JSON. 
[System.Serializable]
public class LocalizationData
{
    public List<LocalizationEntry> dialogues = new List<LocalizationEntry>();
}
