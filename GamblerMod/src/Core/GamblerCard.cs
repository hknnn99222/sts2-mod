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
public abstract class GamblerCard(int cost, CardType type, CardRarity rarity, TargetType target, bool costsX = false)
    : CustomCardModel(cost, type, rarity, target, costsX)
{
    protected int BaseGoldCost;  // 0 = 纯能量卡
    protected bool IsGambleCard; // 打出时是否掷硬币
    // 「风头正盛」词条（v1.6）：数值 ×倍率，结算后清空；与 IsGambleCard 互斥（设计 §2.8）。
    // internal 而非 protected：Core 的 GamblerHooks 要读它来决定是否乘算。
    internal bool IsHeadsStreakCard;
    protected bool KeepStreakAfterPlay; // 「翻本局」击杀时置 true，跳过本次清空

    // 掷硬币结果的视听反馈（M1 方案：特效+音效区分正反面；正式硬币翻转 UI 留待有美术资产后做）
    protected const string HeadsHitFx = "vfx/vfx_heavy_blunt";
    protected const string TailsHitFx = "vfx/vfx_attack_slash";
    protected const string HeadsHitSfx = "heavy_attack.mp3";

    // 金币费的最终值出口：升级减费（§2.7）、假币免单等修正未来挂在这里，CanPlay 与扣费共用
    public virtual int EffectiveGoldCost => BaseGoldCost;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (BaseGoldCost > 0)
        {
            await GoldCost.Pay(Owner, EffectiveGoldCost);
            GD.Print($"[GamblerMod] {Id.Entry} 支付 {EffectiveGoldCost} 金币");
        }

        var heads = IsGambleCard && await GambleService.Flip(Owner, this);
        await ApplyEffect(ctx, play, heads);

        // 「风头正盛」兑现清空（设计 §2.8）：该牌自己已吃满倍率，结算完才清回 1.0。
        if (IsHeadsStreakCard)
        {
            if (KeepStreakAfterPlay)
            {
                GD.Print($"[GamblerMod] {Id.Entry} 击杀续风：倍率保留 {MultiplierService.Label}");
            }
            else
            {
                MultiplierService.Clear();
                GD.Print($"[GamblerMod] {Id.Entry} 兑现风头，倍率清空回 x1.0");
            }
        }

        KeepStreakAfterPlay = false; // 防状态残留到下一次打出
    }

    protected abstract Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads);
}
