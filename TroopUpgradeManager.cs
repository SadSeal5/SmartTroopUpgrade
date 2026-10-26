using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SmartTroopUpgrade
{
    public static class TroopUpgradeManager
    {
        public static int RunUpgrade()
        {
            Settings settings = Settings.Instance;
            if (settings == null || !settings.ModEnabled)
            {
                return 0;
            }

            MobileParty mainParty = MobileParty.MainParty;
            Hero mainHero = Hero.MainHero;
            if (mainParty == null || mainHero == null || mainParty.MemberRoster == null || mainParty.Party == null)
            {
                return 0;
            }

            bool respectLocked = settings.RespectLockedTroops;
            HashSet<string> lockedTroopIds = respectLocked ? GetLockedTroopIds() : null;
            HashSet<string> lockedItemIds = respectLocked ? GetLockedInventoryIds() : null;

            int totalUpgradedOverall = 0;
            int passGuard = 0;
            bool upgradedAny;

            do
            {
                upgradedAny = false;
                passGuard++;
                if (passGuard > 50)
                {
                    break;
                }

                TroopRoster memberRoster = mainParty.MemberRoster;
                int rosterCount = memberRoster.Count;

                // 1. Count regular combat troops per TroopRole
                var roleCounts = new Dictionary<TroopRole, int>
                {
                    [TroopRole.ShieldInfantry] = 0,
                    [TroopRole.ShockInfantry] = 0,
                    [TroopRole.RangedFoot] = 0,
                    [TroopRole.MeleeCavalry] = 0,
                    [TroopRole.HorseArcher] = 0
                };

                int totalCombatRegulars = 0;
                for (int i = 0; i < rosterCount; i++)
                {
                    TroopRosterElement el = memberRoster.GetElementCopyAtIndex(i);
                    CharacterObject ch = el.Character;
                    if (ch == null || ch.IsHero)
                    {
                        continue;
                    }

                    TroopRole role = TroopClassifier.Classify(ch);
                    if (roleCounts.ContainsKey(role))
                    {
                        roleCounts[role] += el.Number;
                    }
                    totalCombatRegulars += el.Number;
                }

                // 2. Calculate target quotas and deficits
                float shieldRatio = Math.Max(0f, settings.ShieldInfantryPercent) / 100f;
                float shockRatio = Math.Max(0f, settings.ShockInfantryPercent) / 100f;
                float rangedRatio = Math.Max(0f, settings.RangedPercent) / 100f;
                float cavRatio = Math.Max(0f, settings.CavalryPercent) / 100f;
                float haRatio = Math.Max(0f, settings.HorseArcherPercent) / 100f;

                var quotas = new Dictionary<TroopRole, int>
                {
                    [TroopRole.ShieldInfantry] = (int)Math.Round(totalCombatRegulars * shieldRatio),
                    [TroopRole.ShockInfantry] = (int)Math.Round(totalCombatRegulars * shockRatio),
                    [TroopRole.RangedFoot] = (int)Math.Round(totalCombatRegulars * rangedRatio),
                    [TroopRole.MeleeCavalry] = (int)Math.Round(totalCombatRegulars * cavRatio),
                    [TroopRole.HorseArcher] = (int)Math.Round(totalCombatRegulars * haRatio)
                };

                var deficits = new Dictionary<TroopRole, int>
                {
                    [TroopRole.ShieldInfantry] = quotas[TroopRole.ShieldInfantry] - roleCounts[TroopRole.ShieldInfantry],
                    [TroopRole.ShockInfantry] = quotas[TroopRole.ShockInfantry] - roleCounts[TroopRole.ShockInfantry],
                    [TroopRole.RangedFoot] = quotas[TroopRole.RangedFoot] - roleCounts[TroopRole.RangedFoot],
                    [TroopRole.MeleeCavalry] = quotas[TroopRole.MeleeCavalry] - roleCounts[TroopRole.MeleeCavalry],
                    [TroopRole.HorseArcher] = quotas[TroopRole.HorseArcher] - roleCounts[TroopRole.HorseArcher]
                };

                // 3. Iterate through member roster and find eligible upgrades
                for (int i = 0; i < memberRoster.Count; i++)
                {
                    TroopRosterElement element = memberRoster.GetElementCopyAtIndex(i);
                    CharacterObject troop = element.Character;
                    if (troop == null || troop.IsHero)
                    {
                        continue;
                    }

                    if (respectLocked && lockedTroopIds != null && troop.StringId != null && lockedTroopIds.Contains(troop.StringId))
                    {
                        continue;
                    }

                    if (troop.UpgradeTargets == null || troop.UpgradeTargets.Length == 0)
                    {
                        continue;
                    }

                    int baseXpCost = troop.GetUpgradeXpCost(mainParty.Party, 0);
                    if (baseXpCost <= 0)
                    {
                        continue;
                    }

                    int readyCount = element.Xp / baseXpCost;
                    readyCount = Math.Min(readyCount, element.Number);
                    if (readyCount <= 0)
                    {
                        continue;
                    }

                    // Check if any upgrade branch is cavalry and whether horses are insufficient
                    bool hasCavalryTarget = false;
                    bool cavalryTargetBlockedByHorses = false;
                    for (int tIdx = 0; tIdx < troop.UpgradeTargets.Length; tIdx++)
                    {
                        CharacterObject t = troop.UpgradeTargets[tIdx];
                        if (t == null) continue;

                        ItemCategory req = t.UpgradeRequiresItemFromCategory;
                        if (req != null)
                        {
                            hasCavalryTarget = true;
                            int avail = GetAvailableHorses(mainParty, req, respectLocked, lockedItemIds);
                            if (avail <= settings.MinHorseReserve)
                            {
                                cavalryTargetBlockedByHorses = true;
                            }
                        }
                    }

                    // Guardrail: Do not upgrade this troop to foot as a fallback if the player intended cavalry; leave ready!
                    if (hasCavalryTarget && cavalryTargetBlockedByHorses)
                    {
                        if ((deficits.TryGetValue(TroopRole.MeleeCavalry, out int dCav) && dCav > 0) ||
                            (deficits.TryGetValue(TroopRole.HorseArcher, out int dHa) && dHa > 0))
                        {
                            continue;
                        }
                    }

                    // Evaluate each upgrade target
                    CharacterObject bestTarget = null;
                    int bestTargetIndex = -1;
                    TroopRole bestRole = TroopRole.Unknown;
                    float bestScore = float.MinValue;
                    int bestGoldCost = 0;
                    int bestXpCost = 0;
                    ItemCategory bestReqCategory = null;
                    int bestAvailHorses = 0;

                    for (int tIdx = 0; tIdx < troop.UpgradeTargets.Length; tIdx++)
                    {
                        CharacterObject target = troop.UpgradeTargets[tIdx];
                        if (target == null) continue;

                        TroopRole targetRole = TroopClassifier.Classify(target);

                        // Check horse requirement
                        ItemCategory reqCat = target.UpgradeRequiresItemFromCategory;
                        int availHorses = int.MaxValue;
                        if (reqCat != null)
                        {
                            availHorses = GetAvailableHorses(mainParty, reqCat, respectLocked, lockedItemIds);
                            if (availHorses <= settings.MinHorseReserve)
                            {
                                continue;
                            }
                        }

                        // Check gold cost
                        int unitGoldCost = troop.GetUpgradeGoldCost(mainParty.Party, tIdx);
                        if (unitGoldCost > 0 && (mainHero.Gold - unitGoldCost < settings.MinGoldReserve))
                        {
                            continue;
                        }

                        int unitXpCost = troop.GetUpgradeXpCost(mainParty.Party, tIdx);
                        if (unitXpCost <= 0)
                        {
                            unitXpCost = baseXpCost;
                        }

                        // Score calculation
                        float deficitScore = (deficits.TryGetValue(targetRole, out int def) && def > 0) ? (def + 1f) : 0.2f;
                        float cultureMultiplier = GetCultureMultiplier(target, targetRole, settings);
                        bool isNoble = IsNobleTroop(target);
                        float nobleMultiplier = (settings.PrioritizeNobleTroops && isNoble) ? 1.4f : 1.0f;
                        float score = deficitScore * cultureMultiplier * nobleMultiplier;

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestTarget = target;
                            bestTargetIndex = tIdx;
                            bestRole = targetRole;
                            bestGoldCost = unitGoldCost;
                            bestXpCost = unitXpCost;
                            bestReqCategory = reqCat;
                            bestAvailHorses = availHorses;
                        }
                    }

                    if (bestTarget != null && bestTargetIndex >= 0)
                    {
                        int countToUpgrade = readyCount;

                        if (bestGoldCost > 0)
                        {
                            int affordableGold = Math.Max(0, (mainHero.Gold - settings.MinGoldReserve) / bestGoldCost);
                            countToUpgrade = Math.Min(countToUpgrade, affordableGold);
                        }

                        if (bestReqCategory != null)
                        {
                            int affordableHorses = Math.Max(0, bestAvailHorses - settings.MinHorseReserve);
                            countToUpgrade = Math.Min(countToUpgrade, affordableHorses);
                        }

                        if (countToUpgrade > 0)
                        {
                            int totalXpCost = countToUpgrade * bestXpCost;
                            int totalGold = countToUpgrade * bestGoldCost;

                            memberRoster.AddToCounts(troop, -countToUpgrade, false, 0, -totalXpCost);
                            memberRoster.AddToCounts(bestTarget, countToUpgrade, false, 0, 0);

                            if (totalGold > 0)
                            {
                                mainHero.ChangeHeroGold(-totalGold);
                            }

                            if (bestReqCategory != null)
                            {
                                ConsumeHorses(mainParty, bestReqCategory, countToUpgrade, respectLocked, lockedItemIds);
                            }

                            totalUpgradedOverall += countToUpgrade;
                            upgradedAny = true;
                            // Restart pass to refresh roster indices and recalculate counts accurately
                            break;
                        }
                    }
                }

            } while (upgradedAny);

            return totalUpgradedOverall;
        }

        private static float GetCultureMultiplier(CharacterObject target, TroopRole role, Settings settings)
        {
            string preferred = null;
            switch (role)
            {
                case TroopRole.ShieldInfantry:
                    preferred = settings.ShieldCulturePriority?.SelectedValue;
                    break;
                case TroopRole.ShockInfantry:
                    preferred = settings.ShockCulturePriority?.SelectedValue;
                    break;
                case TroopRole.RangedFoot:
                    preferred = settings.RangedCulturePriority?.SelectedValue;
                    break;
                case TroopRole.MeleeCavalry:
                    preferred = settings.CavalryCulturePriority?.SelectedValue;
                    break;
                case TroopRole.HorseArcher:
                    preferred = settings.HorseArcherCulturePriority?.SelectedValue;
                    break;
            }

            if (!string.IsNullOrEmpty(preferred) && target.Culture != null)
            {
                if (string.Equals(target.Culture.StringId, preferred, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(target.Culture.Name?.ToString(), preferred, StringComparison.OrdinalIgnoreCase))
                {
                    return 1.5f;
                }
            }

            return 1.0f;
        }

        private static bool IsNobleTroop(CharacterObject troop)
        {
            if (troop == null) return false;
            if (troop.Tier >= 5) return true;

            string id = troop.StringId;
            if (string.IsNullOrEmpty(id)) return false;

            return id.IndexOf("noble", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   id.IndexOf("elite", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   id.IndexOf("knight", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   id.IndexOf("hero", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   id.IndexOf("fian", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   id.IndexOf("druzhinnik", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   id.IndexOf("cataphract", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   id.IndexOf("vanguard", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   id.IndexOf("faris", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int GetAvailableHorses(MobileParty party, ItemCategory category, bool respectLocked, HashSet<string> lockedItemIds)
        {
            int total = 0;
            if (party?.ItemRoster == null || category == null)
            {
                return 0;
            }

            for (int i = 0; i < party.ItemRoster.Count; i++)
            {
                ItemRosterElement element = party.ItemRoster.GetElementCopyAtIndex(i);
                ItemObject item = element.EquipmentElement.Item;
                if (item != null && item.ItemCategory == category)
                {
                    if (respectLocked && lockedItemIds != null && item.StringId != null && lockedItemIds.Contains(item.StringId))
                    {
                        continue;
                    }
                    total += element.Amount;
                }
            }
            return total;
        }

        private static void ConsumeHorses(MobileParty party, ItemCategory category, int count, bool respectLocked, HashSet<string> lockedItemIds)
        {
            if (party?.ItemRoster == null || category == null || count <= 0)
            {
                return;
            }

            int needed = count;
            for (int i = 0; i < party.ItemRoster.Count && needed > 0; i++)
            {
                ItemRosterElement element = party.ItemRoster.GetElementCopyAtIndex(i);
                ItemObject item = element.EquipmentElement.Item;
                if (item != null && item.ItemCategory == category)
                {
                    if (respectLocked && lockedItemIds != null && item.StringId != null && lockedItemIds.Contains(item.StringId))
                    {
                        continue;
                    }

                    int toRemove = Math.Min(element.Amount, needed);
                    party.ItemRoster.AddToCounts(element.EquipmentElement, -toRemove);
                    needed -= toRemove;
                }
            }
        }

        private static HashSet<string> GetLockedTroopIds()
        {
            var locked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var tracker = Campaign.Current?.GetCampaignBehavior<IViewDataTracker>();
                if (tracker != null)
                {
                    var locks = tracker.GetPartyTroopLocks();
                    if (locks != null)
                    {
                        foreach (var id in locks)
                        {
                            if (!string.IsNullOrEmpty(id))
                            {
                                locked.Add(id);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return locked;
        }

        private static HashSet<string> GetLockedInventoryIds()
        {
            var locked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var tracker = Campaign.Current?.GetCampaignBehavior<IViewDataTracker>();
                if (tracker != null)
                {
                    var locks = tracker.GetInventoryLocks();
                    if (locks != null)
                    {
                        foreach (var id in locks)
                        {
                            if (!string.IsNullOrEmpty(id))
                            {
                                locked.Add(id);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return locked;
        }
    }
}