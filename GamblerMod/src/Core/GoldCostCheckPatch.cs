using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Core;

// 金币不足 → 卡不可打出（技术设计 §4.3 ①）。
// 挂点选 PlayerCombatState.HasEnoughResourcesFor 的 Postfix——与 BaseLib 自身的附加费用
// 检查（CustomResourcePatches.CheckAdditionalCosts）同一出口，是引擎正规的资源校验管线；
// 卡牌变灰、AI/自动打出拦截等下游行为均由此统一生效。
[HarmonyPatch(typeof(PlayerCombatState), nameof(PlayerCombatState.HasEnoughResourcesFor))]
internal static class GoldCostCheckPatch
{
    [HarmonyPostfix]
    static void CheckGold(PlayerCombatState __instance, CardModel card, ref bool __result, ref UnplayableReason reason)
    {
        if (!__result || card is not GamblerCard gambler) return;
        if (GoldCost.CanAfford(card.Owner, gambler.EffectiveGoldCost)) return;

        __result = false;
        reason = UnplayableReason.BlockedByCardLogic;
    }
}
