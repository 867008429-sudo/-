# 《唤仙》第三人称移动与唤仙核心机制架构规划

状态：implementation
创建日期：2026-05-27
当前阶段：动画接入阶段 1 - Mixamo 战斗动作已接入 Animator

## 目标

为《唤仙》的“第三人称移动”和“唤仙核心机制”规划一套高内聚、低耦合、数据驱动的 Unity C# 架构。当前只输出结构规划、文件清单、调用关系与 Unity/JSON 交互方案，不写具体 C# 实现。

## 设计原则

- 输入、运动、战斗状态、唤仙资源、动画、特效、数据加载彼此解耦。
- 运行时逻辑依赖接口和数据模型，具体 Unity 组件通过 Adapter/Bridge 接入。
- 策划数据使用 JSON 描述神仙、技能、资源消耗、降临参数、动画键名和特效键名。
- Animator、CharacterController、Cinemachine 等 Unity 组件只出现在表现层或执行层，避免污染核心规则层。
- 现有 `Assets/Scripts/ThirdPersonController.cs` 作为移动原型参考，后续不建议继续把新逻辑塞进该脚本。

## 阶段计划

### 阶段 1：需求提炼与模块边界

状态：complete

- 阅读 `Assets/ProjectDocs/Design_Concept_Overview.md`。
- 阅读现有 `Assets/Scripts/ThirdPersonController.cs` 和 `BasicRigidBodyPush.cs`。
- 提炼第三人称移动、神识值、唤仙槽、协同技能、降临、主唤/辅唤等系统需求。

### 阶段 2：脚本文件清单规划

状态：complete

- 按 Movement、Combat State、Invocation、Data、Animation/Presentation、Utilities 分层。
- 输出计划创建的 C# 脚本文件、职责和依赖方向。

### 阶段 3：调用关系与 Unity 组件交互设计

状态：complete

- 规划 CharacterController 如何由运动执行层使用。
- 规划 Animator 如何由动画桥接层使用。
- 规划 JSON 数据如何被加载、缓存、转换为运行时定义。

### 阶段 4：等待审核

状态：complete

- 等待总制作人审核文件规划。
- 审核通过后再进入 C# 脚本创建与最小原型实现。

### 阶段 5：输入与基础运动执行层

状态：complete

- 创建 `Assets/Scripts/Input/PlayerInputFrame.cs`。
- 创建 `Assets/Scripts/Input/PlayerInputReader.cs`。
- 创建 `Assets/Scripts/Movement/CharacterMotor.cs`。
- 使用 Unity Editor 日志与 `Library/ScriptAssemblies/Assembly-CSharp.dll` 更新时间确认新增脚本已被 Unity 导入并触发编译。
- 注意：独立 batchmode 编译因项目已在另一个 Unity 实例打开而被 Unity 拦截，不是 C# 编译错误。

### 阶段 6：角色相机相对移动桥接

状态：complete

- 创建 `Assets/Scripts/Movement/PlayerLocomotionController.cs`。
- 自动获取同物体上的 `PlayerInputReader` 与 `CharacterMotor`。
- 从 `Camera.main` 获取相机 forward/right，剔除 Y 轴并归一化，计算相机相对移动方向。
- 输入有效时使用 `Quaternion.RotateTowards` 平滑转向移动目标方向。
- 每帧调用 `CharacterMotor.ApplyGravity()`，并通过 `MoveHorizontal()` 与垂直位移执行移动。
- 使用 Unity MCP 刷新编译、校验脚本并抓取 Console，确认 0 编译错误。

### 阶段 7：核心状态机与战斗资源槽

状态：complete

- 创建 `Assets/Scripts/StateMachine/PlayerStateContext.cs`，定义 `EPlayerState` 与状态上下文。
- 创建 `Assets/Scripts/StateMachine/PlayerStateMachine.cs`，提供轻量状态切换、Enter/Exit/Changed 事件。
- 创建 `Assets/Scripts/Combat/CombatResourceController.cs`，管理 Sanity 与 SummonGauge。
- 重构 `PlayerLocomotionController.cs`，只有 `Idle` 或 `Move` 状态允许处理水平移动输入。
- 使用 Unity MCP 刷新编译、校验脚本并抓取 Console，确认 0 编译错误。

