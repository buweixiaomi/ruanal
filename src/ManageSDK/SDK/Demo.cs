using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruanal.ManageApi
{
    /*
     小系统最优方案：（一般不需要考虑调度任务 即主节点不运行任务，只作TCP服务主机）
     一个主节点，一个运行节点
     主节点添加Tcp监听
     
    调用流程：
    1、添加任务（或者修改）
    2、设置任务版本
    3、启动任务
    4、可选（停止任务）
         
         */
    public class Demo
    {


        Ruanal.ManageApi.ApiClient client;
        public Demo()
        {
            client = new ApiClient("http://localhost:7997/", "run", "");
        }


        public void Start()
        {
            //取得节点
            var nodes = client.NodeList();
            //判断是否有运行节点 一般至少要一个，可用调度主节
            if (nodes.Count(x => x.NodeType == 0) == 0)
                throw new Exception("没有运行节点，不能添加任务！");
            //添加任务
            var taskid = client.TaskAdd(new ApiEntity.Task
            {
                DispatchClass = "",//调度任务入口类名
                EnterClass = "TestTask01.TestTask2@aaaa",//入口类名
                EnterDll = "TestTask01.exe",//入口DLL
                Remark = "备注",
                RunCron = "0/15 * * * * ? *",
                State = 0,//添加修改时无用参数
                TaskConfig = "{\"a\":12}",//配置
                TaskBindings = null,//可空（会自动添加到节点绑定），也可手动绑定
                TaskId = 1,//添加时无效
                TaskType = 0,//0 普通任务 1调度任务
                Title = "默认执行任务"
            });

            //设置版本
            client.TaskSetVersion(new ApiEntity.TaskVersion()
            {
                TaskId = taskid,
                FilePath = "http://localhost:7997/taskDllFile/Debug.zip",//版本地址，从这里下载zip的程序包
                FileSize = 20,//大小 kb，参考用
                Remark = "xyk"//说明
            });

            //开始任务
            client.TaskStart(taskid);

            //停止任务
            client.TaskStop(taskid);


            //删除任务
            client.TaskDelete(taskid);

            //当前的任务列表，任务的状态在TaskBindings项里
            var tasks = client.TaskList();

            //重启节点，推荐无参方式，在任务无法停止等情况下调用，如果节点进程已死，是不会重启的
            client.NodeRestart();


        }


        public void Test()
        {

            //取得节点
            var nodes = client.NodeList();
            //判断是否有运行节点 一般至少要一个，可用调度主节
            if (nodes.Count(x => x.NodeType == 0) == 0)
                throw new Exception("没有运行节点，不能添加任务！");
            //添加任务
            var taskid = client.TaskAdd(new ApiEntity.Task
            {
                DispatchClass = "TestTask01.TestDis4@Getx",//调度任务入口类名
                EnterClass = "TestTask01.TestDis4@Setx",//入口类名
                EnterDll = "TestTask01.exe",//入口DLL
                Remark = "备注",
                RunCron = "0/10 * * * * ? *",
                State = 0,//添加修改时无用参数
                TaskConfig = null,//配置
                TaskBindings = null,//可空（会自动添加到节点绑定），也可手动绑定
                TaskId = 1,//添加时无效
                TaskType = 1,//0 普通任务 1调度任务
                Title = "调度任务-无基类"
            });

            //设置版本
            client.TaskSetVersion(new ApiEntity.TaskVersion()
            {
                TaskId = taskid,
                FilePath = "http://localhost:7997/taskDllFile/Debug.zip",//版本地址，从这里下载zip的程序包
                FileSize = 20,//大小 kb，参考用
                Remark = "xyk"//说明
            });
            return;

            //开始任务
            client.TaskStart(taskid);

            //停止任务
            client.TaskStop(taskid);


            //删除任务
            client.TaskDelete(taskid);

            //当前的任务列表，任务的状态在TaskBindings项里
            var tasks = client.TaskList();

            //重启节点，推荐无参方式，在任务无法停止等情况下调用，如果节点进程已死，是不会重启的
            client.NodeRestart();


        }

    }
}
