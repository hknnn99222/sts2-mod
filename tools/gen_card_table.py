#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""赌徒卡池生成器 / 文档对账工具

用法：
    python tools/gen_card_table.py --audit     # 只对账：报出代码 vs 四处文档的差异
    python tools/gen_card_table.py             # 生成 docs/design/gambler-cards-classified.{xlsx,md}

数据源（唯一真值 = 代码）：
    GamblerMod/src/Cards/{Basic,Common,Uncommon,Rare}/*.cs
文档载体（被对账/被生成的目标）：
    docs/design/gambler-design.md         §3 起始套牌 + §5.1–5.3 常规卡池 + §6 分布统计
    docs/design/gambler-cards.xlsx        常规卡池 / 起始套牌
    docs/design/gambler-design.xlsx       常规卡池 / 起始套牌 / 角色总览 …
    docs/design/gambler-cards-classified.{xlsx,md}   本脚本的产出

约束：改卡数值/效果只动卡文件与 design.md 表；本脚本只做抽取与对账，不猜设计意图。
"""
from __future__ import annotations

import argparse
import glob
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CARDS_DIR = os.path.join(ROOT, "GamblerMod", "src", "Cards")
DESIGN_MD = os.path.join(ROOT, "docs", "design", "gambler-design.md")
CARDS_XLSX = os.path.join(ROOT, "docs", "design", "gambler-cards.xlsx")
DESIGN_XLSX = os.path.join(ROOT, "docs", "design", "gambler-design.xlsx")
OUT_XLSX = os.path.join(ROOT, "docs", "design", "gambler-cards-classified.xlsx")
OUT_MD = os.path.join(ROOT, "docs", "design", "gambler-cards-classified.md")

TYPE_CN = {"Attack": "攻击", "Skill": "技能", "Power": "能力"}
TIER_CN = {"Basic": "起始", "Common": "普通", "Uncommon": "罕见", "Rare": "稀有"}
TIER_ORDER = {"Basic": 0, "Common": 1, "Uncommon": 2, "Rare": 3}
RARITY_OF_TIER = {"普通": "Common", "罕见": "Uncommon", "稀有": "Rare"}

STR_RE = re.compile(r'"((?:[^"\\]|\\.)*)"')

# ---- 功能分类口径（详见 docs/design/gambler-cards-classified.md）----
FUNC = {
    "伤害": ["试手气", "底注", "加注", "买定离手", "看场", "悬红", "乘胜", "吃水", "梭哈", "连续下注",
             "清台", "双倍下注", "叠注", "摇骰", "通杀", "押宝", "翻盘",
             "暗注", "孤注一掷", "俄罗斯轮盘", "爆机", "同花顺", "打庄", "晒冷", "追数",
             "全下", "摊牌", "押牌", "截胡", "满贯", "偷天换日", "收网", "赌命"],
    "产金": ["本钱", "筹码", "返水", "散码", "买码", "派彩", "抽头", "高利贷", "台规", "兑现", "数牌",
             "清码", "利滚利", "弃牌", "放血", "翻本"],
    "格挡": ["护盘", "盲注", "收手", "点码", "护池", "保险", "反水", "保底", "抵押", "割肉", "保本"],
    "资源": ["摸牌", "洗牌", "换码", "流水", "割禾青", "加码", "换牌", "单双", "底牌", "换筹", "分红"],
    "操控·辅助": ["暗记", "假筹", "手气正旺", "叠码", "赌神", "出千", "倍投", "封口", "押注", "千王", "瞒天过海", "再加注", "兑筹", "抽水"],
    "debuff": ["放水", "抽佣"],
}
FUNC_ORDER = ["伤害", "产金", "格挡", "资源", "操控·辅助", "debuff"]
MECH_COLS = ["附加费", "可变投入", "赌博", "多掷", "真赌", "风头·兑现", "风头·只读条件", "花费追踪",
             "击杀产金", "即时产金", "引擎产金", "做局", "条件·赌博计数", "自伤/自消耗", "特异",
             "出千·确定性", "赌命·血线"]

TRUEGAMBLE = {"孤注一掷", "俄罗斯轮盘", "单双", "押宝", "押牌"}   # §5.5 真赌家族
MULTI = set()                                                # v1.8：逐击/多次掷硬币牌已删，全池每张牌只掷一次硬币
VOLUME = {"梭哈", "全下"}                                    # §5.5 可变投入家族（v1.12 再加注改买活力）
ENGINE = {"返水", "台规", "利滚利"}                             # §6 引擎型产金
INSTANT = {"本钱", "筹码", "散码", "买码", "派彩", "高利贷", "清码", "抽头", "老虎机", "数牌",
           "吃水", "洗牌", "翻本"}
READONLY = {"叠注", "底牌"}                                     # §6 只读倍率条件（不兑现）
KILLGOLD = {"悬红", "清台", "爆机", "截胡"}
SPECIAL = {"翻盘", "打庄", "摊牌", "保底", "保本", "换牌", "抽佣", "假筹", "分红"}  # §5.5 特异（兜底；分红=金→HP 的引擎，暂归此列）


# ============================================================ 抽取：代码
def parse_cards() -> list[dict]:
    cards = []
    for path in sorted(glob.glob(os.path.join(CARDS_DIR, "**", "*.cs"), recursive=True)):
        if os.path.basename(path) == "GamblerCards.cs":
            continue
        src = open(path, encoding="utf-8").read()
        ctor = re.search(r'(?:base|GamblerCard)\(\s*([^;]*?)\)\s*\n?\s*\{', src, re.S)
        args = ctor.group(1) if ctor else ""
        m = re.match(r'\s*(\d+)', args)
        cost = int(m.group(1)) if m else 0
        xcost = bool(re.search(r'costsX\s*:\s*true', args))
        ctype = TYPE_CN[re.search(r'CardType\.(\w+)', args).group(1)]
        rarity = re.search(r'CardRarity\.(\w+)', args).group(1)
        g = re.search(r'BaseGoldCost\s*=\s*(\d+)', src)
        gold = int(g.group(1)) if g else 0
        # X 金币系数：v1 用 GoldPerX 字段，2026-10-16 起改用 IntVar("GoldX") 单一真源（卡面随升级同步）
        gpx = (re.search(r'GoldPerX\s*=\s*(\d+)', src)
               or re.search(r'IntVar\(\s*"GoldX"\s*,\s*(\d+)', src))

        mv = re.search(r'CardLoc\(', src)
        i = mv.end() - 1
        depth = 0
        j = i
        while j < len(src):
            if src[j] == '(':
                depth += 1
            elif src[j] == ')':
                depth -= 1
                if depth == 0:
                    break
            j += 1
        strs = STR_RE.findall(src[i + 1:j])
        name, desc = strs[0], "".join(strs[1:])

        vals = {}
        for mm in re.finditer(r'new DamageVar\(\s*(-?\d+)', src):
            vals["D"] = mm.group(1)
        for mm in re.finditer(r'new BlockVar\(\s*(-?\d+)', src):
            vals["B"] = mm.group(1)
        for mm in re.finditer(r'new IntVar\(\s*"(\w+)"\s*,\s*(-?\d+)', src):
            vals[mm.group(1)] = mm.group(2)
        desc = re.sub(r'!(\w+)!', lambda mm: vals.get(mm.group(1), mm.group(0)), desc)
        desc = re.sub(r'\*\*(.+?)\*\*', r'\1', desc)
        desc = re.sub(r'\*(.+?)\*', r'\1', desc)
        desc = desc.replace("\\n", "\n")
        if desc.startswith("#"):
            desc = desc[1:]

        cost_s = f"{'X' if xcost else cost}能"
        if xcost and gpx:
            cost_s += f"+{gpx.group(1)}×X金"
        elif gold:
            cost_s += f"+{gold}金"

        cards.append(dict(
            name=name, code=os.path.basename(path)[:-3], type=ctype, rarity=rarity,
            tier=TIER_CN[rarity], cost=cost_s, cost_energy=("X" if xcost else str(cost)),
            cost_gold=("X" if (xcost and gpx) else (str(gold) if gold else "0")),
            cost_gold_per_x=(gpx.group(1) if (xcost and gpx) else None),
            desc=desc, file=os.path.relpath(path, ROOT).replace("\\", "/"),
            gamble=bool(re.search(r'IsGambleCard\s*=\s*true', src)),
            streak=bool(re.search(r'IsHeadsStreakCard\s*=\s*true', src)),
            keep=bool(re.search(r'KeepStreakAfterPlay\s*=\s*true', src)),
        ))
    return cards


def keyword_of(c: dict) -> str:
    """卡面关键词列（与 design.md 赌博列同一口径）。"""
    tags = []
    if c["name"] in MULTI:
        tags.append("赌博·多掷")
    elif c["name"] in TRUEGAMBLE:
        tags.append("赌博·真赌")
    elif c["gamble"] or c["name"] == "试手气":
        tags.append("赌博")
    if c["streak"]:
        tags.append("风头")
    if c["keep"]:
        tags.append("续风")
    if c["name"] == "弃牌":
        tags.append("消耗")
    return "·".join(tags) if tags else "—"


def gamble_cell_of(c: dict) -> str:
    """design.md/xlsx「赌博」列应有的单元格文本。"""
    if c["name"] in MULTI:
        return "✔×3"
    if c["name"] in TRUEGAMBLE:
        return "✔真赌"
    if c["name"] == "试手气" or c["gamble"]:
        return "✔"
    if c["streak"]:
        return "风头"
    if c["name"] == "弃牌":
        return "消耗"
    return "—"


def mechs_of(c: dict, hook: str) -> list[str]:
    r = []
    n = c["name"]
    if n in VOLUME:
        r.append("可变投入")
    elif hook.startswith("附加费"):
        r.append("附加费")
    if n in MULTI:
        r.append("多掷")
    elif n in TRUEGAMBLE:
        r.append("真赌")
    elif "赌博计数" in hook:
        r.append("条件·赌博计数")
    elif "赌博" in hook or "反向赌博" in hook:
        r.append("赌博")
    if n in READONLY:
        r.append("风头·只读条件")
    elif hook.startswith("兑现") or hook.startswith("倍率折算") or hook.startswith("风头"):
        r.append("风头·兑现")
    if hook.startswith("花费追踪"):
        r.append("花费追踪")
    if n in KILLGOLD:
        r.append("击杀产金")
    if hook.startswith("做局"):
        r.append("做局")
    if n in ENGINE:
        r.append("引擎产金")
    elif n in INSTANT:
        r.append("即时产金")
    if "卖血" in hook or "自伤" in hook or "自消耗" in hook:
        r.append("自伤/自消耗")
    if hook.startswith("出千"):
        r.append("出千·确定性")
    if hook.startswith("赌命"):
        r.append("赌命·血线")
    if n in SPECIAL:
        r.append("特异")
    return r


# ============================================================ 抽取：文档
def parse_design_md() -> dict:
    """→ {name: {num, tier, type, cost, gamble, hook, upgrade, section}}"""
    text = open(DESIGN_MD, encoding="utf-8").read()
    out, section = {}, None
    for line in text.splitlines():
        s = line.strip()
        if s.startswith("### 5.1"):
            section = "普通"
        elif s.startswith("### 5.2"):
            section = "罕见"
        elif s.startswith("### 5.3"):
            section = "稀有"
        elif s.startswith("## 3."):
            section = "起始"
        elif s.startswith("## 4.") or s.startswith("### 5.4"):
            section = None
        if not s.startswith("|") or section is None:
            continue
        c = [x.strip() for x in s.strip("|").split("|")]
        if not (c[0].isdigit() and len(c) > 2 and c[1] and c[1] != "名称"):
            continue
        name = c[1].replace("★", "").replace("**", "").replace(" ×4", "")
        if len(c) >= 8:      # # 名称 类型 费用 赌博 钩子 效果 升级后
            out[name] = dict(num=int(c[0]), tier=section, type=c[2], cost=c[3], gamble=c[4],
                             hook=c[5], upgrade=c[7], source="design.md §5")
        elif len(c) == 7:    # # 名称 类型 费用 关键词 效果 升级后
            out[name] = dict(num=int(c[0]), tier=section, type=c[2], cost=c[3], gamble=c[4],
                             hook=c[4], upgrade=c[6], source="design.md §3")
    return out


def norm_cost(s: str) -> str:
    return (s or "").replace(" ", "").replace("×", "").replace("＋", "+")


def read_xlsx_cards(path: str, sheet: str) -> list[dict]:
    from openpyxl import load_workbook
    ws = load_workbook(path)[sheet]
    rows = []
    for r in ws.iter_rows(values_only=True):
        if r and r[1] is not None and r[3] and str(r[1]) != "#":
            rows.append(dict(num=str(r[1]), rarity=str(r[2] or ""), name=str(r[3]),
                             type=str(r[4] or ""), cost=str(r[5] or ""), gamble=str(r[6] or "")))
    return rows


# ============================================================ 对账
NON_MOD_CARDS = {"打击", "防御"}      # §3 里的原版牌，不在 mod 代码里


def audit(cards: list[dict]) -> tuple[list[str], list[str]]:
    md = parse_design_md()
    byname = {c["name"]: c for c in cards}
    issues = []

    # 1) design.md §3/§5 逐卡
    for c in cards:
        d = md.get(c["name"])
        if not d:
            issues.append(f'[design.md] {c["name"]}：文档表里找不到这张卡（代码有）')
            continue
        if d["tier"] != c["tier"]:
            issues.append(f'[design.md §5] {c["name"]}：档位 文档「{d["tier"]}」≠ 代码「{c["tier"]}」')
        if d["type"] != c["type"]:
            issues.append(f'[design.md] {c["name"]}：类型 文档「{d["type"]}」≠ 代码「{c["type"]}」')
        if norm_cost(d["cost"]) != norm_cost(c["cost"]):
            issues.append(f'[design.md] {c["name"]}：费用 文档「{d["cost"]}」≠ 代码「{c["cost"]}」')
        want = gamble_cell_of(c)
        got = d["gamble"]
        if d["source"].endswith("§3"):
            # §3 用「关键词」列措辞而非符号：赌博牌写「赌博」，产金牌写「产金」
            ok = (got in ("赌博", "✔", "✔真赌", "✔×3")) if want != "—" else (got in ("—", "产金"))
        else:
            ok = got == want
        if not ok:
            issues.append(f'[design.md] {c["name"]}：赌博列 文档「{got}」≠ 代码推出的「{want}」')
    for n in md:
        if n not in byname and n not in NON_MOD_CARDS:
            issues.append(f'[design.md] {n}：文档有、代码没有（卡已删或改名？）')

    # 2) 两份 xlsx 的常规卡池
    for label, path in (("gambler-cards.xlsx", CARDS_XLSX), ("gambler-design.xlsx", DESIGN_XLSX)):
        if not os.path.exists(path):
            continue
        for row in read_xlsx_cards(path, "常规卡池"):
            n = row["name"].replace("★", "").replace("**", "")
            c = byname.get(n)
            if not c:
                issues.append(f'[{label}] {n}：卡表里有、代码没有')
                continue
            if RARITY_OF_TIER.get(row["rarity"]) != c["rarity"]:
                issues.append(f'[{label} #{row["num"]}] {n}：档位「{row["rarity"]}」≠ 代码「{c["tier"]}」')
            if row["type"] != c["type"]:
                issues.append(f'[{label} #{row["num"]}] {n}：类型「{row["type"]}」≠ 代码「{c["type"]}」')
            if norm_cost(row["cost"]) != norm_cost(c["cost"]):
                issues.append(f'[{label} #{row["num"]}] {n}：费用「{row["cost"]}」≠ 代码「{c["cost"]}」')
            want = gamble_cell_of(c)
            if row["gamble"] != want:
                issues.append(f'[{label} #{row["num"]}] {n}：赌博列「{row["gamble"]}」≠「{want}」')
        # 起始套牌页的卡名
        try:
            from openpyxl import load_workbook
            ws = load_workbook(path)["起始套牌"]
            for r in ws.iter_rows(min_row=5, values_only=True):
                if r[1] and r[2] and str(r[1]).isdigit():
                    nm = str(r[2]).replace(" ×4", "").replace("**", "")
                    if nm in NON_MOD_CARDS:
                        continue
                    if nm not in byname:
                        issues.append(f'[{label} 起始套牌 #{r[1]}] 卡名「{nm}」不在代码里')
        except KeyError:
            pass

    # 3) xlsx 表头张数 / 版本标记
    from openpyxl import load_workbook
    n_regular = sum(1 for c in cards if c["rarity"] != "Basic")
    for label, path in (("gambler-cards.xlsx", CARDS_XLSX), ("gambler-design.xlsx", DESIGN_XLSX)):
        wb = load_workbook(path)
        hdr = str(wb["常规卡池"].cell(row=2, column=2).value or "")
        m = re.search(r'(\d+)\s*张', hdr)
        if m and int(m.group(1)) != n_regular:
            issues.append(f'[{label}] 常规卡池表头写「{m.group(1)} 张」，实际 {n_regular} 张 → 表头：「{hdr}」')

    # 4) 计数一致性（§6 分布统计 / §7.3 旋钮）
    n_gamble = sum(1 for c in cards
                   if c["gamble"] or c["name"] == "试手气" or c["name"] in MULTI or c["name"] in TRUEGAMBLE)
    n_streak = sum(1 for c in cards if c["streak"])
    n_mech_rel = n_streak + len(READONLY)
    text = open(DESIGN_MD, encoding="utf-8").read()
    seg6 = text.split("## 6.")[1].split("## 7.")[0] if "## 6." in text else ""
    for key, want in {
        "赌博牌": f"{n_gamble}",
        "风头正盛": f"{n_streak} 挂词条",
    }.items():
        for line in seg6.splitlines():
            if line.strip().startswith("|") and key in line:
                cells = [x.strip() for x in line.strip("|").split("|")]
                if len(cells) >= 3 and not cells[2].startswith(want):
                    issues.append(f'[design.md §6] {key}行：文档「{cells[2][:44]}」应以「{want}」开头')
                break
    for line in text.split("## 7.")[-1].splitlines():
        if "张赌博牌" in line:
            mm = re.search(r'全部 (\d+) 张赌博牌', line)
            if mm and int(mm.group(1)) != n_gamble:
                issues.append(f'[design.md §7.3] 赌博牌数写 {mm.group(1)}，实际 {n_gamble}')
        if "张倍率关联牌" in line:
            mm = re.search(r'全部 (\d+) 张倍率关联牌', line)
            if mm and int(mm.group(1)) != n_mech_rel:
                issues.append(f'[design.md §7.3] 倍率关联牌数写 {mm.group(1)}，实际 {n_mech_rel}')

    # 5) 两份 xlsx 的镜像页（分布统计）必须与 design.md 同步
    if os.path.exists(DESIGN_XLSX) and "## 6." in text:
        wbx = load_workbook(DESIGN_XLSX)
        if "分布统计" in wbx.sheetnames:
            wsx = wbx["分布统计"]
            md6 = {}
            for line in seg6.splitlines():
                if line.strip().startswith("|"):
                    cells = [x.strip() for x in line.strip("|").split("|")]
                    if len(cells) >= 3 and cells[0] not in ("维度", ""):
                        md6[cells[0].split("（")[0]] = cells[2]
            for r in range(5, 20):
                key = wsx.cell(row=r, column=2).value
                val = wsx.cell(row=r, column=4).value
                if not key or not val:
                    continue
                k = str(key).split("（")[0]
                if k not in md6:
                    continue
                lead_x = re.match(r'\d+', str(val))
                lead_m = re.match(r'\d+', md6[k])
                if lead_x and lead_m and lead_x.group() != lead_m.group():
                    issues.append(f'[gambler-design.xlsx 分布统计 R{r}] {k}：xlsx「{val}」'
                                  f'与 design.md §6「{md6[k]}」首位数字不一致')

    lines = [
        f"（参考计数）常规卡池 {n_regular} 张：普 {sum(1 for c in cards if c['rarity']=='Common')}"
        f" / 罕 {sum(1 for c in cards if c['rarity']=='Uncommon')}"
        f" / 稀 {sum(1 for c in cards if c['rarity']=='Rare')}",
        f"（参考计数）涉及金币费 {sum(1 for c in cards if c['cost_gold'] != '0' or c['name'] in VOLUME)} 张"
        f"（固定 {sum(1 for c in cards if c['cost_gold'] not in ('0','X'))} + X 倍 2"
        f" + 可变投入 {sum(1 for c in cards if c['name'] in VOLUME and c['cost_gold'] == '0')}）"
        f" | 风头正盛 {n_streak} 张（关联 {n_mech_rel} 张）"
        f" | 硬币类 {n_gamble} 张（单掷 {n_gamble - len(MULTI)} + 多掷 {len(MULTI)}）",
    ]
    return issues, lines


# ============================================================ 生成
def build(cards: list[dict]) -> None:
    sys.path.insert(0, r"C:\Users\hkn\.zcode\cli\plugins\cache\zcode-plugins-official\spreadsheets\0.1.8\skills\xlsx")
    sys.path.insert(0, r"C:\Users\hkn\.zcode\cli\plugins\cache\zcode-plugins-official\spreadsheets\0.1.8\skills\xlsx\templates")
    from base import (setup_sheet, style_header_row, style_data_row, style_total_row,
                      auto_fit_columns, auto_fit_row_heights, font_caption)
    from openpyxl import Workbook
    from openpyxl.styles import Alignment
    from openpyxl.utils import get_column_letter

    md = parse_design_md()
    func_of = {n: f for f, ns in FUNC.items() for n in ns}
    for c in cards:
        d = md.get(c["name"])
        assert d, f'{c["name"]} 在 design.md 里找不到，先补文档表再生成'
        c["num"] = d["num"]
        c["upgrade"] = d["upgrade"]
        c["hook"] = d["hook"]
        c["mechs"] = mechs_of(c, d["hook"])
        c["mech"] = "·".join(c["mechs"]) if c["mechs"] else "特异"
        c["kw"] = keyword_of(c)
        c["func"] = func_of[c["name"]]
        if not c["mechs"]:
            assert c["name"] in SPECIAL, f'{c["name"]} 未解析出机制标签，钩子=「{d["hook"]}」'

    assert len(cards) == 87, f"卡数应为 87（起始 2 + 常规 85），实际 {len(cards)}"
    for f, ns in FUNC.items():
        got = sum(1 for c in cards if c["func"] == f)
        assert got == len(ns), f"{f}: 名单 {len(ns)} 张，实际 {got} 张"

    cards.sort(key=lambda c: (FUNC_ORDER.index(c["func"]), c["mech"], c["num"]))

    wb = Workbook()
    wb.properties.creator = "Z.ai"

    # --- 总览 ---
    ws = wb.active
    ws.title = "总览"
    headers = ["功能族", "张数", "占比", "机制构成"]
    last_col = len(headers) + 1
    setup_sheet(ws, title="赌徒卡池 · 功能分类总览（v1.8，78 张：起始 2 + 常规 76）", last_col=last_col)
    for i, h in enumerate(headers, start=2):
        ws.cell(row=4, column=i, value=h)
    style_header_row(ws, row_num=4, col_start=2, col_end=last_col)
    overview = []
    for f in FUNC_ORDER:
        grp = [c for c in cards if c["func"] == f]
        cnt = {}
        for c in grp:
            for m in c["mechs"]:
                cnt[m] = cnt.get(m, 0) + 1
        overview.append([f, len(grp), len(grp) / len(cards),
                         "、".join(f"{k} {v}" for k, v in sorted(cnt.items(), key=lambda kv: -kv[1]))])
    r = 5
    for i, row in enumerate(overview):
        for j, v in enumerate(row, start=2):
            ws.cell(row=r, column=j, value=v)
        ws.cell(row=r, column=4).number_format = '0.0%'
        style_data_row(ws, row_num=r, col_start=2, col_end=last_col, row_index=i)
        r += 1
    ws.cell(row=r, column=2, value="合计")
    ws.cell(row=r, column=3, value=len(cards))
    ws.cell(row=r, column=4, value=1.0).number_format = '0.0%'
    ws.cell(row=r, column=5, value=f"{len(FUNC_ORDER)} 个功能族 / {len(cards)} 张卡")
    style_total_row(ws, row_num=r, col_start=2, col_end=last_col)
    notes = [
        "分类口径：按「打出这张卡主要给你什么」归入唯一功能族（不重复计数）；机制可多标签。",
        "机制列沿用设计文档 §5.5 机制词汇表的钩子家族命名。",
        "效果列为代码 CanonicalVars 的真实基础值（占位符已替换），升级后取自设计文档 §5/§3。",
        f"数据源：GamblerMod/src/Cards/*.cs 与 docs/design/gambler-design.md。由 tools/gen_card_table.py 生成。",
    ]
    nr = r + 2
    for t in notes:
        ws.cell(row=nr, column=2, value=t).font = font_caption()
        ws.merge_cells(start_row=nr, start_column=2, end_row=nr, end_column=last_col)
        nr += 1
    auto_fit_columns(ws, min_width=10, max_width=60, header_row=4, data_start_row=5)
    auto_fit_row_heights(ws, header_row=4, data_start_row=5)

    # --- 功能分类 ---
    ws2 = wb.create_sheet("功能分类")
    h2 = ["功能族", "机制", "#", "名称", "档位", "类型", "费用", "词条", "效果（代码实际数值）", "升级后"]
    lc2 = len(h2) + 1
    setup_sheet(ws2, title=f"赌徒卡池 · 逐卡功能分类（{len(cards)} 张，按功能族 / 机制排序）", last_col=lc2)
    for i, h in enumerate(h2, start=2):
        ws2.cell(row=4, column=i, value=h)
    style_header_row(ws2, row_num=4, col_start=2, col_end=lc2)
    for i, c in enumerate(cards):
        rr = 5 + i
        for j, v in enumerate([c["func"], c["mech"], c["num"], c["name"], c["tier"], c["type"],
                               c["cost"], c["kw"], c["desc"], c["upgrade"]], start=2):
            ws2.cell(row=rr, column=j, value=v)
        style_data_row(ws2, row_num=rr, col_start=2, col_end=lc2, row_index=i)
        ws2.cell(row=rr, column=4).alignment = Alignment(horizontal="center", vertical="center")
    auto_fit_columns(ws2, min_width=8, max_width=46, header_row=4, data_start_row=5)
    auto_fit_row_heights(ws2, header_row=4, data_start_row=5)
    ws2.freeze_panes = "D5"
    ws2.auto_filter.ref = f"B4:{get_column_letter(lc2)}{4 + len(cards)}"

    # --- 功能×机制矩阵 ---
    ws3 = wb.create_sheet("功能×机制矩阵")
    h3 = ["功能族 \\ 机制"] + MECH_COLS + ["合计"]
    lc3 = len(h3) + 1
    setup_sheet(ws3, title="功能 × 机制 交叉矩阵（张数）", last_col=lc3)
    for i, h in enumerate(h3, start=2):
        ws3.cell(row=4, column=i, value=h)
    style_header_row(ws3, row_num=4, col_start=2, col_end=lc3)
    r = 5
    for i, f in enumerate(FUNC_ORDER):
        grp = [c for c in cards if c["func"] == f]
        ws3.cell(row=r, column=2, value=f)
        for j, m in enumerate(MECH_COLS, start=3):
            n = sum(1 for c in grp if m in c["mechs"])
            if n:
                ws3.cell(row=r, column=j, value=n)
        ws3.cell(row=r, column=lc3, value=len(grp))
        style_data_row(ws3, row_num=r, col_start=2, col_end=lc3, row_index=i)
        for j in range(3, lc3 + 1):
            ws3.cell(row=r, column=j).alignment = Alignment(horizontal="center", vertical="center")
        r += 1
    ws3.cell(row=r, column=2, value="合计")
    for j, m in enumerate(MECH_COLS, start=3):
        ws3.cell(row=r, column=j, value=sum(1 for c in cards if m in c["mechs"]))
    ws3.cell(row=r, column=lc3, value=len(cards))
    style_total_row(ws3, row_num=r, col_start=2, col_end=lc3)
    multi_tagged = [c["name"] for c in cards if len(c["mechs"]) > 1]
    ws3.cell(row=r + 2, column=2, value=(
        f'口径：一张卡可同时带 2 个机制标签（{"、".join(multi_tagged)}），'
        f'故机制列合计 {sum(len(c["mechs"]) for c in cards)} > 卡数 {len(cards)}；'
        f'「合计」列为各功能族实际张数，总计 {len(cards)}。')).font = font_caption()
    ws3.merge_cells(start_row=r + 2, start_column=2, end_row=r + 2, end_column=lc3)
    ws3.row_dimensions[r + 2].height = 30
    auto_fit_columns(ws3, min_width=8, max_width=20, header_row=4, data_start_row=5)
    auto_fit_row_heights(ws3, header_row=4, data_start_row=5)

    # --- 全卡表 ---
    ws4 = wb.create_sheet("全卡表")
    h4 = ["档位", "#", "名称", "类型", "费用", "词条", "效果（代码实际数值）", "升级后"]
    lc4 = len(h4) + 1
    setup_sheet(ws4, title=f"赌徒全卡表（{len(cards)} 张，按档位 / 编号排序）", last_col=lc4)
    for i, h in enumerate(h4, start=2):
        ws4.cell(row=4, column=i, value=h)
    style_header_row(ws4, row_num=4, col_start=2, col_end=lc4)
    byt = sorted(cards, key=lambda c: (TIER_ORDER[c["rarity"]], c["num"]))
    for i, c in enumerate(byt):
        rr = 5 + i
        for j, v in enumerate([c["tier"], c["num"], c["name"], c["type"], c["cost"], c["kw"],
                               c["desc"], c["upgrade"]], start=2):
            ws4.cell(row=rr, column=j, value=v)
        style_data_row(ws4, row_num=rr, col_start=2, col_end=lc4, row_index=i)
        ws4.cell(row=rr, column=3).alignment = Alignment(horizontal="center", vertical="center")
    auto_fit_columns(ws4, min_width=8, max_width=46, header_row=4, data_start_row=5)
    auto_fit_row_heights(ws4, header_row=4, data_start_row=5)
    ws4.freeze_panes = "C5"
    ws4.auto_filter.ref = f"B4:{get_column_letter(lc4)}{4 + len(byt)}"

    wb.save(OUT_XLSX)
    print("saved", os.path.relpath(OUT_XLSX, ROOT))

    # --- md ---
    L = []
    L.append(f"# 赌徒卡池 · 功能分类（v1.8，{len(cards)} 张：起始 2 + 常规 76）\n")
    L.append("数据源：`GamblerMod/src/Cards/*.cs`（数值为 `CanonicalVars` 真实基础值，占位符已替换）；"
             "编号／档位／升级后取自 `docs/design/gambler-design.md` §5、§3。"
             "本文件由 `tools/gen_card_table.py` 生成，改卡后重跑即可同步。\n")
    L.append("分类口径：按「打出这张卡主要给你什么」归入**唯一功能族**（不重复计数）；"
             "机制列沿用设计文档 §5.5 机制词汇表的钩子家族命名。\n")
    L.append("---\n")
    L.append("## 一、总览\n")
    L.append("| 功能族 | 张数 | 占比 | 机制构成 |")
    L.append("|---|---|---|---|")
    for f, n, p, comp in overview:
        L.append(f"| **{f}** | {n} | {p*100:.1f}% | {comp} |")
    L.append(f"| **合计** | {len(cards)} | 100.0% | {len(FUNC_ORDER)} 个功能族 |")
    L.append("")
    L.append("## 二、功能 × 机制 交叉矩阵\n")
    L.append("| 功能族 \\ 机制 | " + " | ".join(MECH_COLS) + " | 合计 |")
    L.append("|---" * (len(MECH_COLS) + 2) + "|")
    for f in FUNC_ORDER:
        grp = [c for c in cards if c["func"] == f]
        row = [str(sum(1 for c in grp if m in c["mechs"])) or "·" for m in MECH_COLS]
        L.append(f"| **{f}** | " + " | ".join(row) + f" | {len(grp)} |")
    L.append("| **合计** | " + " | ".join(str(sum(1 for c in cards if m in c["mechs"]))
                                        for m in MECH_COLS) + f" | {len(cards)} |")
    L.append("")
    L.append(f"> 口径：一张卡可同时带 2 个机制标签（{'、'.join(multi_tagged)}），"
             f"故机制列合计 {sum(len(c['mechs']) for c in cards)} > 卡数 {len(cards)}；"
             f"「合计」列为各功能族实际张数，总计 {len(cards)}。")
    L.append("")
    L.append("## 三、各族明细\n")
    for f in FUNC_ORDER:
        grp = [c for c in cards if c["func"] == f]
        L.append(f"### {f}（{len(grp)} 张）\n")
        L.append("| 机制 | 成员 |")
        L.append("|---|---|")
        seen = []
        for c in grp:
            if c["mech"] not in seen:
                seen.append(c["mech"])
        for m in seen:
            mem = "、".join(f'{c["name"]}#{c["num"]}' for c in grp if c["mech"] == m)
            L.append(f"| {m} | {mem} |")
        L.append("")
    L.append("## 四、全卡表\n")
    cur = None
    for c in byt:
        if c["tier"] != cur:
            cur = c["tier"]
            L.append(f"### {cur}\n")
            L.append("| # | 名称 | 类型 | 费用 | 词条 | 效果（代码实际数值） | 升级后 |")
            L.append("|---|---|---|---|---|---|---|")
        L.append(f'| {c["num"]} | {c["name"]} | {c["type"]} | {c["cost"]} | {c["kw"]} | '
                 f'{c["desc"].replace(chr(10), "<br>")} | {c["upgrade"]} |')
    L.append("")
    open(OUT_MD, "w", encoding="utf-8").write("\n".join(L))
    print("saved", os.path.relpath(OUT_MD, ROOT))


def main() -> int:
    ap = argparse.ArgumentParser(description="赌徒卡池生成器 / 文档对账")
    ap.add_argument("--audit", action="store_true", help="只对账，不生成文件")
    a = ap.parse_args()
    cards = parse_cards()
    print(f"从代码抽到 {len(cards)} 张卡")
    md = parse_design_md()
    print(f"design.md 表里 {len(md)} 行")
    issues, lines = audit(cards)
    for l in lines:
        print(l)
    if issues:
        print(f"\n发现 {len(issues)} 处差异：")
        for i in issues:
            print("  -", i)
    else:
        print("\n四处文档与代码一致，无差异。")
    if not a.audit:
        build(cards)
    return 1 if issues else 0


if __name__ == "__main__":
    raise SystemExit(main())
