using PluginUtils.Injection.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.SaveData
{
    public class SaveDataHelper
    {
        public static event Action<GSDataFile.CompoundType> ModifySaveData;

        internal class SaveFile : CachedModificationFileProxyFactory
        {
            public override byte[] Modify(byte[] data)
            {
                var e = ModifySaveData;
                if (e == null)
                {
                    return data;
                }
                var obj = GSDataFile.Read(data);
                e(obj);
                return GSDataFile.Write(obj);
            }
        }
    }
}
