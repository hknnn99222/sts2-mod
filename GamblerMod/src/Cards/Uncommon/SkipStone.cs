using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace GamblerMod.Cards.Uncommon;

// 罕见 #70·留一手（游戏设计 §5.2 / §5.5，v1.14 改版）：0 能，将 1 张手牌放回抽牌堆，获得 9 格挡（升级 12）。
// 钩子由「卖血换甲」改为「特异·回牌换甲」——把暂时没用的牌洗回牌库，换一格防线。
// 注：选卡界面沿用引擎自带的"弃置"提示文案（引擎没有"放回抽牌堆"的预设提示），实际动作是放回抽牌堆。
public sealed class SkipStone : GamblerCard
{
    public SkipStone() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(9, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var hand = PileType.Hand.GetPile(Owner).Cards;
        if (hand.Count > 0)
        {
            var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1, 1);
            var picked = (await CardSelectCmd.FromHand(ctx, Owner, prefs, c => c != this, this)).ToList();
            if (picked.Count > 0)
                await CardPileCmd.Add(picked[0], PileType.Draw, CardPilePosition.Random, this, false);
        }

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "留一手",
        "#将 1 张手牌放回抽牌堆，获得 !B! 格挡。");
}
