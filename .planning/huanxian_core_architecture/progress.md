# 进度记录

## 2026-05-27

- 使用 `planning-with-files-zh` 技能建立持久化规划。
- 读取策划案 `Assets/ProjectDocs/Design_Concept_Overview.md`。
- 读取现有脚本 `Assets/Scripts/ThirdPersonController.cs` 和 `Assets/Scripts/BasicRigidBodyPush.cs`。
- 完成第三人称移动与唤仙核心机制的架构蓝图。
- 当前等待总制作人审核文件规划，尚未创建任何 C# 脚本实现。
- 总制作人审批通过架构蓝图与“孙悟空主唤”垂直切片方向。
- 创建 `Assets/Scripts/Input/PlayerInputFrame.cs`，定义每帧输入快照。
- 创建 `Assets/Scripts/Input/PlayerInputReader.cs`，捕获移动、视角、锁定、轻/重攻击、闪避、招架、Q 协同唤仙、E 降临唤仙。
- 创建 `Assets/Scripts/Movement/CharacterMotor.cs`，封装 `CharacterController.Move()`、水平移动、垂直速度、重力与传送。
- 运行 Unity batchmode 时被已打开的 Unity 项目实例拦截；随后检查 Editor 日志与 `Assembly-CSharp.dll` 更新时间，确认新增脚本已导入且未发现 C# 编译错误。
- 创建 `Assets/Scripts/Movement/PlayerLocomotionController.cs`，把 `PlayerInputReader.CurrentFrame.Move` 桥接到 `CharacterMotor`。
- 实现相机相对移动：读取 `Camera.main` forward/right，剔除 Y 轴后组合移动方向。
- 实现基础转向：输入方向有效时用 `Quaternion.RotateTowards` 平滑朝移动方向旋转。
- 通过 Unity MCP 刷新脚本、校验 `PlayerLocomotionController.cs`、抓取 Console，确认项目保持 0 编译错误。
- 创建 `Assets/Scripts/StateMachine/PlayerStateContext.cs`，定义 `EPlayerState: Idle, Move, Dodge, Parry, Attack, Invoke`。
- 创建 `Assets/Scripts/StateMachine/PlayerStateMachine.cs`，实现轻量状态切换与 Enter/Exit/Changed 事件。
- 创建 `Assets/Scripts/Combat/CombatResourceController.cs`，实现 Sanity 与 SummonGauge 的基础资源槽。
- 重构 `PlayerLocomotionController.cs`，让移动只在状态机 `CanMove` 时响应输入，非移动状态仍执行重力。
- 通过 Unity MCP 刷新编译、校验新增脚本并抓取 Console，确认项目保持 0 编译错误。
- 创建 `Assets/Scripts/StateMachine/DodgeState.cs`，实现闪避输入、Sanity 消耗、`TriggerDodge` 和自动回 Idle/Move。
- 创建 `Assets/Scripts/StateMachine/ParryState.cs`，实现招架输入、Sanity 消耗、`TriggerParry` 和自动回 Idle/Move。
- 创建 `Assets/Scripts/Invocation/InvocationController.cs`，监听 Q 协同与 E 主唤降临。
- 创建 `Assets/Scripts/Invocation/DescentController.cs`，预留孙悟空降临表现，当前通过 Debug.Log 输出“齐天大圣降临！切换动作模组”。
- 通过 Unity MCP 刷新编译、校验新增脚本并抓取 Console，确认项目保持 0 编译错误。
- 切换到资深 QA / 测试 Agent，对 `Playground` 当前活动场景执行自动化审计。
- 找到 CharacterController 主角对象 `PlayerArmature`，移除冲突的 `StarterAssets.ThirdPersonController`。
- 自动补齐 `PlayerInputReader`、`CharacterMotor`、`PlayerLocomotionController`、`PlayerStateMachine`、`CombatResourceController`、`InvocationController`、`DescentController`、`DodgeState`、`ParryState`。
- 创建运行时诊断脚本 `Assets/Scripts/Core/QA_DiagnosticBootstrapper.cs`，并在场景中创建 `QA_DiagnosticBootstrapper` 对象挂载该组件。
- QA 面板显示玩家状态、Sanity、SummonGauge，并提供按钮与 E 键热键执行“充能 100 并触发降临”。
- 通过 Unity MCP 强制刷新编译、校验脚本、抓取 Console，确认项目保持 0 编译错误。
- 第五阶段创建 `Assets/Scripts/Movement/PlayerAnimationController.cs`，接管 Animator 参数桥接。
- `PlayerAnimationController` 自动缓存 `Animator`、`CharacterMotor`、`PlayerStateMachine`，每帧将水平速度写入 `Speed`，将接地状态写入 `Grounded`，并在存在 `FreeFallSpeed` 参数时写入垂直速度。
- 当状态进入 `EPlayerState.Invoke` 时，脚本会将子 Renderer 材质颜色设为金色，并在 Animator 存在 `TriggerTransform` 参数时触发该 Trigger。
- 通过 Unity MCP 将 `PlayerAnimationController` 挂载到 `PlayerArmature`，检测到 `Armature_Mesh` 为 `SkinnedMeshRenderer`，共有 3 个材质。
- Unity MCP 编译刷新与 Console 检查通过，项目保持 0 编译错误。
- 重构 `PlayerInputReader.cs`：Space 改为跳跃，Shift 使用 0.25 秒阈值区分点按闪避和长按奔跑，LeftControl/C 触发下蹲输入，F 临时作为招架输入以避免与 Shift 冲突。
- 扩展 `PlayerInputFrame.cs`：新增 `JumpPressed`、`SprintHeld`、`CrouchPressed`。
- 重构 `PlayerLocomotionController.cs`：根据 `SprintHeld` 应用奔跑速度倍率，并在 Space + Grounded + CanMove 时给 `CharacterMotor` 写入跳跃垂直速度。
- 重构 `PlayerAnimationController.cs`：对齐官方 `StarterAssetsThirdPerson` Animator 参数，写入 `Speed`、`MotionSpeed`、`Grounded`、`Jump`、`FreeFall`。
- 修复 `DodgeState.cs` 和 `ParryState.cs`：只有 Animator 中真实存在对应 Trigger 参数时才调用 `SetTrigger`，避免官方 Animator 缺少 `TriggerParry` 时刷红。
- 清空 Console 后通过 Unity MCP 强制刷新编译、校验脚本并抓取 Console，确认本轮重构后 Console Error 为 0。
- 修复 `PlayerLocomotionController.cs` 相机相对移动方向：获取 `Camera.main.transform.forward/right` 后显式 `y = 0f`，再归一化并组合移动方向。
- 添加相机方向近零保护：相机投影方向异常时回退到角色自身 `transform.forward/right` 的地面投影。
- 确认移动脚本只旋转角色 `transform.rotation`，不修改 Camera 自身 Rotation。
- 通过 Unity MCP 清空 Console、强制刷新编译并校验脚本，确认转圈修复后 Console Error 为 0。
- 进一步定位到 `PlayerCameraRoot` 是 `PlayerArmature` 子物体。旧官方控制器移除后，CameraRoot 不再独立写入世界旋转，导致玩家转身会带动 Cinemachine Follow 目标旋转，形成相机-玩家正反馈。
- 新增 `Assets/Scripts/Movement/PlayerCameraRootController.cs`：LateUpdate 中根据 `PlayerInputReader.CurrentFrame.Look` 独立控制 `PlayerCameraRoot` 的世界旋转，解耦玩家朝向与相机目标朝向。
- 重构 `PlayerLocomotionController.cs`：同一帧只调用一次 `CharacterController.Move`，将水平速度和垂直速度合并到 `CharacterMotor.MoveWithVerticalVelocity`，减少运动采样和碰撞不稳定。
- 通过 Unity MCP 挂载 `PlayerCameraRootController` 到 `PlayerArmature`，确认 `PlayerCameraRoot` 存在，并再次刷新编译、抓取 Console，Console Error 为 0。
- 新增 `Assets/Scripts/Movement/CrouchController.cs` 并挂载到 `PlayerArmature`。LeftControl/C 会切换下蹲，运行时平滑降低 `CharacterController.height` 和 `center`。
- 重构 `DodgeState.cs`：Shift 点按进入 Dodge 后会按相机相对输入方向执行短距离真实位移，若没有方向输入则向角色前方闪避。
- 重构 `PlayerLocomotionController.cs`：下蹲时应用 `crouchMultiplier` 降低移动速度。
- 调低 `PlayerCameraRootController` 灵敏度：mouseSensitivity=0.12，gamepadSensitivity=45，并同步写入当前 `PlayerArmature` 场景组件。
- Unity MCP 刷新编译和 Console 检查通过，Console Error 为 0。

