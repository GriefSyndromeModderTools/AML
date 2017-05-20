using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSORepPlayer
{
    public class PlayingStatus
    {
        public bool IsReplayLoaded
        {
            get;
            internal set;
        }

        public bool IsReplayFinished
        {
            get;
            internal set;
        }
    }
}
