using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkRemoteDelegatePlugin
{
    struct APIReturnValue
    {
        public int ReturnValue;
        public int ErrorCode;

        public static APIReturnValue OK()
        {
            return OK(0);
        }

        public static APIReturnValue OK(int value)
        {
            return new APIReturnValue { ReturnValue = value };
        }

        public static APIReturnValue Err(int code)
        {
            return new APIReturnValue { ErrorCode = code };
        }
    }
}
