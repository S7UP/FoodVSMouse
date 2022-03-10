using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 在关卡编辑模式下的Grid
/// </summary>
public class MapGrid : MonoBehaviour
{

    /// <summary>
    /// 格子的状态
    /// </summary>
    [System.Serializable]
    public struct GridInfo 
    { 
        public int mainGridStateIndex; // 格子主要状态（地形）的编号
        public int xIndex;
        public int yIndex;

        // 可能还有个美食编号，毕竟要记录初始卡片
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
