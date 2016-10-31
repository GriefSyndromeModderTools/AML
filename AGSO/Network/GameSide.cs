using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Network
{
    public abstract class GameSide
    {
        public GameSide(byte id)
        {

        }

        public abstract byte[] Dequeue();
        public abstract void Enqueue(byte[] data);
    }
}
