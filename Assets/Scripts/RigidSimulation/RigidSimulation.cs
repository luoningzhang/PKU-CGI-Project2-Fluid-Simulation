using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;

public class RigidSimulation : MonoBehaviour
{
    public double m_simuStep = 0.001;


    Vector3D m_gravity = new Vector3D(0, -9.8, 0);

    RJoint[] m_joints;
    RBody[] m_bodies;

    // ! 在这里定义你需要的变量


    // Start is called before the first frame update
    void Start()
    {
        m_joints = GetComponentsInChildren<RJoint>();
        m_bodies = GetComponentsInChildren<RBody>();

        // ! 在这里写下你需要的初始化

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        int simuSteps = (int)(Mathf.Round(Time.deltaTime / (float)m_simuStep));
        for (int i = 0; i < simuSteps; i++)
            UpdateFunc();
    }

    void UpdateFunc()
    {
        // ! 在这里写下仿真过程

    }
}
