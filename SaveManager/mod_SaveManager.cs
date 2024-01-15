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

        private static bool DEBUG = false;

        private static JObject _savedData;

        private static JObject SavedData
        {
            get 
            {
                if (_savedData == null)
                    _savedData = ReadSaveFor("SaveManager");
                return _savedData;
            }
        }

        public static int QuicksaveCycleNumber
        {
            get
            {
                var n = SavedData["quicksaveCycleNumber"]?.Value<int?>() ?? (MaxQuickSaves-1);
                Log($"read: value={n}");
                return n % MaxQuickSaves;
            }

            set
            {
                SavedData["quicksaveCycleNumber"] = value % MaxQuickSaves;
                Log($"write: value={value} savedData={SavedData}");
                WriteSaveFor("SaveManager", SavedData);
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
                File.WriteAllText(saveFile, saveData.ToString());

                /* 
                // Calling ToString() is worse for large JSON objects than saveData.WritetTo() but 
                // Patchwork has problems with the latter that I have no time to dig into, so
                using (StreamWriter streamWriter = new StreamWriter(saveFile))
                using (JsonTextWriter writer = new JsonTextWriter(streamWriter))
                {
                    writer.Formatting = Formatting.Indented;
                    saveData.WriteTo(writer);  // This makes PatchworkLauncher emit a "key missing" error at patch time
                }
                */
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

        private static void Log(string message)
        {
            if (DEBUG)
                Game.Console.AddMessage(message);
        }
    }
}