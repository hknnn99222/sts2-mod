using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using GamblerMod.Character;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Core;

// 全部赌徒卡的模板方法基类（技术设计 §4.1，M1 冻结候选）：
//   打出流程固化 = 扣金币费 →（赌博卡）掷硬币 → ApplyEffect(heads)
//   可打出检查 = GoldCostCheckPatch（引擎 HasEnoughResourcesFor 出口）
// 子类只做四件事：base 构造传费用四元组、构造体声明 BaseGoldCost/IsGambleCard、
// CanonicalVars 声明数值、ApplyEffect 写效果；升级走 OnUpgrade + UpgradeValueBy。
// 约束④：改卡数值/效果只动卡文件，本类与 Core 层不动。
[Pool(typeof(GamblerCardPool))]
public abstract class GamblerCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : CustomCardModel(cost, type, rarity, target)
{
    protected int BaseGoldCost;  // 0 = 纯能量卡
    protected bool IsGambleCard; // 打出时是否掷硬币

    // 掷硬币结果的视听反馈（M1 方案：特效+音效区分正反面；正式硬币翻转 UI 留待有美术资产后做）
    protected const string HeadsHitFx = "vfx/vfx_heavy_blunt";
    protected const string TailsHitFx = "vfx/vfx_attack_slash";
    protected const string HeadsHitSfx = "heavy_attack.mp3";

    // 金币费的最终值出口：升级减费（§2.7）、假币免单等修正未来挂在这里，CanPlay 与扣费共用
    public int EffectiveGoldCost => BaseGoldCost;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (BaseGoldCost > 0)
        {
            await GoldCost.Pay(Owner, EffectiveGoldCost);
            GD.Print($"[GamblerMod] {Id.Entry} 支付 {EffectiveGoldCost} 金币");
        }

        var heads = IsGambleCard && GambleService.Flip(Owner, this);
        await ApplyEffect(ctx, play, heads);
    }

    protected abstract Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads);
}
