# 《唤仙》QA 自动化诊断报告

日期：2026-05-27
角色：资深游戏 QA / 测试 Agent
场景：`Assets/Scenes/Playground.unity`

## 结论

当前活动场景已完成第三人称主角组件合规性修复，Unity Console 编译错误为 0。

## 场景审计

- 找到 CharacterController 主角对象：`PlayerArmature`
- 检测到旧模板控制器：`StarterAssets.ThirdPersonController`
- 处理结果：已安全移除 `StarterAssets.ThirdPersonController`
- 保留组件：`Animator`、`CharacterController`、`BasicRigidBodyPush`、`StarterAssetsInputs`、`PlayerInput`

## 组件注入结果

已在 `PlayerArmature` 上确认或挂载：

- `HuanXian.Input.PlayerInputReader`
- `HuanXian.Movement.CharacterMotor`
- `HuanXian.Movement.PlayerLocomotionController`
- `HuanXian.StateMachine.PlayerStateMachine`
- `HuanXian.Combat.CombatResourceController`
- `HuanXian.Invocation.InvocationController`
- `HuanXian.Invocation.DescentController`
- `HuanXian.StateMachine.DodgeState`
- `HuanXian.StateMachine.ParryState`

已创建场景 QA 对象：

- `QA_DiagnosticBootstrapper`
- 挂载组件：`HuanXian.Core.QA_DiagnosticBootstrapper`

## 修复统计

- 移除冲突组件：1
- 挂载运行时组件：10
- 创建 QA GameObject：1
- Unity Console errors：0

## QA 面板验证方式

1. 打开 `Playground` 场景。
2. 点击 Play。
3. 屏幕左上角应出现 `HuanXian QA Diagnostics` 面板。
4. 面板会显示：
   - 当前玩家状态：`Idle`、`Move`、`Invoke` 等。
   - 当前神识值：`Sanity`。
   - 当前唤仙槽：`SummonGauge`。
5. 按 `E`，或点击面板按钮 `Charge 100 and Invoke`。
6. 预期结果：
   - `SummonGauge` 被充到 100。
   - 调用 `InvocationController.TryEnterDescent()`。
   - 玩家状态切换到 `Invoke`。
   - Console 输出：`齐天大圣降临！切换动作模组`。
   - 预览持续时间结束后自动回到 `Idle` 或 `Move`。

## 注意事项

- 当前 QA 面板是运行时诊断工具，后续正式版本应移除或用开发者开关控制。
- `QA_DiagnosticBootstrapper` 内部使用 `FindObjectOfType` 作为兜底定位，仅在引用缺失时触发；当前 MCP 静态检查提示性能 warning，但不影响编译和测试。
- 当前角色 A-Pose/T-Pose 的根因仍可能包括 Animator Controller、Avatar、动画状态机或模型绑定缺失。本次修复解决的是控制权冲突、组件缺失和 E 键无反馈的诊断通路。

## 第五阶段动画桥接补充

- 已创建并挂载 `HuanXian.Movement.PlayerAnimationController` 到 `PlayerArmature`。
- 当前 `PlayerArmature` 的 Animator 参数中检测到：
  - `Speed`：Float
  - `Grounded`：Bool
-  - `Jump`：Bool
  - `FreeFall`：Bool
  - `MotionSpeed`：Float
- 当前未检测到 `TriggerTransform`，脚本会自动跳过缺失 Transform Trigger，避免运行时报错。
- 变身反馈通过 `Armature_Mesh` 的 3 个材质实例改为 `Color.yellow` 进行肉眼确认。

## 输入与动画重构补充

- Space：跳跃。
- Shift 点按：释放时按下时间小于 0.25 秒，触发闪避。
- Shift 长按：持续按下超过 0.25 秒，进入奔跑输入模式。
- LeftControl 或 C：触发下蹲输入。
- F：临时招架输入，避免与 Shift 微操冲突。
- 官方 Animator 映射：
  - `Speed`：写入水平物理速度乘以走/跑动画系数。
  - `MotionSpeed`：有移动输入时为 1，否则为 0。
  - `Grounded`：写入 `CharacterMotor.IsGrounded`。
  - `Jump`：Space 且接地时写入 true。
  - `FreeFall`：不接地且垂直速度为负时写入 true。
- 已修复旧红字来源：`TriggerParry`/`TriggerDodge` 不存在时不再调用 `Animator.SetTrigger`。
- 本轮清空 Console 后重新编译检查，Console Error 为 0。

## 相机相对移动转圈修复

- 已重构 `PlayerLocomotionController.GetCameraRelativeMoveDirection`。
- `Camera.main.transform.forward` 和 `right` 获取后会立即执行：
  - `camForward.y = 0f`
  - `camRight.y = 0f`
  - `Normalize()`
- 最终 `moveDirection` 也会再次 `y = 0f`，确保移动方向完全贴地。
- 当没有移动输入时，`GetCameraRelativeMoveDirection` 直接返回 `Vector3.zero`，角色不会无端旋转。
- 当前脚本不修改 Camera 的 Rotation；只在有移动方向时旋转玩家自身 `transform.rotation`。
- Unity MCP 清空 Console 后刷新编译检查，Console Error 为 0。

## 相机反馈循环深层修复

- 根因追加定位：`PlayerCameraRoot` 是 `PlayerArmature` 的子物体，玩家转身会让 CameraRoot 继承旋转。
- 旧官方 `ThirdPersonController` 原本负责独立写 CameraRoot 旋转；移除后该职责缺失。
- 已新增并挂载 `PlayerCameraRootController` 到 `PlayerArmature`。
- `PlayerCameraRootController` 在 `LateUpdate` 中根据 Look 输入直接写 `PlayerCameraRoot.rotation` 的世界旋转，抵消父物体旋转继承造成的反馈。
- 已把 `PlayerLocomotionController` 中的水平 Move 与垂直 Move 合并为单次 `CharacterMotor.MoveWithVerticalVelocity`，减少角色控制器运动抖动。
- Unity MCP 编译与 Console 检查：Error 为 0。

## 闪避、下蹲与视角灵敏度补充

- 已新增并挂载 `CrouchController` 到 `PlayerArmature`。
- LeftControl 或 C：切换下蹲状态。
- 下蹲反馈：`CharacterController.height` 平滑降低到 1.05，`center.y` 平滑降低到 0.55。
- 下蹲移动：`PlayerLocomotionController` 使用 `crouchMultiplier=0.45` 降低移动速度。
- Shift 点按：进入 `DodgeState` 后执行真实短距离冲刺，默认 `duration=0.28`、`dodgeSpeed=8`。
- 闪避方向：优先使用相机相对移动输入方向；没有移动输入时，使用角色当前前方。
- 视角灵敏度已调低：`mouseSensitivity=0.12`，`gamepadSensitivity=45`。
- Unity MCP 编译与 Console 检查：Error 为 0。

## Human Attack / Summon Gauge QA

- `HumanAttackController` is mounted on `PlayerArmature`.
- `DamageReceiver` is available for enemies and test dummies.
- `QA_CombatDummy` was created near `(0, 1, 3)` for hit testing.
- Left mouse performs light attack: Sanity -6, damage 12, SummonGauge +8 on hit.
- Right mouse performs heavy attack: Sanity -14, damage 28, SummonGauge +18 on hit.
- Verification: enter Play mode, face `QA_CombatDummy`, click left/right mouse, and watch `SummonGauge` increase in the QA panel.
- Current official Animator has no attack clips. The controller will automatically use `Attack_Light` / `Attack_Heavy` later if those states are added, without code changes.
- Unity Console errors: 0.
