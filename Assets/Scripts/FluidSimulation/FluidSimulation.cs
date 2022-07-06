using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH{
public class BufferReleaser : MonoBehaviour{
    public static void ReleaseAllBuffers(){
        ComputeBufferWrapperFloat3.ReleaseBuffers();
    }
    public void OnDestroy(){
        ReleaseAllBuffers();
    }
}

public class ComputeBufferWrapperFloat3{
    static private List<ComputeBuffer> _BufferPool;
    private int _bufferPoolIndex;
    private int _BufferDim;
    private int _BufferStride;
    public int dim => _BufferDim;
    static ComputeBufferWrapperFloat3()
    {
        _BufferPool = new List<ComputeBuffer>();
    }
    public static void ReleaseBuffers(){
        foreach (ComputeBuffer b in ComputeBufferWrapperFloat3._BufferPool)
        {
            b.Release();
        }
    }
    public ComputeBufferWrapperFloat3(int dim){
        Vector3[] zero_initialized_data = new Vector3[dim];
        for (int i = 0; i < dim; i++)
        {
            zero_initialized_data[i] = Vector3.zero;
        }
        _BufferDim = zero_initialized_data.Length;
        _BufferStride = sizeof(float) * 3;
        _bufferPoolIndex = ComputeBufferWrapperFloat3._BufferPool.Count;

        ComputeBuffer buffer = new ComputeBuffer(_BufferDim, _BufferStride);
        buffer.SetData(zero_initialized_data);
        ComputeBufferWrapperFloat3._BufferPool.Add(buffer);
    }

    public ComputeBufferWrapperFloat3(Vector3[] data){
        _BufferDim = data.Length;
        _BufferStride = sizeof(float) * 3;
        _bufferPoolIndex = ComputeBufferWrapperFloat3._BufferPool.Count;

        ComputeBuffer buffer = new ComputeBuffer(_BufferDim, _BufferStride);
        buffer.SetData(data);
        ComputeBufferWrapperFloat3._BufferPool.Add(buffer);

    }
    public void SetData(Vector3[] data){
        ComputeBufferWrapperFloat3._BufferPool[_bufferPoolIndex].SetData(data);
    }
    public static implicit operator ComputeBuffer(ComputeBufferWrapperFloat3 b) => ComputeBufferWrapperFloat3._BufferPool[b._bufferPoolIndex];
    public static implicit operator ComputeBufferWrapperFloat3(Vector3[] A) => new ComputeBufferWrapperFloat3(A);
    public static implicit operator Vector3[](ComputeBufferWrapperFloat3 ABuffer)
    {
        Vector3[] A = new Vector3[ABuffer.dim];
        ((ComputeBuffer)ABuffer).GetData(A);
        return A;
    }
}
public struct ContainerWall{
    public Vector3 inward_normal;
    public Vector3 point;
    public float elasticity;
}
public class Containers{
    static public ContainerWall[] BoxContainer(float width, float height, float depth, float depth_1,float wallElasticity){
        ContainerWall wall_lower, wall_upper, wall_left, wall_right, wall_back, wall_front;
        wall_lower = new ContainerWall{
            inward_normal = Vector3.up,
            point = Vector3.zero,
            elasticity = wallElasticity
        };
        wall_upper = new ContainerWall{
            inward_normal = Vector3.down,
            point = height * Vector3.up,
            elasticity = wallElasticity
        };
        wall_left = new ContainerWall{
            inward_normal = Vector3.right,
            point = width / 2 * Vector3.left,
            elasticity = wallElasticity
        };
        wall_right = new ContainerWall{
            inward_normal = Vector3.left,
            point = width / 2 * Vector3.right,
            elasticity = wallElasticity
        };
        wall_back = new ContainerWall{
            inward_normal = Vector3.forward,
            point = ((depth/2)+(depth_1/2)) * Vector3.back,
            elasticity = wallElasticity
        };
        wall_front = new ContainerWall{
            inward_normal = Vector3.back,
            point = ((depth/2)-(depth_1/2)) * Vector3.back,
            elasticity = wallElasticity
        };

        ContainerWall[] walls = new ContainerWall[]{wall_lower, wall_upper, wall_left, wall_right, wall_back, wall_front };
        return walls;

    }
}
public class FluidSimulation : MonoBehaviour
{
    float containerHeight=10.0f;
    float containerWidth=10.0f;
    float containerDepth=10.0f;
    float initialParticleSeparation = 2.0f;
    public float wall_elasticity = 0.4f;
    public float hDensity = 1.1f;
    public float hViscosity = 7.0f;
    public float k = 100.0f;
    public float g = 7.0f;
    public float radius = 0.1f;
    public float particleMass = 10.0f;
    static float PI = 3.141592653F;
    static float epsilon = 0.0001f;
    static float thres = 0.0022f;
    int number=0;

    public Material sphMaterial;
    static readonly int sphMaterialPositionsID = Shader.PropertyToID("_Positions");

    private Mesh mesh_1;

    private float[] Densities;
    private float[] Pressures;
    private Vector3[] PressureForces;
    private Vector3[] ViscosityForces;
    private Vector3[] Accelerations;
    private Vector3[] Positions;
    private Vector3[] Velocities;
    BufferReleaser bufferReleaser;
    private ContainerWall[]  ContainerWalls;

    private float resolution=0.2f;
    private float threshold=1.0f;
    public ComputeShader computeShader;

    private CubeGrid grid;
    private int visualState=0;
    private int wallState=0;
    private int watering=0;
    private float range_depth=0;
    GameObject cube1 = new GameObject();
    GameObject cube2 = new GameObject();
    GameObject cube3 = new GameObject();
    GameObject cube5 = new GameObject();
    // Start is called before the first frame update
    public void Start(){
        cube1 = GameObject.Find("Cube1");
        cube2 = GameObject.Find("Cube2");
        cube3 = GameObject.Find("Cube3");
        cube5 = GameObject.Find("Cube5");
        range_depth = containerDepth/3;
        grid = new CubeGrid(computeShader,resolution,containerWidth,containerHeight,containerDepth,threshold);
        ContainerWalls = Containers.BoxContainer(containerWidth-range_depth, containerHeight, containerDepth, containerDepth-range_depth,wall_elasticity);
        bufferReleaser = gameObject.AddComponent<BufferReleaser>();
        number = 1;
        Positions       = new Vector3[number];
        Velocities      = new Vector3[number];
        Positions[0] = new Vector3(containerWidth/4,containerHeight*2/3,-containerDepth/2);

        GameObject tmp_obj = new GameObject();
        tmp_obj = GameObject.Find("Sphere");
        mesh_1 = tmp_obj.GetComponent<MeshFilter>().mesh;
        Vector3[] point = mesh_1.vertices;
        for(int i=0;i<point.Length;i++){
            point[i] = new Vector3(mesh_1.vertices[i].x*radius,mesh_1.vertices[i].y*radius,mesh_1.vertices[i].z*radius);
        }
        mesh_1.vertices = point;
        mesh_1.RecalculateNormals();
    }
    float W_viscosity_laplacian(Vector3 r_vec,float h){
        float r = r_vec.magnitude;
        float result = 0.0f;
        if(r<=h) result=45/(PI*Mathf.Pow(h,6))*(h-r);
        return result;
    }
    float W_poly(Vector3 r_vec, float h){
        float r_squared = Vector3.Dot(r_vec,r_vec);
        float result = 0.0f;
        if(r_squared<=Mathf.Pow(h, 2))
            result = 315/(64*PI *Mathf.Pow(h,9))*Mathf.Pow(Mathf.Pow(h,3)-r_squared, 3);
        return result;
    }
    Vector3 W_spiky_gradient(Vector3 r_vec, float h){
        float r = r_vec.magnitude;
        Vector3 result;
        if(r<0.01f)result = new Vector3(0.0f, 0.0f, 0.0f);
        else if(r>h)result = new Vector3(0.0f, 0.0f, 0.0f);
        else result = -15/(PI*Mathf.Pow(h,6))*3*Mathf.Pow((h-r),2)*r_vec.normalized;
        return result;
    }
    Vector3 reset_position(Vector3 p, ContainerWall wall){
        float p_dot_n = Vector3.Dot(p - wall.point, wall.inward_normal);
        return p - p_dot_n * wall.inward_normal + epsilon * wall.inward_normal;
        
    }
    Vector3 reflect_velocity(Vector3 p_dot, ContainerWall wall){
        float p_dot_n = Vector3.Dot(p_dot, wall.inward_normal);
        return p_dot - (1 + wall.elasticity) * p_dot_n * wall.inward_normal;
    }


    public void FixedUpdate(){
        if(watering != 0 && number<=200){
            number++;
            Vector3[] _Positions       = new Vector3[number];
            Vector3[] _Velocities      = new Vector3[number];
            for(int i=0;i<number-1;i++){
                _Positions[i]  = Positions[i];
                _Velocities[i] = Velocities[i];
            }
            if(watering == 1)_Positions[number-1]  = new Vector3(containerWidth/4-0.5f,containerHeight*2/3,-containerDepth/2);
            if(watering == 2)_Positions[number-1]  = new Vector3(containerWidth/4     ,containerHeight*2/3,-containerDepth/2-0.5f);
            if(watering == 3)_Positions[number-1]  = new Vector3(containerWidth/4+0.5f,containerHeight*2/3,-containerDepth/2);
            if(watering == 4)_Positions[number-1]  = new Vector3(containerWidth/4     ,containerHeight*2/3,-containerDepth/2+0.5f);
            watering = watering%4+1;
            _Velocities[number-1] = new Vector3(0.0f,0.0f,0.0f);
            Positions = _Positions;
            Velocities= _Velocities;
        }
        if(wallState==1){
            range_depth+=0.1f;
            cube1.transform.position = new Vector3(cube1.transform.position.x+thres,cube1.transform.position.y,cube1.transform.position.z);
            cube2.transform.position = new Vector3(cube2.transform.position.x,cube2.transform.position.y,cube2.transform.position.z-thres);
            cube3.transform.position = new Vector3(cube3.transform.position.x-thres,cube3.transform.position.y,cube3.transform.position.z);
            cube5.transform.position = new Vector3(cube5.transform.position.x,cube5.transform.position.y,cube5.transform.position.z+thres);
            ContainerWalls = Containers.BoxContainer(containerWidth-range_depth, containerHeight, containerDepth, containerDepth-range_depth,wall_elasticity);
            if(range_depth>=containerDepth*2/3)wallState=0;
        }
        if(wallState==2){
            range_depth-=0.1f;
            cube1.transform.position = new Vector3(cube1.transform.position.x-thres,cube1.transform.position.y,cube1.transform.position.z);
            cube2.transform.position = new Vector3(cube2.transform.position.x,cube2.transform.position.y,cube2.transform.position.z+thres);
            cube3.transform.position = new Vector3(cube3.transform.position.x+thres,cube3.transform.position.y,cube3.transform.position.z);
            cube5.transform.position = new Vector3(cube5.transform.position.x,cube5.transform.position.y,cube5.transform.position.z-thres);
            ContainerWalls = Containers.BoxContainer(containerWidth-range_depth, containerHeight, containerDepth, containerDepth-range_depth,wall_elasticity);
            if(range_depth<=containerDepth/3)wallState=0;
        }
        Densities       = new float[number];
        Pressures       = new float[number];
        PressureForces  = new Vector3[number];
        ViscosityForces = new Vector3[number];
        Accelerations   = new Vector3[number];

        float dt = Time.fixedDeltaTime;
        for(int i=0;i<number;i++){
            Densities[i] = 0;
            for(int j=0;j<number;j++){
                Densities[i]+=particleMass*W_poly(Positions[i]-Positions[j],hDensity);
            }
            Pressures[i] = k*(Densities[i]-particleMass);
        }
        for(int i=0;i<number;i++){
            PressureForces[i] = new Vector3(0.0f, 0.0f, 0.0f);
            for(int j=0;j<number;j++){
                PressureForces[i]+=-particleMass*(Pressures[i]+Pressures[j])/(2*Densities[j])*W_spiky_gradient(Positions[i]-Positions[j],hDensity);
            }
        }
        for(int i=0;i<number;i++){
            ViscosityForces[i] = new Vector3(0.0f, 0.0f, 0.0f);
            for(int j=0;j<number;j++){
                ViscosityForces[i]+=(Velocities[j]-Velocities[i])/Densities[j]*W_viscosity_laplacian(Positions[i]-Positions[j],hViscosity);
            }
        }
        Vector3 G = new Vector3(0.0f, -g, 0.0f);
        for(int i=0;i<number;i++){
	        Accelerations[i]=(PressureForces[i]+ViscosityForces[i])/Densities[i]+G;
        }
        for(int i=0;i<number;i++){
            Velocities[i]=Velocities[i]+dt*Accelerations[i];
            Positions[i]=Positions[i]+dt*Velocities[i];
        }
        for(uint wall_index = 0; wall_index < 6; wall_index++){
            ContainerWall wall = ContainerWalls[wall_index];
            for(int i=0;i<number;i++){
                Vector3 p = Positions[i];
                Vector3 p_dot = Velocities[i];
                if(Vector3.Dot(p - wall.point, wall.inward_normal) < 0){
                    Positions[i] = reset_position(p, wall);
                    if(Vector3.Dot(p_dot, wall.inward_normal) < 0)Velocities[i] = reflect_velocity(p_dot, wall);
                }
            }
        }
        
    }

    // Update is called once per frame
    public void Update(){
        Vector3[] _Positions = new Vector3[number];
        for(int i=0;i<number;i++){
            _Positions[i] = new Vector3(Positions[i].x / ((float) containerWidth*1.4f),
                Positions[i].y / ((float) containerHeight*1.2f)-0.5f/1.2f,
                Positions[i].z / ((float) containerDepth*1.4f)+0.5f/1.4f);
        }
        if(visualState == 0){
            grid.evaluateAll(_Positions,number);
            Mesh mesh_k = GetComponent<MeshFilter>().mesh;
            mesh_k.Clear();
            mesh_k.vertices = grid.vertices.ToArray();
            mesh_k.triangles = grid.getTriangles();
            mesh_k.RecalculateNormals();
        }
        else{
            sphMaterial.SetBuffer(sphMaterialPositionsID, new ComputeBufferWrapperFloat3(_Positions));
            float max = Mathf.Max(containerDepth, containerHeight, containerWidth);
            Bounds bounds = new Bounds(Vector3.zero, Vector3.one * max);
            Graphics.DrawMeshInstancedProcedural(mesh_1, 0, sphMaterial, bounds, number);
        }

        if(Input.GetKeyDown(KeyCode.Alpha1))visualState = 0;
        if(Input.GetKeyDown(KeyCode.Alpha2)){
            visualState = 1;
            Mesh mesh_k = GetComponent<MeshFilter>().mesh;
            mesh_k.Clear();
            mesh_k.vertices = new Vector3[number];
            mesh_k.RecalculateNormals();
        }
        if(Input.GetKeyDown(KeyCode.A))watering = 1;
        if(Input.GetKeyDown(KeyCode.S))watering = 0;
        if(Input.GetKeyDown(KeyCode.Alpha3)){
            wallState = 1;
        }
        if(Input.GetKeyDown(KeyCode.Alpha4)){
            wallState = 2;
        }
    }

    }
}