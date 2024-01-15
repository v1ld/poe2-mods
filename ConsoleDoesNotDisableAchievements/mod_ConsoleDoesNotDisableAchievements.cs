using Game;
using Game.GameData;
using Patchwork;
using Console = Game.Console;

namespace V1ld_ConsoleDoesNotDisableAchievements
{
    [ModifiesType("Game.CommandLine")]
    public static class V1ld_CommandLine
    {
        [ModifiesMember("IRoll20s")]
        public static void IRoll20s()
        {
            //if (!GameState.Instance.CheatsEnabled && GodChallengeGameData.AnyEnabledChallenge(GodChallengeGameData.DisablesCheatsPredicate))
            //{
            //    Console.AddMessage("Cannot enable cheats with God Challenges enabled.");
            //    return;
            //}
            GameState.Instance.CheatsEnabled = !GameState.Instance.CheatsEnabled;
            //if (GameState.Instance.CheatsEnabled && SingletonBehavior<AchievementTracker>.Instance != null)
            //{
            //    SingletonBehavior<AchievementTracker>.Instance.DisableAchievements = true;
            //}
            if (GameState.Instance.CheatsEnabled)
            {
                Console.AddMessage("Console Enabled - Achievements and Challenge Completion still enabled.");
            }
            else
            {
                Console.AddMessage("Console Disabled");
            }
            //if (AnalyticsManager.Instance != null)
            //{
            //    AnalyticsManager.Instance.CheatsEnabled(GameState.Instance.CheatsEnabled);
            //}
        }
    }

    // this is overkill and could be reduced to just the overrides of the 2 getters
    [ModifiesType]
    public class V1ld_AchievementTracker : AchievementTracker
    {
        [ModifiesMember("OnyxAwake")]
        public override void OnyxAwake()
        {
            base.OnyxAwake();
            //if (GameState.Instance.CheatsEnabled)
            //{
            //    DisableAchievements = true;
            //}
        }

        [ModifiesMember("get_m_disableAchievements")]
        private bool get_m_disableAchievements()
        {
            return false;
        }

        [ModifiesMember("set_m_disableAchievements")]
        private void set_m_disableAchievements(bool value)
        {
            m_persistentAchievementTracker.m_disableAchievements = false;
        }

        [ModifiesMember("get_DisableAchievements")]
        public bool get_DisableAchievements()
        {
            return false;
        }

        [ModifiesMember("set_DisableAchievements")]
        public void set_DisableAchievements(bool value)
        {
            m_disableAchievements = false;
        }

        [ModifiesMember("DisableAchievement")]
        new public void DisableAchievement(ChallengeGameData achievement)
        {
            m_disabledAchievements.Clear();
        }
    }

    [ModifiesType]
    public class V1ld_CharacterExportMetadata : CharacterExportMetadata
    {
        // This is how the Steam UGC stuff tracks downloaded characters from "cheat games"
        [NewMember]
        new public bool IsFromCheatGame
        {
            [NewMember]
            get { return false; }
            [NewMember]
            set { return; }
        }
    }
}