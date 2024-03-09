

<h1 style="display:inline; vertical-align:middle; margin-left: 30px;">Musical Moments - 音乐时刻</h1>
    
<img src="https://github.com/TheD0ubleC/MusicalMoments/assets/143760576/7f630f9e-de47-45d8-a861-112ddf8b015d" width="120" style="vertical-align:middle;">

## 介绍
**Musical Moments (MM)** 是一款完全使用.NET6.0开发的实时音频工具 不像传统的实时音频播放工具一样闭源、收费和功能少(说的就是你SoundPad)

因为发现市面上还没有一款免费开源好用的实时音频工具 所以我决定开发一款如同我描述的工具(sh*t [SLAM](https://slam.flankers.net/) 你们知道我有多难吗 这家伙[七年](https://github.com/SilentSys/SLAM)没更新了?!? 还是我硬给他[汉化](https://www.bilibili.com/video/BV1tK411i7S3)一下和搭建[音频整合站](slam.scmd.cc)才勉强在国内续命 关于他的视频都发了3个了 然后群里巴拉巴拉吵着说CS2怎么还不能用 再加上我早就加了CS2支持 但懒狗valve控制台坏了都不知道修 然后就真没办法咯 硬肝出来这个 还因为哔哩哔哩被封了30天视频都发不了没热度呜呜呜呜)

## 快速跳转

 - 如果你已经有了.NET环境请跳至[第二步](https://github.com/TheD0ubleC/MusicalMoments/tree/main?tab=readme-ov-file#%E7%AC%AC%E4%BA%8C%E6%AD%A5%E9%85%8D%E7%BD%AEmmmusical-moments)
 - 如果你已经有了.NET环境和VB声卡请跳至[绑定音频设备与按键](https://github.com/TheD0ubleC/MusicalMoments/tree/main?tab=readme-ov-file#%E7%BB%91%E5%AE%9A%E9%9F%B3%E9%A2%91%E8%AE%BE%E5%A4%87%E4%B8%8E%E6%8C%89%E9%94%AE)或[游戏/语音工具内设置](https://github.com/TheD0ubleC/MusicalMoments/tree/main?tab=readme-ov-file#%E6%B8%B8%E6%88%8F%E8%AF%AD%E9%9F%B3%E5%B7%A5%E5%85%B7%E5%86%85%E9%85%8D%E7%BD%AE)
## 第一步:安装.NET运行时</h2>
## [点击此处下载.NET运行时](https://download.visualstudio.microsoft.com/download/pr/e030e884-446c-4530-b37b-9cda7ee93e4a/403c115daa64ad3fcf6d8a8b170f86b8/dotnet-sdk-6.0.127-win-x64.exe) 

- 下载完成后，双击打开安装程序并按照指示完成安装。

![image](https://github.com/TheD0ubleC/MusicalMoments/assets/143760576/667f76a7-776b-4e09-afab-e72aada0c4c0)

安装完成后会显示

![image](https://github.com/TheD0ubleC/MusicalMoments/assets/143760576/78943de2-2812-48b5-98a7-42dba6ea5c38)

## 第二步: 配置MM(Musical Moments)

### [下载最新版本](https://github.com/TheD0ubleC/MusicalMoments/releases/tag/Release)

- 下载并解压到任意位置。

### 首次启动

- 首次启动MM时，你将进入引导页。请按照引导完成初次设置。

![image](https://github.com/TheD0ubleC/MusicalMoments/assets/143760576/402b2b7e-7a79-49e9-bfe9-9171cd731be4)

### 虚拟声卡

- 我推荐使用VB虚拟声卡来获取最佳体验。你可以使用压缩包中的VB安装程序，或通过MM下载最新版本的VB。

  **注意**：过新的版本可能导致兼容性问题，请选择适宜版本。但一般不可能会出现，因为虚拟声卡的性质都是相同的。

- 安装成功后，在MM中点击“重新检测”验证安装是否成功。

  如果在安装过程中出现需要管理员权限的提示，请以管理员身份运行安装程序。

![image](https://github.com/TheD0ubleC/MusicalMoments/assets/143760576/abd85c55-806b-4db4-af9c-78a6315adbe9)

 - 如果安装VB时弹出该消息框请使用管理员运行安装程序

### 绑定音频设备与按键

请按下图进行操作

![image](https://github.com/TheD0ubleC/MusicalMoments/assets/143760576/c8a90cd7-9604-4f67-b16a-6fe8e0220298)

- 导入音频可在“音频”页直接拖入音频文件或手动放入运行目录下的“AudioData”文件夹。

- 右键点击导入的音频，并选择“设为播放项”。

![image](https://github.com/TheD0ubleC/MusicalMoments/assets/143760576/38277b5e-34dd-4a24-b15d-604802b6f70e)

### 游戏/语音工具内配置

- 我将以CS2和KOOK为例展示如何在游戏或语音工具中配置MM。

  **KOOK示例配置**

![70717d71a498315b8ebca1ce68d52525](https://github.com/TheD0ubleC/MusicalMoments/assets/143760576/f447ef78-7a27-46f2-92f9-7a152d01f4ea)

  **CS2示例配置**

![image](https://github.com/TheD0ubleC/MusicalMoments/assets/143760576/461615f4-e0b6-4147-b3f2-f205200c4d60)

- 其他语音工具或游戏（如Discord、Valorant、OW2等）的配置方法类似。

## 开始使用

- 完成以上步骤后，按下你绑定的播放音频快捷键即可播放音频。如果需要使用麦克风，按下绑定的切换源快捷键进行切换。切换成功后，会有语音提示（如：“切换为麦克风”或“切换为音频”）。注意，切换为麦克风后通话可能会有大约200ms的延迟，请注意闭麦实际。

**注意**：如果按下快捷键没有反应，请尝试以管理员权限运行MM。
