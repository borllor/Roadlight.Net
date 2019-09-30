using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roadlight.Utility
{
    public static class CacheKeys
    {
        /// <summary>
        /// 接收消息缓存Key，{0}表示MessageKey，{1}表示MessageType
        /// </summary>
        public const string ReceivedMessage_Arg2 = "JinRi_Notify_ReceivedMessage_{0}_{1}";
        /// <summary>
        /// 接收消息缓存时间
        /// </summary>
        public const int ReceivedMessage_TimeOut = 60 * 10;
    }
}
