Claude Code 核心架构指令：Phase 2 & 3 融合版
Task: 基于当前的 GameSetup 框架，重构并实现《Tangy TD》的核心肉鸽循环与多职业防御塔系统。

1. 核心架构重构 (Architecture Update)

StatSystem (组件化数值): 编写一个通用的 Stat 类，支持 BaseValue 和 Multiplier。所有英雄和防御塔的 AttackSpeed、Damage、Range 必须通过这个系统计算，以便肉鸽升级时进行百分比叠加。

ExperienceSystem: >     * EnemyBase 死亡时触发 OnDeath 事件。

GameManager 监听该事件，增加 CurrentXP。

实现等级公式：NextLevelXP = CurrentLevel * 100 * 1.2。

2. 肉鸽三选一与刷新机制 (Roguelike Loop)

UpgradeData (ScriptableObject): 字段包含：Title, Description, Rarity, TargetStat (枚举), BonusValue。

UpgradeManager: >     * 实现 GetRandomUpgrades(int count)。

Reroll 机制: 增加一个 RerollCount 变量，UI 界面点击刷新时，扣除次数并重新抽取。

时停逻辑: 弹出 UI 时执行 Time.timeScale = 0，选择后恢复。

3. 多职业防御塔系统 (Tower Classes & Targeting)

ITowerTargeter (接口): 定义 Transform GetTarget(float range, LayerMask mask)。

实现三种索敌逻辑：

NearestTargeter (战士/通用): 锁定距离最近的敌人。

PriorityTargeter (射手): 优先锁定带有 "Flying" 标签或血量最低的敌人。

AoeTargeter (法师/射线): 在范围内寻找敌人密度最高的区域。

PlacementSystem: 允许点击屏幕，在鼠标位置实例化防御塔。需检测 Physics2D.OverlapBox 确保位置没有树木（Environment 层）或已有塔。

4. 视觉反馈与爽感 (Game Juice)

DamagePopup: 实现一个简单的对象池管理受击跳字。

HitFeedback: 编写一个 SimpleFlash 脚本，当敌人受到伤害时，修改其 SpriteRenderer 的 Color 瞬间变白再恢复。

请按照以上逻辑生成代码，优先实现 StatSystem 和 UpgradeManager，并更新 GameSetup 将其串联起来。

💡 为什么这套提示词很“精确”？
StatSystem（数值系统）： 这是视频里那种“极致爽感”的来源。如果你只是简单地 damage += 5，后期升级会很乏力。通过 BaseValue * Multiplier 的方式，可以让玩家在后期拿到翻倍加成时感受到数值爆炸。

ITowerTargeter（索敌接口）： 针对你关心的“法师射线”和“射手索敌”，这个接口让你可以为不同职业写独立的 .cs 文件，而不需要在一个巨大的 if-else 里面写代码。

LayerMask（层级遮罩）： 专门处理你提到的“树木建筑”阻挡问题，这是 Unity 开发中最稳妥的做法。

🛠️ 你的操作步骤：
发送给 Claude： 观察它生成的 UpgradeData 结构。

手动操作： Claude 生成 UpgradeData 脚本后，你在 Unity 中右键创建 3-4 个具体的升级资源（比如：+20% 攻速，+1 子弹数）。

运行测试： 看看升级时那个 “掷骰子（Reroll）” 按钮能不能正常刷新出不同的卡片。

等这一步跑通了，我们下一阶段就让它把“满屏连锁闪电”的特效逻辑加上去！