## Human Attack / Summon Gauge Prototype

- Added `Assets/Scripts/Combat/DamageReceiver.cs` for prototype enemy or dummy hit targets.
- Added `Assets/Scripts/Combat/HumanAttackController.cs` and mounted it on `PlayerArmature`.
- Light attack: left mouse, costs 6 Sanity, deals 12 damage, grants 8 SummonGauge on hit.
- Heavy attack: right mouse, costs 14 Sanity, deals 28 damage, grants 18 SummonGauge on hit.
- Hit detection uses an OverlapSphere in front of the player and only grants SummonGauge when a `DamageReceiver` is hit.
- Created `QA_CombatDummy` in the scene with `DamageReceiver` for immediate testing.
- Animator fallback tries `Attack_Light` / `Attack_Heavy` if those states exist; current official controller lacks attack clips, so no animation error is thrown.
- Unity MCP refresh, validation, and Console check passed with 0 errors.

## Mixamo Animation Integration Phase 1

- Read `Assets/Character/Animations/Mixamo/README.md` and used its intent-to-state mapping as the animation integration source of truth.
- Updated `Assets/Character/Animations/StarterAssetsThirdPerson.controller` through Unity MCP.
- Added Animator states `Attack_Light`, `Attack_Heavy`, `Dodge_Left`, `Dodge_Right`, `Dodge_Backward`, `Parry_Block`, and `Invoke_Transform`.
- Bound those states to Mixamo clips `Slash Advance`, `Smash`, `Standing Dodge Left`, `Standing Dodge Right`, `Standing Dodge Backward`, `Blocking`, and `Sword And Shield Power Up`.
- Added Animator triggers `TriggerParry` and `TriggerTransform`.
- Added Any State transitions for `TriggerParry -> Parry_Block` and `TriggerTransform -> Invoke_Transform`.
- Added exit-time return transitions from one-shot combat states back to `Idle Walk Run Blend`.
- Verified via Unity MCP that all required states have motions and both triggers exist; Console check returned 0 entries.
- Fixed first-play Mixamo retargeting issue where attack/dodge poses could pull the humanoid below the ground by baking Root Rotation, Root Position Y, and Root Position XZ into pose for all staged Mixamo clips.
- Tuned combat animation playback speeds and prototype action windows so light attack, heavy attack, dodge, parry, and invoke poses are visible in the current controller.
- Restored parry input to `F` in `PlayerArmature` scene/prefab settings to avoid conflicting with Shift sprint/dodge.
- Fixed follow-up combat feel: `PlayerInputReader` now executes before state components, Dodge locks the chosen animation state when entering, and Parry can cancel from the prototype human attack state.
- Started the Black Myth: Wukong-inspired combat-feel pass: Shift dodge now fires on key-down instead of key-up, attack can be dodge-canceled, light/heavy attacks gain a short forward lunge, attacks rotate toward input direction on start, and light/heavy clicks can buffer a follow-up attack during the chain window.
- Continued the combat-feel pass: tuned `PlayerFollowCamera` to FOV 48, stronger 3rd-person follow damping, slightly wider shoulder/arm framing, and no procedural noise; attacks now retain low-speed input drift after the opening lunge so the player is not hard-rooted during attack animations.
