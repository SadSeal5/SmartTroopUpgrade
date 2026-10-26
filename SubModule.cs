using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SmartTroopUpgrade
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            new Harmony("com.smarttroopupgrade.bannerlord").PatchAll();
        }

        protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
        {
            base.InitializeGameStarter(game, starterObject);
            if (starterObject is CampaignGameStarter cgs)
            {
                cgs.AddBehavior(new SmartTroopUpgradeBehavior());
            }
        }
    }
}