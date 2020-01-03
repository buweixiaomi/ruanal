using Ruanal.Core.ApiSdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.Core
{
    public class CommandQueue
    {
        const int MAXRUN = 200;
        object rootlock = new object();
        List<CmdDetail> cmds = new List<CmdDetail>();
        List<int> runingcmds = new List<int>();
        public int Count
        {
            get { return cmds.Count; }
        }
        public bool TryEnqueue(CmdDetail cmd)
        {
            lock (rootlock)
            {
                if (cmds.Exists(x => x.CmdId == cmd.CmdId))
                    return false;
                if (runingcmds.Exists(x => x == cmd.CmdId))
                    return false;
                cmds.Add(cmd);
                return true;
            }
        }

        public CmdDetail TryBegin()
        {
            lock (rootlock)
            {
                if (cmds.Count == 0) return null;
                var item = cmds[0];
                cmds.RemoveAt(0);
                while (runingcmds.Count > MAXRUN)
                {
                    runingcmds.RemoveAt(0);
                }
                runingcmds.Add(item.CmdId);
                return item;
            }
        }

        public bool TryEnd(CmdDetail cmd)
        {
            lock (rootlock)
            {
                if (runingcmds.Contains(cmd.CmdId))
                {
                    runingcmds.Remove(cmd.CmdId);
                    return true;
                }
                return false;
            }
        }
    }
}
