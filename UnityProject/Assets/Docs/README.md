# 说明

## 软件环境

- Unity2018.4.10
- Android Studio 3.6.1

## 持续化存储

- AssetBundle
- database
	- 好友列表
- pic, sound, video, file

## 模仿原生页面机制

### LoadingView
btn -> CreatePanel -> loadingView -> 等待Tween的动画结束，发起初始化请求httpRequest, refreshList -> 超时处理 -> hide
SourceData脚本受Widget调度。

一级FadeIn由Widget控制。
二级FadeIn由Item控制。

### Toast

## Android

Utils.RequestUserPermission(); //统一管理动态授权

## iOS

### XCode配置

1. 添加依赖库：选中 IMDemo 的【Target】，在【General】面板中的 【Embedded Binaries】和【Linked Frameworks and Libraries】添加依赖库。
2. 需要在【Build Setting】-【Other Linker Flags】添加 -ObjC。 

## TODO:
- [ ] 1. 多个需要传入int type的地方，如果会话类型只有C2C，则去掉这个参数。
- [x] 2. 多参数或可变参数，使用json string传值。
- [x] 3. 消息解析统一到一个函数中，传入需要返回的消息号。
- [ ] 4. 安卓系统返回键，先收回键盘，再收回WidgetView。回收键盘后，OnKeyboardInvisible();
- [ ] 5. 安卓系统返回键，先收回MaterialUI的View，再收回WidgetView。
- [ ] 6. MaterialUI真机Canvas扩展算法。
- [ ] 7. 使用CanvasGroup隐藏被遮盖的WidgetPanel。
- [ ] 8. CP请求走自定义消息，服务器RESTAPI发起。
- [ ] 9. 内存管理和回收，测试脚本。DestroyObject。

## 测试用例

1. 手机验证码，是否有特殊手机号，无法注册？
