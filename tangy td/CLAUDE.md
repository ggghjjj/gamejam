# Tangy TD - 塔防 + 肉鸽 (Game Jam 48h)

## 项目概述
Unity 2D 塔防 + Roguelike 游戏，还原 Steam 游戏《Tangy TD》。
固定路径塔防 + 自由移动英雄射击，升级三选一肉鸽循环。

## 技术栈
- Unity 2022+ (2D)
- 无外部美术资源，全部使用 `SpriteFactory` 代码生成纯色几何体
- 无外部字体，使用 Unity 内置 `LegacyRuntime.ttf`（Windows 系统支持中文回退）
- UI 全部代码动态构建（无需手动拖拽 UI 元素）

## 架构约定

### 文件夹结构
```
Assets/Scripts/
  Core/       GameManager(单例), WaveManager(单例), GameSetup(场景搭建)
  Hero/       HeroController(移动), HeroShooter(射击), Bullet(子弹)
  Enemy/      EnemyBase(敌人基础), EnemySpawner(生成器)
  Path/       WaypointPath(路径系统)
  Upgrade/    UpgradeData(SO), UpgradeManager(单例), UpgradeUI
  UI/         HUDManager
  Utils/      SpriteFactory(精灵生成)
```

### 关键设计模式
- **GameSetup 一键搭建**: 场景中只需一个空 GameObject 挂 `GameSetup` 脚本，运行时自动创建所有游戏对象（Hero, Path, Spawner, WaveManager, Canvas, HUD, UpgradeUI 等）
- **单例模式**: GameManager, WaveManager, UpgradeManager 通过 `.Instance` 访问
- **事件驱动**: GameManager 发出 OnLevelUp → UpgradeManager 监听 → 弹出三选一 UI
- **SpriteFactory 缓存**: 按颜色+尺寸缓存生成的 Sprite，避免重复创建

### 敌人类型 (EnemyType 枚举)
- Normal: 红色圆形，标准属性
- Fast: 绿色小圆，高速低血
- Tank: 紫色大圆，高血低速
- Boss: 橙色超大圆，每5波出现

### 升级效果 (UpgradeEffect 枚举)
AttackDamage, AttackSpeed, MoveSpeed, AttackRange, MultiShot,
PierceShot, MaxHP, HPRegen, ExpBonus, CritChance

## 开发规范
- 游戏内所有玩家可见文本使用**中文**
- Debug.Log 可以用英文或中文均可
- 新增脚本后需在 GameSetup.cs 中集成（自动创建 GameObject 和挂载组件）
- 不要创建需要手动在编辑器中拖拽赋值的公开字段，尽量代码赋值

## Unity 编辑器操作清单

> 每次从代码层面改动后，需要在 Unity 编辑器中执行的操作。
> GameSetup 会自动搭建大部分内容，手动操作极少。

### 首次设置（只做一次）
1. 打开 `Assets/Scenes/SampleScene.unity`
2. 在 Hierarchy 中右键 → Create Empty，重命名为 `_GameSetup`
3. 在 Inspector 中点 Add Component → 搜索 `GameSetup` → 添加
4. **按 Play 即可运行游戏**

### 如果修改了以下内容，需要额外操作
- 新增 Layer/Tag → 在 Edit > Project Settings > Tags and Layers 中添加
- 新增 Scene → 在 File > Build Settings 中添加到 Scenes In Build
- 修改物理碰撞矩阵 → Edit > Project Settings > Physics 2D

## 4-Phase 开发计划
- Phase 1 ✅ 核心骨架（路径、英雄移动、敌人生成、基础射击）
- Phase 2 ✅ 波次系统 + 升级三选一 + HUD
- Phase 3: 敌人多样化 + 难度曲线 + 游戏手感打磨
- Phase 4: 完整游戏循环 + 开始/结束界面 + 最终打磨