### 阶段 8：攻防状态与主唤/辅唤框架

状态：complete

- 创建 `Assets/Scripts/StateMachine/DodgeState.cs`，处理闪避输入、神识消耗、Animator Trigger 与自动回到移动状态。
- 创建 `Assets/Scripts/StateMachine/ParryState.cs`，处理招架输入、神识消耗、Animator Trigger 与自动回到移动状态。
- 创建 `Assets/Scripts/Invocation/InvocationController.cs`，监听 Q 协同与 E 降临输入。
- 创建 `Assets/Scripts/Invocation/DescentController.cs`，预留孙悟空降临通路并输出“齐天大圣降临！切换动作模组”。
- E 键主唤降临检查 SummonGauge >= 100，扣除 100 后强制进入 `EPlayerState.Invoke`。
- 使用 Unity MCP 刷新编译、校验脚本并抓取 Console，确认 0 编译错误。

### 动画接入阶段 1：Mixamo 战斗动作接入

状态：complete

- 阅读 `Assets/Character/Animations/Mixamo/README.md` 中的动作意图映射。
- 将 `Attack_Light` 绑定到 `X Bot@Slash Advance.fbx` 的 `Slash Advance` Clip。
- 将 `Attack_Heavy` 绑定到 `X Bot@Smash.fbx` 的 `Smash` Clip。
- 将 `Dodge_Left`、`Dodge_Right`、`Dodge_Backward` 分别绑定到对应 Mixamo 闪避 Clip。
- 新增 `Parry_Block` 状态，绑定 `X Bot@Blocking.fbx` 的 `Blocking` Clip，并通过 `TriggerParry` 从 Any State 进入。
- 新增 `Invoke_Transform` 状态，绑定 `X Bot@Sword And Shield Power Up.fbx` 的 `Sword And Shield Power Up` Clip，并通过 `TriggerTransform` 从 Any State 进入。
- 为一次性攻击、闪避、招架、降临动作添加回到 `Idle Walk Run Blend` 的退出转场。
- 使用 Unity MCP 反查 Animator 状态、Motion 与 Trigger，确认全部命中；Console 保持 0 error / 0 warning。

## 计划创建的目录结构

```text
Assets/Scripts/
  Core/
    HuanXianBehaviour.cs
    ServiceLocator.cs
  Input/
    PlayerInputReader.cs
    PlayerInputFrame.cs
  Movement/
    CharacterMotor.cs
    CharacterMovementConfig.cs
    GroundProbe.cs
    CameraRelativeMovement.cs
    PlayerLocomotionController.cs
  StateMachine/
    IState.cs
    StateMachine.cs
    PlayerStateContext.cs
    PlayerIdleState.cs
    PlayerMoveState.cs
    PlayerDodgeState.cs
    PlayerAttackState.cs
    PlayerParryState.cs
    PlayerInvocationState.cs
  Combat/
    CombatController.cs
    CombatResourceController.cs
    HitEvent.cs
    CombatEventBus.cs
    TargetLockController.cs
  Invocation/
    InvocationController.cs
    InvocationLoadout.cs
    InvocationResourceModel.cs
    DeityRuntimeInstance.cs
    DeitySkillExecutor.cs
    DescentController.cs
    AssistSkillController.cs
  Data/
    JsonDataRepository.cs
    DeityDefinition.cs
    DeitySkillDefinition.cs
    DescentDefinition.cs
    MovementDefinition.cs
    GameDataIds.cs
  Animation/
    PlayerAnimatorBridge.cs
    AnimatorParameterIds.cs
    AnimationEventRelay.cs
  Presentation/
    VfxSpawnService.cs
    AudioCueService.cs
    DeityAvatarPresenter.cs
  Interaction/
    DivineSenseInteractor.cs
    ExplorationAbilityGate.cs
```

## 依赖方向

```text
Input -> PlayerStateContext
Data -> Invocation/Movement/Combat
StateMachine -> Movement + Combat + Invocation
Movement -> CharacterController
Combat -> Invocation resource events
Invocation -> Data + Combat + Animation/Presentation
Animation/Presentation -> Animator/VFX/Audio
Unity Scene Components -> Controllers
```

