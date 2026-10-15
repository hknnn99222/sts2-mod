using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace GamblerMod.Cards.Rare;

// 稀有 #76·押牌（游戏设计 §5.3 / §5.5）：1 能，造成 8 伤害；赌博：正面 → 15 伤害；**反面** → 弃掉 2 张手牌（升级 12 / 19）。
// 锚定：白板 1 能 = 6 伤；期望 (8-4.2 + 15)/2 ≈ 9.4（弃牌折价后）→ ≈1.2× ✓（原 24 伤 = 2.7×，v1.7 收尾时校正）
// 钩子：真赌·押手牌。
public sealed class AnteUp : GamblerCard
{
    public AnteUp() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move),
        new IntVar("HeadsDamage", 15),
        new IntVar("Discards", 2),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars["HeadsDamage"].UpgradeValueBy(4);
    }

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        await DamageCmd.Attack(heads ? (int)DynamicVars["HeadsDamage"].BaseValue : DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(heads ? HeadsHitFx : TailsHitFx, null, heads ? HeadsHitSfx : null)
            .Execute(ctx);

        if (heads) return;

        var hand = PileType.Hand.GetPile(Owner).Cards;
        var count = (int)DynamicVars["Discards"].BaseValue;
        for (var i = 0; i < count && hand.Count > 0; i++)
            await CardCmd.Discard(ctx, hand[0]);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "押牌",
        "#造成 !D! 伤害。*赌博*：正面 → 改为 !HeadsDamage! 伤害；反面 → 弃掉 !Discards! 张手牌。");
}
