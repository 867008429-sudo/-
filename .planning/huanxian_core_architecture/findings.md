# 架构调研发现

## 策划案提炼

- 游戏核心是第三人称动作冒险，基础操作包括移动、锁定、轻攻击、重攻击、闪避、招架、处决、仙术协同和唤仙降临。
- 唤仙机制分为神仙绑定、协同技能、降临、共鸣成长。
- 神识值用于协同技能，唤仙槽用于短时降临。
- 唤仙槽应通过连续命中、完美闪避、精准招架、击败精英、打断 Boss 大招、处决等“高质量战斗行为”积累。
- 降临状态持续 12 至 18 秒，期间切换动作模组、攻击范围、抗打断能力和技能组合。
- 首个垂直切片建议围绕孙悟空：如意追云、分身乱打、短连段、终式。

## 现有代码发现

- `Assets/Scripts/ThirdPersonController.cs` 来自 Starter Assets 风格，职责较集中：输入读取、相机旋转、移动、跳跃重力、Animator 参数都在同一脚本。
- 该脚本依赖 `CharacterController`、`StarterAssetsInputs`、可选 `PlayerInput`、`Animator` 和主相机。
- 现有移动适合作为早期参考，但不适合继续承载战斗、唤仙、状态机、锁定等复杂系统。
- `BasicRigidBodyPush.cs` 独立处理 CharacterController 碰撞推动刚体，可保留为物理交互辅助。

## 架构判断

- 移动核心应拆为输入帧、状态机、运动控制、CharacterController 执行、Animator 桥接。
- 唤仙核心应拆为资源模型、神仙配置、协同技能执行器、降临控制器、表现层 Presenter。
- JSON 数据不应直接引用 Prefab/AnimatorClip，而应使用 key，再由表现服务映射 Unity 资源。
- 首版应优先实现“可玩闭环”：移动、闪避/攻击事件、神识值、唤仙槽、孙悟空协同、孙悟空降临。

## 第一阶段实现发现

- 项目启用了 `com.unity.inputsystem`，但当前未在 Assets 下发现专用 `.inputactions` 文件。
- `PlayerInputReader` 因此采用“默认键鼠轮询 + PlayerInput SendMessage 回调”的双入口方案，后续接入动作资产时可保留同一公开接口。
- Unity batchmode 被当前已打开的项目实例拦截，日志显示 `HandleProjectAlreadyOpenInAnotherInstance`，不是脚本编译错误。
- 当前打开的 Unity 实例已导入新增脚本并刷新 `Library/ScriptAssemblies/Assembly-CSharp.dll`，Editor 日志未发现新增脚本相关 `error CS`。

## 第二阶段实现发现

- `PlayerLocomotionController` 只桥接输入与马达，不直接读键盘，也不直接处理 Animator。
- 相机相对移动以 `Camera.main` 为默认相机源，并允许 Inspector 手动指定 `movementCamera`。
- `Quaternion.RotateTowards` 比直接设置 forward 更适合作为第一版平滑转向，后续可由动画根运动或锁定系统接管。
- Unity MCP `validate_script` 对 `PlayerLocomotionController` 报出一条 line 0 GC warning，但脚本内无字符串拼接，判断为静态规则误报；Console error 为 0。

## 第三阶段实现发现

- 状态上下文使用 `PlayerStateContext` 持久保存 `CurrentState` 与 `PreviousState`，方便 Inspector 观察。
- `PlayerStateMachine` 采用事件式 Enter/Exit/Changed，后续 Animator、音效、VFX 或战斗系统可订阅状态变化，避免硬耦合。
- 移动控制器只读取 `PlayerStateMachine.CanMove`，不关心 Dodge/Attack/Invoke 的具体实现细节。
- `CombatResourceController` 先提供最小资源 API：`ModifySanity`、`ModifySummonGauge`、`TrySpendSanity`、`ResetSummonGauge`。后续攻击命中与完美招架只需调用资源入口。
- Unity MCP 校验新增状态机与资源槽均为 0 errors；Console error 保持 0。

## 第四阶段实现发现

- `DodgeState` 与 `ParryState` 作为独立组件接入状态机，当前以固定 duration 自动回到 Idle/Move；后续可由动画事件替代计时器。
- 闪避默认消耗 15 Sanity，招架默认消耗 10 Sanity，均通过 `CombatResourceController.TrySpendSanity` 统一扣除。
- 攻防状态使用 Animator Trigger：`TriggerDodge`、`TriggerParry`。Animator 缺失时不会报错，只执行状态与资源逻辑。
- `InvocationController` 负责监听 Q/E，`DescentController` 负责降临表现与状态恢复，职责边界清晰。
- E 键降临完整链路为：输入 E -> 检查 SummonGauge -> 扣槽 -> ForceState(Invoke) -> BeginDescent() -> Debug.Log 大圣降临 -> duration 后 ReturnToLocomotion。
- Unity MCP 校验新增攻防与唤仙脚本均为 0 errors；Console error 保持 0。

## 动画接入阶段 1 发现

- Mixamo FBX 已以 Humanoid 动画类型导入，且每个文件都暴露一个语义清晰的主 `AnimationClip`。
- `HumanAttackController` 已经通过 `Animator.CrossFade` 查找 `Attack_Light` 与 `Attack_Heavy`，因此只需在 Controller 中补同名状态即可接通轻/重攻击动画。
- `PlayerAnimationController` 已经在 Dodge 状态切入时优先 CrossFade 到 `Dodge_Left`、`Dodge_Right`、`Dodge_Backward`，因此闪避动画接入不需要额外 Trigger。
- `ParryState` 与降临链路依赖 Animator Trigger，给 Controller 增加 `TriggerParry`、`TriggerTransform` 后，运行时缓存会在 Play 时识别并触发对应状态。
- 当前接入仍是第一版“一次性动作状态 + 退出时间返回移动 Blend Tree”，后续阶段可改为动画事件驱动命中帧、取消窗口、无敌帧和降临动作模组。
