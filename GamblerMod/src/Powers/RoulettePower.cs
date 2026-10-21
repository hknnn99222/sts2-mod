using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;

namespace GamblerMod.Powers;

// 俄罗斯轮盘（稀有 #32 的能力本体，v1.15 新增）：
// 每回合开始随机开一枪——枪口对着自己（受 X 伤、拿 Y 金）或随机敌人（Z 伤）。
// Amount = 档位：0 = 基础（自伤 3 / 5 金 / 敌 10），1 = 升级（自伤 2 / 7 金 / 敌 14）。
// 随机源用本战斗独立的 Rng（与掷币错开 mixin），保证读档后同回合序列可复现。
public sealed class RoulettePower : CustomPowerModel
{
    private Rng? _rng;
    private const ulong Mixin = 0x524F554C4554UL; // "ROULET"

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        var upgraded = Amount >= 1;
        var selfDamage = upgraded ? 2 : 3;
        var selfGold = upgraded ? 7 : 5;
        var enemyDamage = upgraded ? 14 : 10;

        _rng ??= new Rng(player, player.Character.Id, Mixin);

        if (_rng.NextBool())
        {
            var target = CombatState?.Enemies?.FirstOrDefault(e => e.IsAlive);
            if (target != null)
                await CreatureCmd.Damage(ctx, target, enemyDamage, ValueProp.Move, player.Creature);
            return;
        }

        await CreatureCmd.Damage(ctx, player.Creature, selfDamage, ValueProp.Move, player.Creature);
        await PlayerCmd.GainGold(selfGold, player, false);
    }

    public override List<(string, string)>? Localization => new PowerLoc(
        "俄罗斯轮盘",
        "#每回合开始随机开一枪：自己受 3 伤并得 5 金，或对随机敌人造成 10 伤。",
        "#每回合开始随机开一枪。");
}
