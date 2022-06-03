using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGridController : IGameControllerMember
{
    public BaseGrid[,] mGridList; //格子表
    public Transform masterTrans;
    public LayerMask mask = 1 << 9; // 打开第9层，第9层为格子！

    private BaseGridController()
    {
    }

    public BaseGridController(Transform masterTrans)
    {
        this.masterTrans = masterTrans;
    }

    /// <summary>
    /// 生成场地格子
    /// </summary>
    public void MInit()
    {
        mGridList = new BaseGrid[MapMaker.yRow, MapMaker.xColumn];
        for (int i = 0; i < MapMaker.yRow; i++)
        {
            for (int j = 0; j < MapMaker.xColumn; j++)
            {
                mGridList[i, j] = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Grid/Grid").GetComponent<BaseGrid>();
                mGridList[i, j].transform.SetParent(masterTrans);
                mGridList[i, j].InitGrid(j, i);
                mGridList[i, j].MInit();
            }
        }
    }

    public void MUpdate()
    {
        Ray r = GameController.Instance.mCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(r.origin.x, r.origin.y), Vector2.zero, Mathf.Infinity, mask);

        if (hit.collider != null)
        {
            if (hit.collider.tag == "Grid")
            {
                GameController.Instance.overGrid = hit.collider.GetComponent<BaseGrid>();
            }
        }

        for (int i = 0; i < MapMaker.yRow; i++)
        {
            for (int j = 0; j < MapMaker.xColumn; j++)
            {
                mGridList[i, j].MUpdate();
                //mGridController.GetGridList()[i, j].MUpdate();
            }
        }
    }

    public BaseGrid[,] GetGridList()
    {
        return mGridList;
    }

    public BaseGrid GetGrid(int xIndex, int yIndex)
    {
        return mGridList[yIndex, xIndex];
    }


    public void MDestory()
    {
        throw new System.NotImplementedException();
    }

    public void MPause()
    {
        throw new System.NotImplementedException();
    }

    public void MResume()
    {
        throw new System.NotImplementedException();
    }


}
