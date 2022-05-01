# Unity_Demo_StatusMachine

Unity版本：2018.3.7f1

网上找不到简单的状态机(Player那啥的状态机)，找到的都不好用，太复杂了，而自己的项目不需要那么复杂难用的东西...于是就自己写了一个状态机。<br>
其中最大的难点莫过于Inspector的自定义，太恶心了，当时以为三两天就能搞定，结果大E了，用了一个多星期才搞定，被PropertyDrawer这个坑货搞了数次心态。

<br>
<br>
<br>


***

绘制得一般般，用起来也一般般，反正就是一般般，拙作，花了一星期干出这东西，自己都只能说一句，“就这？”<br>
可能最大的收获就是让我熟悉了PropertyDrawer这个苟东西(被它支配的恐惧还残留在DNA中


<img src="https://github.com/Ls-Jan/Unity_Demo_StatusMachine/blob/main/RunningDisplay%5BMP4%2CGIF%2CPNG%5D/1.png" height="400"><img src="https://github.com/Ls-Jan/Unity_Demo_StatusMachine/blob/main/RunningDisplay%5BMP4%2CGIF%2CPNG%5D/2.gif" height="400">

<img src="https://github.com/Ls-Jan/Unity_Demo_StatusMachine/blob/main/RunningDisplay%5BMP4%2CGIF%2CPNG%5D/3.png" height="400">

<br>
<br>
<br>
<br>

***

下面的运行样例没啥说的，我设置的状态机很简单，红点是Enemy。<br>
当蓝点(Player)离红点远了红点进入到Fast模式，速度加快；<br>
当蓝点在红点范围内时，红点进入到Slow模式，速度减慢，并且当射击CD小于0时发动攻击。

<img src="https://github.com/Ls-Jan/Unity_Demo_StatusMachine/blob/main/RunningDisplay%5BMP4%2CGIF%2CPNG%5D/4.gif" height="400">

