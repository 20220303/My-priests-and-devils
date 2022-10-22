using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using com.myDirector;
using PerfabsController;


public class GameController: MonoBehaviour , SceneController, UserAction
{


    /*类的主要任务 
     * 
     *  1.加载资源
     *  2.移动对象
     *  3.计算游戏结果  
     *  
     */



    UserGUI userGUI;


    public CoastController rightCoast;
    public CoastController leftCoast;
    
    public BoatController boat;
    private PAndVController[] PVQueue;

    //水的位置，固定下来(只读属性)
    readonly Vector3 ocean_pos = new Vector3(0, -.7F, 0);



    //游戏启动之前用于初始化任何变量和游戏状态,在脚本实例生命周期中仅被调用一次,不能做协程
    void Awake()
    {

        Director director = Director.getInstance();
        director.currentSceneController = this;
        userGUI = gameObject.AddComponent<UserGUI>() as UserGUI;
        PVQueue = new PAndVController[6];
        loadResources();
    }



    //加载资源
    public void loadResources()
    {
        //加载准备好的预制件water
        //记得改预制件的名字
        //Object.Instantiate加载预制件的位置，如果不写其他参数的话就是预制件原本的位置

        //加载我的Ocean
        GameObject ocean = Object.Instantiate (Resources.Load("Perfabs/Ocean", typeof(GameObject)), ocean_pos, Quaternion.identity, null) as GameObject;
        ocean.name = "ocean";

        //加载我的Coast
        rightCoast = new CoastController("right");
        leftCoast = new CoastController("left");


        //加载我的角色
        for(int i = 0; i < 3; i++)
        {
            PAndVController APriest = new PAndVController("priest", "priest" + i, rightCoast.getAvailablePosition());
            APriest.getOnCoast(rightCoast);
            rightCoast.getOnCoast(APriest);
            PVQueue[i] = APriest;

        }
        for(int i = 0; i < 3; i++)
        {
            PAndVController ADevils = new PAndVController("devil", "devil" + i, rightCoast.getAvailablePosition());
            ADevils.getOnCoast(rightCoast);
            rightCoast.getOnCoast(ADevils);
            PVQueue[i+3] = ADevils;
        }


        //加载船
         boat = new BoatController();
       
    }



    public void restart()
    {
        boat.reset();
        rightCoast.reset();
        leftCoast.reset();
        for (int i = 0; i < PVQueue.Length; i++)
        {
            PVQueue[i].reset();
        }
    }


    //点击PV的组件
    public void PAndVIsClicked(PAndVController POrVCtrl)
    {

        //下船上岸
        if (POrVCtrl.isOnBoat())
        {
            //先判断现在是哪一个岸
            CoastController whichCoast;
            if (boat.get_which_coast() == 1)
            {
                whichCoast = rightCoast;
               
            }
            else
            {
                whichCoast = leftCoast;
            }

            //动作
            boat.GetOffBoat(POrVCtrl.getName());
            POrVCtrl.moveToPosition(whichCoast.getAvailablePosition());
            POrVCtrl.setPosition(whichCoast.getAvailablePosition());
            POrVCtrl.getOnCoast(whichCoast);
            whichCoast.getOnCoast(POrVCtrl);
            

        }
        else//下岸上船
        {
            CoastController whichCoast = POrVCtrl.getCoastController();

            if (boat.getEmptyIndex() == -1)
            {   //船满了
                return;
            }
            else if(whichCoast.get_which_coast() != boat.get_which_coast())
            {  
                //船没靠岸
                return;
            }
            else
            {
                //动作
                whichCoast.getOffCoast(POrVCtrl.getName());
                POrVCtrl.moveToPosition(boat.getAvailablePosition());
                POrVCtrl.setPosition(boat.getAvailablePosition());
                POrVCtrl.getOnBoat(boat);
                boat.GetOnBoat(POrVCtrl);

            } 
        }
        //检查游戏状态
        userGUI.status = endGame();
    }



    //移动船的组件
    public void moveBoat()
    {
        if (boat.isEmpty())
            return;
        boat.Move();
        userGUI.status = endGame();
    }


   

    //判断游戏是否结束的条件0没结束，1输掉，2赢了
    public int endGame()
    {
        //两边的priest和devil的数量
        int right_priest = rightCoast.getPVNum()[0];
        int right_devil = rightCoast.getPVNum()[1];

        int left_priest = leftCoast.getPVNum()[0];
        int left_devil = leftCoast.getPVNum()[1];

        //胜利的唯一情况
        if (left_devil + left_priest == 6) return 2;


        if (boat.get_which_coast() == 1)
        {
            //在right岸
            right_priest += boat.getPVNum()[0];
            right_devil += boat.getPVNum()[1];

        }
        else
        {
            //在left岸
            left_priest += boat.getPVNum()[0];
            left_devil += boat.getPVNum()[1];
        }

        //输掉的情况
        if (right_priest < right_devil && right_priest > 0) return 1;
        if (left_priest < left_devil && left_priest > 0) return 1;
        

        //继续游戏
        return 0;
    }



}