using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace SmartTroopUpgrade
{
    public class SmartTroopUpgradeBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, OnHourlyTickParty);
            CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener(this, OnMissionEnded);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
            // Empty - Mod state is driven by MCM settings and game state
        }

        private void OnHourlyTickParty(MobileParty party)
        {
            if (party == MobileParty.MainParty)
            {
                TroopUpgradeManager.RunUpgrade();
            }
        }

        private void OnMissionEnded(IMission mission)
        {
            TroopUpgradeManager.RunUpgrade();
        }

        private void OnGameLoaded(CampaignGameStarter starter)
        {
            TroopUpgradeManager.RunUpgrade();
        }

        private void OnTick(float dt)
        {
            if (Input.IsKeyDown(InputKey.LeftControl) &&
                Input.IsKeyDown(InputKey.LeftShift) &&
                Input.IsKeyPressed(InputKey.U))
            {
                int upgraded = TroopUpgradeManager.RunUpgrade();
                InformationManager.DisplayMessage(new InformationMessage($"[Smart Troop Upgrade] Manual upgrade processed: {upgraded} troop(s) upgraded."));
            }
        }
    }
}