using Game;
using Patchwork;
using System;

namespace V1ld_DontAutosaveEveryLevel
{
    [ModifiesType]
    public class V1ld_GameState_DAEL : GameState
    {
        [NewMember]
        public static DateTime V1ld_LastAutoSaveTime;

        [ModifiesMember("Autosave")]
        new public static void Autosave()
        {
            if (Option.TrialOfIron)
            {
                TrialOfIronSave(viaApplicationQuit: false);
                return;
            }

            var now = DateTime.Now;
            var minutes = 10f;
            if ((minutes >= 0) && ((now - V1ld_LastAutoSaveTime).TotalMinutes >= minutes))
            {
                SaveLoadManager.SaveGame(SaveLoadManager.SaveGameType.Autosave, string.Empty);
                Instance.AutosaveCycleNumber++;
                V1ld_LastAutoSaveTime = now;
            }
        }
    }
}