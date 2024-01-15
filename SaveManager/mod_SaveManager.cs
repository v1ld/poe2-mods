using Game;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Patchwork;
using System;
using System.IO;
using UnityEngine;
using static Game.SaveLoadManager;

namespace V1ld_SaveManager
{
    [ModifiesType("Game.SaveLoadManager")]
    public static class V1ld_SaveLoadManager
    {
        [NewMember]
        [DuplicatesBody("SaveGame")]
        public static string SaveGame_Original(SaveGameType saveGameType, string userSaveName = "")
        {
            return null;
        }

        [ModifiesMember("SaveGame")]
        public static string SaveGame(SaveGameType saveGameType, string userSaveName = "")
        {
            if (saveGameType == SaveGameType.Quicksave)
                V1ld_ModData.QuicksaveCycleNumber += 1;
            return SaveGame_Original(saveGameType, userSaveName);
        }
    }

    [ModifiesType("Game.SaveLoadUtils")]
    public static class V1ld_SaveLoadUtils
    {
        [ModifiesMember("GetQuicksaveFileName")]
        public static string GetQuicksaveFileName()
        {
            return SaveLoadUtils.GetSpecialSaveFileName($"quicksave_{V1ld_ModData.QuicksaveCycleNumber}", includeMapName: false);
        }
    }

    [ModifiesType]
    public class V1ld_SaveGameMetadata : SaveGameMetadata
    {
        [ModifiesMember("IsQuickSave")]
        new public bool IsQuickSave()
        {
            // use a test that specifically excludes the original quicksave file
            return Path.GetFileNameWithoutExtension(FileName).Contains(" quicksave_");
        }
    }

    [NewType]
    internal class V1ld_ModData
    {
        private const int MaxQuickSaves = 10;

        private static bool m_initializedSavedData;

        private static JObject savedData;

        public static int QuicksaveCycleNumber
        {
            get
            {
                CheckSavedDataInitialized();
                var n = savedData["quicksaveCycleNumber"]?.Value<int?>() ?? 0;
                // Game.Console.AddMessage($"read value is {n}");
                return n % MaxQuickSaves;
            }

            set
            {
                CheckSavedDataInitialized();
                savedData["quicksaveCycleNumber"] = value % MaxQuickSaves;
                // Game.Console.AddMessage($"write value is {value}, saveData is {savedData.ToString()}");
                WriteSaveFor("SaveManager", savedData);
            }
        }

        private static void CheckSavedDataInitialized()
        {
            if (!m_initializedSavedData)
            {
                savedData = ReadSaveFor("SaveManager");
                // Game.Console.AddMessage($"Initialized, saveData is {savedData.ToString()}");
                m_initializedSavedData = true;
            }
        }

        private static JObject ReadSaveFor(string mod)
        {
            string saveFile = GetSaveFilePath(mod);
            if (!File.Exists(saveFile))
            {
                var saveData = new JObject();
                saveData["_mod"] = mod;
                return saveData;
            }

            try
            {
                using (StreamReader streamReader = new StreamReader(saveFile))
                using (JsonTextReader reader = new JsonTextReader(streamReader))
                {
                    var saveData = (JObject)JToken.ReadFrom(reader);
                    saveData["_mod"] = mod;
                    return saveData;
                }
            }
            catch (Exception ex)
            {
                Game.Console.AddMessage($"SavesForMods: {ex}");
                return null;
            }
        }

        private static void WriteSaveFor(string mod, JObject saveData)
        {
            string saveFile = GetSaveFilePath(mod);
            saveData["_mod"] = mod;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(saveFile));

                // Calling ToString() is worse for large JSON objects than saveData.WritetTo() but 
                // Patchwork has problems with the latter that I have no time to dig into, so
                File.WriteAllText(saveFile, saveData.ToString());

                /*
                using (StreamWriter streamWriter = new StreamWriter(saveFile))
                using (JsonTextWriter writer = new JsonTextWriter(streamWriter))
                {
                    // streamWriter.Write(saveData.ToString());  // Calling ToString() not great for large objects
                    writer.Formatting = Formatting.Indented;
                    saveData.WriteTo(writer);  // PatchworkLauncher gives a "key missing" error at final patch here
                } */
            }
            catch (Exception ex)
            {
                Game.Console.AddMessage($"SavesForMods: {ex}");
                return;
            }
            return;
        }

        private static string GetSaveFilePath(string m)
        {
            string basePath = Path.Combine(Application.dataPath, "../Mods/SavesForMods");
            string sessionPath = Path.Combine(basePath, GameState.PlayerCharacter.SessionID.ToString());
            string modFile = Path.Combine(sessionPath, m + ".json");
            return Path.GetFullPath(modFile);
        }
    }
}