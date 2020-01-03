using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.MApiEntity
{
    public class TaskBinding
    {
        public int BindId { get; set; }
        public int TaskId { get; set; }
        public int NodeId { get; set; }
        public int LocalState { get; set; }
        public int ServerState { get; set; }
        public DateTime? LastRunTime { get; set; }
        public Node Node { get; set; }
    }
}
