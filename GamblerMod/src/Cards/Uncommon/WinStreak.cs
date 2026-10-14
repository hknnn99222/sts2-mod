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

namespace GamblerMod.Cards.Uncommon;

// 罕见 #18·连胜（游戏设计 §5.2 / §5.4）：1 能，5 伤害；赌博：正面 → 再次触发本效果并再次赌博
//（最多连锁 2 次，升级后基础 7 伤害）。连锁与"赌神"组合由上限截断。
public sealed class WinStreak : GamblerCard
{
    private const int MaxChain = 2;

    public WinStreak() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var chain = 0;
        var hit = heads;
        while (true)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, play)
                .Targeting(play.Target)
                .WithHitFx(hit ? HeadsHitFx : TailsHitFx, null, hit ? HeadsHitSfx : null)
                .Execute(ctx);

            if (!hit || chain >= MaxChain) break;
            chain++;
            hit = await GambleService.Flip(Owner, this); // 连锁掷
        }
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "连胜",
        "#造成 !D! 伤害。*赌博*：正面 → 再次触发本效果并再次*赌博*（最多连锁 2 次）。");
}
