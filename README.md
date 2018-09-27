# KinectSkinner

## 配置步骤
* 将`Kinect v2 with MS-SDK`和`Skinner`导入同一个项目。删除`K2Examples/KinectDemos`文件夹。这里不需要演示场景。
* 打开`Assets/test/test`场景。在层次结构中禁用`Neo`游戏对象。
* 在Hierarchy中创建一个空的游戏对象并将其命名为`KinectController`。将`K2Examples/KinectScripts/KinectManager.cs`添加为此对象的组件。
* 选择`Neo（Skinner Source）`游戏对象。从动画制作器组件的控制器设置中删除`Mocaps`，以防止在场景开始时播放录制的模板动画。
* 选择`Assets/NeoLowMan/Neo`。在其`Rig-tab`上禁用`Optimize game objects`设置，并确保其装备为`Humanoid`。
* 将`K2Examples/KinectScripts/AvatarController.cs`添加到场景中的`Neo（Skinner Source）` 游戏对象，并启用其`Mirrored Movement`和`Vertical Movement`设置。确保对象的变换旋转为`（0,180,0）`。
* - [ ]  [可选]如果您想要阻止相机自己的动画移动，请禁用场景中`Camera Tracker`游戏物体的`Rotation`、`Distance`和`Shake` 的父游戏对象的脚本组件。
* :rocket:运行场景并开始在传感器前移动，以查看效果。
* 尝试通过更改场景中`Skinner Renderers`游戏物体的子物体，来体验其他`skinner`渲染器。
