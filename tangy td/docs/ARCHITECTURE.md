# Tangy TD - 游戏架构文档

> 本文档帮助你理解项目结构和 Unity 开发模式，即使不熟悉 Unity 也能快速定位问题。

---

## 1. 项目目录结构

```
tangy td/                          ← Unity 项目根目录
├── Assets/                        ← 所有游戏资源（你主要关注这里）
│   ├── Scenes/
│   │   └── SampleScene.unity      ← 游戏场景文件（Unity 编辑器打开的"关卡"）
│   ├── Scripts/                   ← 所有 C# 代码
│   │   ├── Core/                  ← 核心管理器
│   │   │   ├── GameManager.cs     ← 全局状态：生命、经验、等级、游戏状态
│   │   │   ├── GameSetup.cs       ← 场景自动搭建（运行时创建所有对象）
│   │   │   └── WaveManager.cs     ← 波次循环、敌人类型选择、Boss 波
│   │   ├── Hero/                  ← 玩家英雄相关
│   │   │   ├── HeroController.cs  ← WASD 移动、属性（HP/攻击力/攻速/射程）
│   │   │   ├── HeroShooter.cs     ← 自动瞄准射击、多重射击、暴击
│   │   │   └── Bullet.cs          ← 子弹飞行、碰撞伤害、穿透
│   │   ├── Enemy/                 ← 敌人相关
│   │   │   ├── EnemyBase.cs       ← 敌人基础：沿路径移动、受伤、死亡
│   │   │   └── EnemySpawner.cs    ← 按类型生成敌人（含 DestroyNotifier）
│   │   ├── Path/                  ← 路径系统
│   │   │   └── WaypointPath.cs    ← 路点数组、Gizmo 可视化
│   │   ├── Upgrade/               ← 升级系统
│   │   │   ├── UpgradeData.cs     ← 升级数据定义（ScriptableObject）
│   │   │   ├── UpgradeManager.cs  ← 升级池、三选一、效果应用、合成检测
│   │   │   └── UpgradeUI.cs       ← 升级卡牌 UI（全代码构建）
│   │   ├── UI/                    ← 界面
│   │   │   └── HUDManager.cs      ← 生命/波次/等级/经验条/公告
│   │   └── Utils/                 ← 工具
│   │       └── SpriteFactory.cs   ← 代码生成纯色 Sprite（方形/圆形）
│   ├── ScriptableObjects/         ← SO 资产存放处（目前运行时生成）
│   ├── Prefabs/                   ← 预制体存放处（目前未使用）
│   └── Materials/                 ← 材质存放处（目前未使用）
├── ProjectSettings/               ← Unity 项目配置（物理、输入、画质等）
├── Packages/                      ← Unity 包管理（类似 npm 的 package.json）
├── docs/                          ← 项目文档
├── CLAUDE.md                      ← AI 助手上下文文件
├── .gitignore                     ← Git 忽略规则
└── .claude/                       ← Claude Code 配置
    └── settings.json              ← Hook 配置（修改 .cs 后提醒 Unity 操作）

不需要关注的目录（自动生成，已被 .gitignore 忽略）：
├── Library/                       ← Unity 编辑器缓存（~22000 文件）
├── Temp/                          ← 编译临时文件
├── Logs/                          ← 编辑器日志
└── UserSettings/                  ← 个人编辑器偏好
```

---

## 2. Unity 核心概念速查

### GameObject 和 Component

Unity 中一切都是 **GameObject**（游戏对象），它本身是个空容器。
功能靠挂载 **Component**（组件）实现：

```
GameObject "Hero"
  ├── Transform          ← 位置/旋转/缩放（每个 GO 自带）
  ├── SpriteRenderer     ← 渲染图片（蓝色方块）
  ├── Rigidbody2D        ← 物理引擎（移动碰撞）
  ├── BoxCollider2D      ← 碰撞体（检测接触）
  ├── HeroController     ← 我们写的脚本（移动逻辑）
  └── HeroShooter        ← 我们写的脚本（射击逻辑）
```

