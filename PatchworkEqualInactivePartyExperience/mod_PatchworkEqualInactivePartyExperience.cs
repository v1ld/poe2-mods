using Game;
using Game.GameData;
using Patchwork;
using System.Collections.Generic;
using Onyx;

namespace V1ld_PatchworkEqualInactivePartyExperience
{
    [ModifiesType]
    public class V1ld_PartyManager : PartyManager
    {
        [ModifiesMember("AssignXPToInactiveParty")]
        new private void AssignXPToInactiveParty(int xp)
        {
            if (GameState.PlayerCharacter == null || GameState.PlayerCharacter.Stats == null || DataObject.IsNullOrEmpty(ExperienceTable.GameData))
            {
                return;
            }
            IList<PartyMemberData> partyMembersOfInactiveStatus = GetPartyMembersOfInactiveStatus();
            if (partyMembersOfInactiveStatus == null || partyMembersOfInactiveStatus.Count <= 0)
            {
                return;
            }
            foreach (PartyMemberData partyMemberData in partyMembersOfInactiveStatus)
            {
                if (partyMemberData != null && partyMemberData.MemberType == PartyMemberType.Primary)
                {
                    int newxp = ((partyMemberData.Level > GameState.PlayerCharacter.Stats.Level) ? (xp * ExperienceTable.GameData.StoredExperienceHigherLevelPercent / 100) : (xp * ExperienceTable.GameData.StoredExperiencePercent / 100));
                    partyMemberData.Experience += newxp;
                }
            }
        }
    }
}