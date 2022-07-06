using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keyboard_control : MonoBehaviour
{
    private GameObject root;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 用 GetKey 或 GetKeyDown 或 GetKeyUp 交互
        if (Input.GetKey(KeyCode.W))
        {
            // 当按下W键，每秒沿自身z轴向前走一个单位
            transform.Translate(Vector3.forward * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * Time.deltaTime);
        }
    }
}
