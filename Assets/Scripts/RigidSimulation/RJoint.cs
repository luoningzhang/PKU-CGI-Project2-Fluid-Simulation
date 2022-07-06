using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;

public class RJoint : MonoBehaviour
{
    // Start is called before the first frame update
    public RBody m_body1;
    public RBody m_body2;

    [HideInInspector]
    public Vector3D m_fromBody1;
    public Vector3D m_fromBody2;

    void Start()
    {
        var off1 = transform.position;
        if (m_body1 != null)
            off1 -= m_body1.transform.position;
        m_fromBody1 = new Vector3D(off1.x, off1.y, off1.z);

        var off2 = transform.position;
        if (m_body2 != null)
            off2 -= m_body2.transform.position;
        m_fromBody2 = new Vector3D(off2.x, off2.y, off2.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_body2 != null)
        {
            transform.position = m_body2.transform.position + m_body2.transform.rotation * (
                new Vector3((float)m_fromBody2.X, (float)m_fromBody2.Y, (float)m_fromBody2.Z));
        }
        else if (m_body1 != null)
        {
            transform.position = m_body2.transform.position + m_body1.transform.rotation *(
                new Vector3((float)m_fromBody1.X, (float)m_fromBody1.Y, (float)m_fromBody1.Z));
        }
    }
}
