// !!!!!!!!!!!!!!!!!!!
// 姓名：
// 学号：
// !!!!!!!!!!!!!!!!!!!

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Matrix4x4Extension
{
    public static Matrix4x4 Add(this Matrix4x4 lhs, Matrix4x4 rhs)
    {
        Matrix4x4 ret = Matrix4x4.zero;
        for (int i = 0; i < 4; ++i)
            ret.SetColumn(i, lhs.GetColumn(i) + rhs.GetColumn(i));

        return ret;
    }

    public static Matrix4x4 Substract(this Matrix4x4 lhs, Matrix4x4 rhs)
    {
        Matrix4x4 ret = Matrix4x4.zero;
        for (int i = 0; i < 4; ++i)
            ret.SetColumn(i, lhs.GetColumn(i) - rhs.GetColumn(i));

        return ret;
    }

    public static Matrix4x4 Negative(this Matrix4x4 lhs)
    {
        Matrix4x4 ret = Matrix4x4.zero;
        for (int i = 0; i < 4; ++i)
            ret.SetColumn(i, -lhs.GetColumn(i));

        return ret;
    }

    public static Matrix4x4 Multiply(this Matrix4x4 lhs, float s)
    {
        Matrix4x4 ret = Matrix4x4.zero;
        for (int i = 0; i < 4; ++i)
            ret.SetColumn(i, lhs.GetColumn(i) * s);

        return ret;
    }
    public static Matrix4x4 Multiply(this float s, Matrix4x4 lhs)
    {
        Matrix4x4 ret = Matrix4x4.zero;
        for (int i = 0; i < 4; ++i)
            ret.SetColumn(i, s * lhs.GetColumn(i));

        return ret;
    }

    public static Matrix4x4 Multiply(this Matrix4x4 lhs, Matrix4x4 rhs)
    {
        return lhs * rhs;
    }

    public static float Trace(this Matrix4x4 lhs)
    {
        return lhs.m00 + lhs.m11 + lhs.m22;
    }


    public static Vector3 Add3(this Vector3 lhs, Vector4 rhs)
    {
        return new Vector3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
    }
    public static Vector3 Add3(this Vector4 lhs, Vector3 rhs)
    {
        return new Vector3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
    }
    public static Vector3 Add3(this Vector4 lhs, Vector4 rhs)
    {
        return new Vector3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
    }
    public static Vector3 Add3(this Vector3 lhs, Vector3 rhs)
    {
        return lhs + rhs;
    }

    public static Vector3 AddAssign3(ref this Vector3 lhs, Vector4 rhs)
    {
        lhs.x += rhs.x;
        lhs.y += rhs.y;
        lhs.z += rhs.z;
        return lhs;
    }
    public static Vector3 AddAssign3(ref this Vector4 lhs, Vector3 rhs)
    {
        lhs.x += rhs.x;
        lhs.y += rhs.y;
        lhs.z += rhs.z;
        return lhs;
    }
    public static Vector3 AddAssign3(ref this Vector4 lhs, Vector4 rhs)
    {
        lhs.x += rhs.x;
        lhs.y += rhs.y;
        lhs.z += rhs.z;
        return lhs;
    }
    public static Vector3 AddAssign3(ref this Vector3 lhs, Vector3 rhs)
    {
        return lhs += rhs;
    }
}


public class SimuCube : MonoBehaviour
{
    // 杨氏模量
    public float youngModulus = 1e6f;

    // 泊松比
    public float possionRatio = 0.47f;

    // 密度
    public float density = 1000f;

    // 重力加速度
    public Vector3 gravity = new Vector3(0.0f, -9.8f, 0.0f);

    // 时间步
    public float simuTimeStep = 0.001f;

    // 最大仿真次数, 防止过于卡顿
    public int maxSimuSteps = -1;


    // size
    readonly float cubeSize = 1.0f;
    readonly int subdivide = 2;

    float []masses;
    Vector3[]positions;
    Vector3[]velocity;
    Vector3[]forces;

    // ground
    GameObject groundObject;

