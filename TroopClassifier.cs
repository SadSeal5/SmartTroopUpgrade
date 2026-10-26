using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace SmartTroopUpgrade
{
    public static class TroopClassifier
    {
        public static TroopRole Classify(CharacterObject troop)
        {
            if (troop == null)
            {
                return TroopRole.Unknown;
            }

            if (troop.IsMounted)
            {
                if (troop.IsRanged)
                {
                    return TroopRole.HorseArcher;
                }
                return TroopRole.MeleeCavalry;
            }

            if (troop.IsRanged)
            {
                return TroopRole.RangedFoot;
            }

            // Regular Infantry: Check if character has a shield in battle equipment
            Equipment equipment = troop.FirstBattleEquipment ?? troop.Equipment;
            if (equipment != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    EquipmentElement element = equipment[i];
                    if (element.Item != null)
                    {
                        if ((element.Item.PrimaryWeapon != null && element.Item.PrimaryWeapon.IsShield) ||
                            element.Item.ItemType == ItemObject.ItemTypeEnum.Shield)
                        {
                            return TroopRole.ShieldInfantry;
                        }
                    }
                }
            }

            return TroopRole.ShockInfantry;
        }
    }
}