核心规则层不直接生成特效、不直接操作 Animator Controller 状态、不直接读键盘手柄输入。

## 关键调用链

### 普通移动

```text
PlayerInputReader
  -> PlayerInputFrame
  -> PlayerStateContext
  -> StateMachine(PlayerIdleState / PlayerMoveState)
  -> PlayerLocomotionController
  -> CharacterMotor
  -> CharacterController.Move()
  -> PlayerAnimatorBridge.SetLocomotion()
```

### 闪避/招架/攻击积累神识与唤仙槽

```text
PlayerInputReader
  -> StateMachine(PlayerDodgeState / PlayerParryState / PlayerAttackState)
  -> CombatController
  -> CombatEventBus.Publish(HitEvent / PerfectDodge / PerfectParry)
  -> CombatResourceController.AddMindPower()
  -> InvocationController.AddInvocationGauge()
  -> PlayerAnimatorBridge.SetCombatFlags()
```

### 协同技能

```text
PlayerInputReader.AssistSkill
  -> InvocationController.TryCastAssistSkill(slot)
  -> JsonDataRepository.GetSkillDefinition(skillId)
  -> DeitySkillExecutor.Execute(definition, casterContext, targetContext)
  -> CombatController.ApplySkillPayload()
  -> VfxSpawnService.Spawn(skill.vfxKey)
  -> PlayerAnimatorBridge.Trigger(skill.animationTrigger)
```

### 降临

```text
PlayerInputReader.Descent
  -> InvocationController.TryEnterDescent()
  -> DescentController.Begin(activeDeityDefinition)
  -> PlayerStateMachine.Enter(PlayerInvocationState)
  -> PlayerAnimatorBridge.SetAnimatorOverrideOrLayer(deity.animationProfile)
  -> CharacterMotor.ApplyMovementModifier(descent.movementModifier)
  -> CombatController.ApplyCombatProfile(descent.combatProfile)
  -> DeityAvatarPresenter.ShowAvatar(deity.avatarVfxKey)
```

### 降临结束

```text
DescentController.Tick(deltaTime)
  -> duration reached or finisher consumed
  -> CombatController.RestoreBaseProfile()
  -> CharacterMotor.ClearMovementModifier()
  -> PlayerAnimatorBridge.RestoreBaseAnimationProfile()
  -> DeityAvatarPresenter.HideAvatar()
  -> StateMachine.ReturnToLocomotion()
```

## JSON 数据交互规划

建议数据路径：

```text
Assets/StreamingAssets/GameData/
  deities.json
  deity_skills.json
  descents.json
  movement_profiles.json
```

JSON 只存配置，不存 Unity 对象引用。Unity 资源通过 key 间接映射：

- `animationTrigger`
- `animatorLayer`
- `animatorOverrideKey`
- `vfxKey`
- `audioCueKey`
- `skillId`
- `deityId`
- `movementProfileId`

加载流程：

```text
JsonDataRepository.LoadAll()
  -> 解析 JSON DTO
  -> 校验 id 唯一性、引用完整性、数值范围
  -> 构建 Dictionary<string, Definition>
  -> Controllers 通过 id 查询 Definition
```

## Unity 组件交互边界

- `CharacterController`：只由 `CharacterMotor` 调用 `Move()`，其他系统只能请求移动意图或修改移动参数。
- `Animator`：只由 `PlayerAnimatorBridge` 写入参数、Trigger、Layer Weight 或 Override Controller。
- `Cinemachine`：移动系统只读取相机朝向，后续镜头控制可独立为 Camera 模块。
- `ParticleSystem/VisualEffect`：只由 `VfxSpawnService` 根据 key 实例化。
- `AudioSource`：只由 `AudioCueService` 根据 key 播放。
- `MonoBehaviour Update`：集中在少数 Controller 中，数据模型类不继承 MonoBehaviour。

## 审核关注点

- 是否接受“现有 ThirdPersonController 逐步迁移，而不是继续扩写”的路线。
- 是否确认首个垂直切片以孙悟空为主唤神仙。
- JSON 是否作为首版策划数据格式，后续可再升级为 ScriptableObject 或 Addressables。
- 降临是否采用 Animator Override/Layer 切换作为首版动作模组方案。
