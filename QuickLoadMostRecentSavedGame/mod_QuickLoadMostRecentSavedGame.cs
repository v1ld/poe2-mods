using Game;
using Patchwork;
using Onyx;
using System;
using Game.UI;
using UnityEngine;

namespace V1ld_QuickLoadMostRecentSavedGame
{
    [ModifiesType]
    public class V1ld_InGameHUD_QLMRS : InGameHUD
    {
        [ModifiesMember("HandleInput")]
        new private void HandleInput()
        {
            if (!GameInput.GetControlUp(MappedControl.QUICKLOAD) || UISingletonHudWindow<UICharacterCreationManager>.Instance.IsVisible)
            {
                return;
            }
            if (SaveLoadManager.DoesSaveExist(SaveLoadUtils.GetSaveGameNameFromType(SaveLoadManager.SaveGameType.Quicksave)))
            {
                if (!SingletonBehavior<FadeManager>.Instance.IsFadeActive())
                {
                    FadeManager instance = SingletonBehavior<FadeManager>.Instance;
                    instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Combine(instance.OnFadeEnded, new FadeManager.OnFadeEnd(GameResources.LoadLastSaveOnFadeEnd));
                    SingletonBehavior<FadeManager>.Instance.FadeToBlackForTransition(AudioFadeMode.MusicAndFx);
                }
            }
            else
            {
                UISystemMessager.Instance.PostMessage(GuiStringTable.GetText(1500), Color.red);
            }
        }
    }
}