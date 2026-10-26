using System;
using MCM.Abstractions.Base.Global;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Common;

namespace SmartTroopUpgrade
{
    public class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => "SmartTroopUpgrade";
        public override string DisplayName => "Smart Troop Upgrade";
        public override string FolderName => "SmartTroopUpgrade";
        public override string FormatType => "json2";

        private static Settings _fallbackInstance;
        public static new Settings Instance
        {
            get
            {
                try
                {
                    return AttributeGlobalSettings<Settings>.Instance ?? (_fallbackInstance ??= new Settings());
                }
                catch
                {
                    return _fallbackInstance ??= new Settings();
                }
            }
        }

        [SettingPropertyBool("Mod Enabled", Order = 0, RequireRestart = false, HintText = "Enable or disable automatic troop upgrades.")]
        [SettingPropertyGroup("General")]
        public bool ModEnabled { get; set; } = true;

        [SettingPropertyFloatingInteger("Shield Infantry Ratio (%)", 0f, 100f, "#0'%'", Order = 1, RequireRestart = false, HintText = "Target quota percentage for shield infantry.")]
        [SettingPropertyGroup("Army Composition Ratios")]
        public float ShieldInfantryPercent { get; set; } = 40f;

        [SettingPropertyFloatingInteger("Shock Infantry Ratio (%)", 0f, 100f, "#0'%'", Order = 2, RequireRestart = false, HintText = "Target quota percentage for shock infantry.")]
        [SettingPropertyGroup("Army Composition Ratios")]
        public float ShockInfantryPercent { get; set; } = 10f;

        [SettingPropertyFloatingInteger("Ranged Ratio (%)", 0f, 100f, "#0'%'", Order = 3, RequireRestart = false, HintText = "Target quota percentage for foot archers/crossbowmen.")]
        [SettingPropertyGroup("Army Composition Ratios")]
        public float RangedPercent { get; set; } = 25f;

        [SettingPropertyFloatingInteger("Cavalry Ratio (%)", 0f, 100f, "#0'%'", Order = 4, RequireRestart = false, HintText = "Target quota percentage for melee cavalry.")]
        [SettingPropertyGroup("Army Composition Ratios")]
        public float CavalryPercent { get; set; } = 15f;

        [SettingPropertyFloatingInteger("Horse Archer Ratio (%)", 0f, 100f, "#0'%'", Order = 5, RequireRestart = false, HintText = "Target quota percentage for mounted archers.")]
        [SettingPropertyGroup("Army Composition Ratios")]
        public float HorseArcherPercent { get; set; } = 10f;

        [SettingPropertyInteger("Minimum Gold Reserve", 0, 100000, "0", Order = 1, RequireRestart = false, HintText = "Player gold reserve buffer. Upgrades will stop if gold would fall below this amount.")]
        [SettingPropertyGroup("Resource Safeguards")]
        public int MinGoldReserve { get; set; } = 2000;

        [SettingPropertyInteger("Minimum Horse Reserve", 0, 50, "0", Order = 2, RequireRestart = false, HintText = "Preserve at least this many mount items in inventory before upgrading to cavalry.")]
        [SettingPropertyGroup("Resource Safeguards")]
        public int MinHorseReserve { get; set; } = 5;

        [SettingPropertyBool("Respect Locked Troops", Order = 3, RequireRestart = false, HintText = "Do not auto-upgrade troops that are locked in the party screen.")]
        [SettingPropertyGroup("Resource Safeguards")]
        public bool RespectLockedTroops { get; set; } = true;

        [SettingPropertyBool("Prioritize Noble Troops", Order = 4, RequireRestart = false, HintText = "Give priority weighting to noble/elite troop lines when upgrading.")]
        [SettingPropertyGroup("Resource Safeguards")]
        public bool PrioritizeNobleTroops { get; set; } = true;

        [SettingPropertyDropdown("Shield Infantry Culture Priority", Order = 1, RequireRestart = false, HintText = "Preferred culture for shield infantry.")]
        [SettingPropertyGroup("Culture Priorities")]
        public Dropdown<string> ShieldCulturePriority { get; set; } = new Dropdown<string>(new string[] { "Sturgia", "Empire", "Vlandia", "Aserai", "Battania", "Khuzait" }, 0);

        [SettingPropertyDropdown("Ranged Foot Culture Priority", Order = 2, RequireRestart = false, HintText = "Preferred culture for ranged foot troops.")]
        [SettingPropertyGroup("Culture Priorities")]
        public Dropdown<string> RangedCulturePriority { get; set; } = new Dropdown<string>(new string[] { "Battania", "Empire", "Aserai", "Vlandia", "Khuzait", "Sturgia" }, 0);

        [SettingPropertyDropdown("Cavalry Culture Priority", Order = 3, RequireRestart = false, HintText = "Preferred culture for melee cavalry.")]
        [SettingPropertyGroup("Culture Priorities")]
        public Dropdown<string> CavalryCulturePriority { get; set; } = new Dropdown<string>(new string[] { "Vlandia", "Empire", "Sturgia", "Aserai", "Khuzait", "Battania" }, 0);

        [SettingPropertyDropdown("Horse Archer Culture Priority", Order = 4, RequireRestart = false, HintText = "Preferred culture for horse archers.")]
        [SettingPropertyGroup("Culture Priorities")]
        public Dropdown<string> HorseArcherCulturePriority { get; set; } = new Dropdown<string>(new string[] { "Khuzait", "Aserai", "Empire" }, 0);

        [SettingPropertyDropdown("Shock Infantry Culture Priority", Order = 5, RequireRestart = false, HintText = "Preferred culture for shock infantry.")]
        [SettingPropertyGroup("Culture Priorities")]
        public Dropdown<string> ShockCulturePriority { get; set; } = new Dropdown<string>(new string[] { "Sturgia", "Battania", "Empire", "Aserai", "Vlandia", "Khuzait" }, 0);
    }
}