### MonoBehaviour 生命周期

我们写的每个脚本都继承 `MonoBehaviour`，Unity 按顺序调用这些方法：

```
Awake()        → 对象创建时调用一次（初始化自身）
Start()        → 第一帧前调用一次（初始化依赖其他对象的逻辑）
Update()       → 每帧调用（游戏逻辑，如输入检测、AI）
FixedUpdate()  → 固定间隔调用（物理相关，如移动）
OnDestroy()    → 对象销毁时调用（清理事件订阅）
```

### ScriptableObject

**不挂在 GameObject 上**的数据容器，用来定义"配置数据"。
本项目中 `UpgradeData` 就是 SO —— 定义一个升级的名称、效果、数值。

---

## 3. 架构设计详解

### 3.1 单例模式（全局管理器）

三个管理器使用单例模式，通过 `类名.Instance` 在任何地方访问：

```
GameManager.Instance     → 游戏状态、生命、经验、等级
WaveManager.Instance     → 当前波次、波次循环控制
UpgradeManager.Instance  → 升级池、已获得升级、属性加成
```

**实现方式：**
```csharp
public static GameManager Instance { get; private set; }
private void Awake()
{
    if (Instance != null) { Destroy(gameObject); return; }
    Instance = this;
}
```

### 3.2 事件驱动（解耦通信）

各系统通过 C# 事件（`System.Action`）通信，避免互相直接引用：

```
GameManager.OnLevelUp(int level)
    │
    ├──→ UpgradeManager.OnLevelUp()    → 弹出三选一
    │        │
    │        └──→ UpgradeUI.ShowChoices()  → 显示卡牌 UI
    │
    └──→ HUDManager.UpdateLevelText()  → 更新等级显示

GameManager.OnLivesChanged(int lives)
    └──→ HUDManager.UpdateLives()      → 更新生命显示

WaveManager.OnWaveStart(int wave)
    └──→ HUDManager.OnWaveStart()      → 显示"第 N 波"公告
```

**为什么这样设计？**
- GameManager 不需要知道 HUD 和 UpgradeUI 的存在
- 新增 UI 元素只需订阅事件，无需改 GameManager 代码
- 调试时可以单独测试每个系统

### 3.3 GameSetup 一键搭建

```
GameSetup.Awake() 按顺序创建：
    1. GameManager       → 全局状态
    2. WaypointPath      → 路径（根据摄像机自适应）
    3. Hero              → 英雄（SpriteRenderer + Controller + Shooter）
    4. EnemySpawner      → 生成器（引用 path）
    5. WaveManager       → 波次管理（引用 spawner）
    6. UpgradeManager    → 升级管理
    7. Canvas + HUD      → UI 系统
    8. UpgradeUI         → 升级选择界面
    9. EventSystem       → UI 点击事件处理
```

**为什么用代码搭建而不是手动拖拽？**
- Game Jam 时间紧张，避免 Scene 文件冲突
- 所有配置集中在一个文件，容易修改
- 不需要理解 Unity 编辑器复杂的 Inspector 面板

---

## 4. 数据流：一个完整游戏循环

