using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.ManageApi.ApiEntity
{
    public class Node
    {
        public int NodeId { get; set; }
        public string ClientId { get; set; }
        public string Title { get; set; }
        public string NodeConfig { get; set; }
        /// <summary>0:工作节点  1:调度节点</summary>
        public int NodeType { get; set; }
        public DateTime? LastHeartTime { get; set; }
        public int State { get; set; }
        public string Remark { get; set; }
    }
}
