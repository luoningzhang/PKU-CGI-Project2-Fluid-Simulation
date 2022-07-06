# 使用说明

谢悦 1900013055



解压该包，使用Unity $2020.3.30f1c1$​以上版本打开，

点击Scenes中的Fluid场景。

主要实现了由一定粒子构成的流体，可形成特定的形状，并可根据键盘控制实现不同的形状的转换，重建了该流体表面。

## 使用方法

点击Scenes中的Fluid场景。打开界面如下

<img src="images\image-20220606000619083.png" style="zoom:80%;" />

空间中有六个玻璃材质的cube构成一个长方体容器

<img src="images\image-20220605232923650.png" alt="image-20220605232923650" style="zoom:33%;" />

点击开始，

**一、”A“，”S“字母键放水、停水**

按“A”字母键开始放水，按“S"字母键流体停住，流体达到200个粒子的时候自动停住。

<img src="images\image-20220605233756694.png" alt="image-20220605233756694" style="zoom:50%;" />

**二、”1“，”2“数字键切换粒子显示状态（重建mesh或粒子状）**

初始流体为重建mesh状，按”2“数字键流体由mesh状变为粒子状，如下。按”1“数字键流体改为mesh状

<img src="images\image-20220605233849564.png" alt="image-20220605233849564" style="zoom:50%;" />

**三、”3“，”4“数字键切换容器形状（四壁朝内缩进，朝外扩张）**

按”3“数字键玻璃容器四壁向内缩进，如下

<img src="images\image-20220605233623247.png" alt="image-20220605233623247" style="zoom:50%;" />

按”3“数字键玻璃容器四壁向外扩张，如下

<img src="images\image-20220605233701893.png" alt="image-20220605233701893" style="zoom:45%;" />

## 代码

主要script代码在Assets\Scripts\FluidSimulation中

FluidSimulation.cs是主要流体模拟器代码，

MarchingCubesComputeShader.compute和CubeGrid.cs是重建mesh代码，

My Surface GPU.mat和StandardMetaballMaterial.mat和Point Surface.shader是物体材质及渲染器。



## Reference

对流体粒子态的显示参考了 https://github.com/denommenator/SPH-Unity/tree/master/Assets 中的材质和渲染器 [My Surface GPU.mat ](https://github.com/denommenator/SPH-Unity/blob/master/Assets/My Surface GPU.mat)和 [Point Surface.shader](https://github.com/denommenator/SPH-Unity/blob/master/Assets/Point Surface.shader) ，及其使用方法。

对流体重建mesh态参考了 https://github.com/dario-zubovic/metaballs 中融球材质 [StandardMetaballMaterial.mat ](https://github.com/dario-zubovic/metaballs/blob/master/Assets/Materials/StandardMetaballMaterial.mat)及融球构建方法和其使用computeshader快速采用marchingcube重建mesh的方法部分代码。

