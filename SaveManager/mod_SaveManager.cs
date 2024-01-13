using Game;
using Patchwork;
using System.IO;
using static Game.SaveLoadManager;

namespace V1ld_SaveManager
{
    [ModifiesType("Game.SaveLoadManager")]
    public static class V1ld_SaveLoadManager
    {
        [NewMember]
        private static int m_quicksaveCycleNumber;

        [NewMember]
        public static int QuicksaveCycleNumber
        {
            [NewMember]
            get { return m_quicksaveCycleNumber; }
            [NewMember]
            set { m_quicksaveCycleNumber = value % 10; }
        }

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
                V1ld_SaveLoadManager.QuicksaveCycleNumber += 1;
            return SaveGame_Original(saveGameType, userSaveName);
        }
    }

    [ModifiesType("Game.SaveLoadUtils")]
    public static class V1ld_SaveLoadUtils
    {
        [ModifiesMember("GetQuicksaveFileName")]
        public static string GetQuicksaveFileName()
        {
            return SaveLoadUtils.GetSpecialSaveFileName($"quicksave_{V1ld_SaveLoadManager.QuicksaveCycleNumber}", includeMapName: false);
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
}