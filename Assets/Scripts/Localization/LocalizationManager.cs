using UnityEngine;
using System.Collections.Generic;


public class LocalizationManager : MonoBehaviour
{
    // Stores all loaded text.
    private static Dictionary<string, string> localizedText = new Dictionary<string, string>();

    // Load a JSON file from Resources/Dialogue/
    public static void LoadLanguage(string fileName)
    {
        localizedText.Clear(); // Clear old text

        // Load the JSON file.
        TextAsset jsonFile = Resources.Load<TextAsset>($"Dialogue/{fileName}");

        // Check if the file was found.
        if (jsonFile == null)
        {
            Debug.LogError($"Localization file not found: Dialogue/{fileName}.json");
            return;
        }

        // Convert the JSON text into our data class.
        LocalizationData data = JsonUtility.FromJson<LocalizationData>(jsonFile.text);

        if (data == null || data.dialogues == null)
        {
            Debug.LogError("Failed to parse localization JSON.");
            return;
        }

        // Fill the dictionary so we can look up text quickly.
        foreach (var entry in data.dialogues)
        {
            localizedText[entry.key] = entry.value;
        }

        Debug.Log($"Successfully loaded {localizedText.Count} localized texts from {fileName}.json");
    }

    // get text from the text id (key) from the dialogue item.
    public static string GetText(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return "Empty Key";
        }

        if (localizedText.TryGetValue(key, out string text))
            return text;

        // Report if the key is missing.
        return $"<Missing: {key}>";
    }

    // Clear everything in the dictionary.
    public static void Clear()
    {
        localizedText.Clear();
    }
}