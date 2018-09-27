# KinectSkinner

## 配置步骤

* 将来自Unity资产商店的K2资产导入同一个项目。删除K2Examples / KinectDemos文件夹。这里不需要演示场景。
* 打开资产/测试/测试场景。在层次结构中禁用新游戏对象。这不是真的需要。
* 在Hierarchy中创建一个空的游戏对象并将其命名为KinectController，以与其他演示场景保持一致。将K2Examples / KinectScripts / KinectManager.cs添加为此对象的组件。所有其他与Kinect相关的组件都需要KinectManager组件。
* 选择“Neo（Skinner Source）” - 层次结构中的游戏对象。从动画制作器组件的控制器设置中删除“Mocaps”，以防止在场景开始时播放录制的模板动画。
* 按对象名称下方的“选择”，在项目中查找模型的资产。在其Rig-tab上禁用“优化游戏对象”设置，并确保其装备为Humanoid。否则，AvatarController将找不到它需要控制的模型的关节。
* 将K2Examples / KinectScripts / AvatarController-component添加到'Neo（Skinner Source）' - 场景中的游戏对象，并启用其“镜像移动”和“垂直移动”设置。确保对象的变换旋转为（0,180,0）。
* （可选）如果您想要阻止相机自己的动画移动，请禁用场景中主摄像机的“摄像机跟踪器”，“旋转”，“距离”和“摇动” - 父游戏对象的脚本组件。
* 运行场景并开始在传感器前移动，以查看效果。尝试其他skinner渲染器。他们是场景中“Skinner Renderers”游戏对象的孩子。