```
[游戏开始]
    │
    ▼
WaveManager 启动波次循环 ──→ 1.5 秒后开始第 1 波
    │
    ▼
EnemySpawner.SpawnEnemy() ──→ 创建敌人 GameObject
    │                              │
    │                              ▼
    │                         EnemyBase.Init()
    │                         设置血量/速度/颜色/路径
    │
    ▼
[敌人沿路径移动] ──→ 到达终点？──→ GameManager.OnEnemyReachedEnd()
    │                                    │
    │                                    ▼
    │                              playerLives--
    │                              Lives <= 0 → Game Over
    │
    ▼
[英雄在范围内] ──→ HeroShooter 检测到敌人
    │                    │
    │                    ▼
    │              SpawnBullet() ──→ Bullet 飞向敌人
    │                                    │
    │                                    ▼
    │                              OnTriggerEnter2D()
    │                              EnemyBase.TakeDamage()
    │                                    │
    │                                    ▼
    │                              HP <= 0 → Die()
    │                                    │
    │                                    ▼
    │                              GameManager.AddExperience()
    │                                    │
    │                                    ▼
    │                              经验够了？→ OnLevelUp 事件
    │                                    │
    │                                    ▼
    │                              UpgradeManager → 暂停游戏
    │                              显示三选一 UI
    │                                    │
    │                                    ▼
    │                              玩家点击选择
    │                              应用效果 → 恢复游戏
    │
    ▼
[所有敌人死亡] ──→ WaveManager 检测到 enemiesAlive == 0
    │
    ▼
波间倒计时 ──→ 下一波开始（敌人更多更强）
    │
    ▼
[循环继续...]
```

---

## 5. 关键脚本职责一览

| 脚本 | 职责 | 关键属性/方法 |
|------|------|---------------|
| **GameManager** | 全局状态中心 | `playerLives`, `experience`, `level`, `AddExperience()`, `OnLevelUp` 事件 |
| **GameSetup** | 场景搭建 | `Awake()` 中创建所有 GameObject |
| **WaveManager** | 波次循环 | `WaveLoop()` 协程, `OnEnemyDied()`, `OnWaveStart` 事件 |
| **HeroController** | 英雄移动 | `moveSpeed`, `attackDamage`, `attackSpeed`, `attackRange` |
| **HeroShooter** | 自动射击 | `FindNearestEnemy()`, `SpawnBullet()`, MultiShot/暴击支持 |
| **Bullet** | 子弹行为 | `Init(direction, damage, pierce)`, `OnTriggerEnter2D()` |
| **EnemyBase** | 敌人行为 | `Init()`, `MoveAlongPath()`, `TakeDamage()`, `Die()` |
| **EnemySpawner** | 敌人工厂 | `SpawnEnemy(type, wave, hpMult, speedMult)` |
| **WaypointPath** | 路径数据 | `waypoints[]`, `GetPosition(index)`, Gizmo 可视化 |
| **UpgradeData** | 升级定义 | `upgradeName`, `effect`, `value`, `cardColor` |
| **UpgradeManager** | 升级逻辑 | `SelectUpgrade()`, `ApplyEffect()`, `CheckSynthesis()` |
| **UpgradeUI** | 升级界面 | `ShowChoices()`, `CreateCard()`, 全代码构建 UI |
| **HUDManager** | 状态显示 | `UpdateLives()`, `UpdateExp()`, `OnWaveStart()` |
| **SpriteFactory** | 图形生成 | `CreateSquare(color)`, `CreateCircle(color)`, 内存缓存 |

---

## 6. 常见问题定位

### "游戏画面什么都没有"
→ 检查场景中是否有 `_Setup` 对象且挂了 `GameSetup` 脚本

### "敌人不动 / 不生成"
→ 检查 `GameManager.state` 是否为 `Playing`
→ 检查 `WaveManager.spawner` 引用是否为 null（Console 看报错）

### "升级界面不弹出"
→ 经验值是否够升级？检查 Console 有无 "Level Up" 日志
→ `UpgradeManager.OnLevelUp` 是否正确订阅了 `GameManager.OnLevelUp`

### "UI 点击没反应"
→ 检查场景中是否有 EventSystem（GameSetup 会自动创建）
→ Canvas 是否有 GraphicRaycaster 组件

### "角色/敌人太大或太小"
→ 英雄：`GameSetup.cs` 中 `heroGO.transform.localScale`
→ 敌人：`EnemySpawner.cs` 中各类型的 `scale` 值
→ 子弹：`HeroShooter.cs` 中 `bulletGO.transform.localScale`

### "敌人跑出屏幕"
→ `GameSetup.cs` 中路径坐标基于 `Camera.main` 计算
→ 确认 Main Camera 的 `orthographicSize` 设置合理（默认 5）
