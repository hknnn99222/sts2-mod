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

// 稀有 #34·豹子（游戏设计 §5.3）：2 能 + 10 金，9×3 次，每击独立赌博：正面则该击改为 18（升级 每击 10 / 正 20）。
// 注意：本卡不走基类 IsGambleCard 单掷流程，ApplyEffect 的 heads 参数无效，逐击自行掷。
public sealed class TripleSevens : GamblerCard
{
    public TripleSevens() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        BaseGoldCost = 10;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, ValueProp.Move),
        new IntVar("HeadsDamage", 18),
        new IntVar("Hits", 3),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["HeadsDamage"].UpgradeValueBy(2);
    }

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var hits = (int)DynamicVars["Hits"].BaseValue;
        for (var i = 0; i < hits; i++)
        {
            var hitHeads = await GambleService.Flip(Owner, this);
            await DamageCmd.Attack(hitHeads ? DynamicVars["HeadsDamage"].BaseValue : DynamicVars.Damage.BaseValue)
                .FromCard(this, play)
                .Targeting(play.Target)
                .WithHitFx(hitHeads ? HeadsHitFx : TailsHitFx, null, hitHeads ? HeadsHitSfx : null)
                .Execute(ctx);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "豹子",
        "#花费 *10 金币*。\n造成 !D! 伤害 !Hits! 次，每击独立*赌博*：正面则该击改为 !HeadsDamage! 伤害。");
}