    // mesh
    MeshFilter mesh;
    int[] meshVertexMap;

    Matrix4x4 identityMatrix = Matrix4x4.identity;

    private class Tetrahedron
    {
        public int[] vi = new int[4];
        public float volume = 1.0f / 6.0f;
        public Matrix4x4 Dm = Matrix4x4.identity;
        public Matrix4x4 Bm = Matrix4x4.identity;
    }
    Tetrahedron[] tets;

    // 计算拉梅参数
    float mu = 0;
    float lmbda = 0;


    // 在以上部分填写你的代码
    void Start()
    {
        int numVerticesPerDim = subdivide + 2;
        int numVertices = numVerticesPerDim * numVerticesPerDim * numVerticesPerDim;

        masses = new float[numVertices];
        positions = new Vector3[numVertices];
        velocity = new Vector3[numVertices];
        forces = new Vector3[numVertices];
        
        // 初始化仿真格点
        int posIdx = 0;
        for (int i = 0; i < numVerticesPerDim; ++i)
            for (int j = 0; j < numVerticesPerDim; ++j)
                for(int k = 0; k < numVerticesPerDim; ++k)
                {
                    var offset = cubeSize * new Vector3((float)i / (subdivide + 1), (float)j / (subdivide + 1), (float)k / (subdivide + 1));
                    positions[posIdx] = transform.TransformPoint(offset);
                    ++posIdx;
                }

        groundObject = GameObject.Find("Ground");

        // 获取mesh
        mesh = GetComponentInChildren<MeshFilter>();
        var vertices = mesh.mesh.vertices;
        meshVertexMap = new int[vertices.Length];

        Vector3 vMin = vertices[0];
        Vector3 vMax = vertices[0];
        foreach (var v in vertices)
        {
            vMin = Vector3.Min(v, vMin);
            vMax = Vector3.Max(v, vMax);
        }

        var meshOffset = vMin;
        var meshScale = (vMax - vMin) / cubeSize;
        var invScale = new Vector3(1.0f / meshScale.x, 1.0f / meshScale.y, 1.0f / meshScale.z);

        for (int i = 0; i < vertices.Length; i++)
        {
            var pos = (vertices[i] - meshOffset);
            pos.Scale(invScale);
            var idx = pos * (subdivide + 1);
            int xi = Mathf.Clamp(Mathf.RoundToInt(idx.x), 0, numVerticesPerDim - 1);
            int yi = Mathf.Clamp(Mathf.RoundToInt(idx.y), 0, numVerticesPerDim - 1);
            int zi = Mathf.Clamp(Mathf.RoundToInt(idx.z), 0, numVerticesPerDim - 1);
            meshVertexMap[i] = (xi * numVerticesPerDim + yi) * numVerticesPerDim + zi;
        }

        // 计算拉梅参数
        mu = youngModulus / (2.0f * (1.0f + possionRatio));
        lmbda = youngModulus * possionRatio / ((1.0f + possionRatio) * (1.0f - 2.0f * possionRatio));


        // 其他初始化代码
        // 在以上部分填写你的代码
    }


    void FixedUpdate()
    {
        int simuSteps = (int)(Mathf.Round(Time.deltaTime / simuTimeStep));
        if (maxSimuSteps > 0)
            simuSteps = Mathf.Min(simuSteps, maxSimuSteps);

        for (int simuCnt = 0; simuCnt < simuSteps; ++simuCnt)
        {
            UpdateFunc();
        }


        // 更新顶点位置
        var vertices = mesh.mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            var pos = positions[meshVertexMap[i]];
            pos = mesh.transform.InverseTransformPoint(pos);
            vertices[i] = pos;
        }
        mesh.mesh.vertices = vertices;
        mesh.mesh.RecalculateNormals();
    }
    void UpdateFunc()
    {
        float groundHeight = 0;
        if (groundObject != null)
            groundHeight = groundObject.transform.position.y + groundObject.transform.localScale.y / 2;

        // 进行仿真，计算每个顶点的位置
        // ! 在以下部分实现FEM仿真


        //// 在以上部分填写你的代码

    }
}
