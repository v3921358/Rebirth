using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    public enum CrashCallType
    {
        //1: SendBackupPacket, 2: ZtlExceptionHandler, 3: CMSException


        Undefined,
        SendBackupPacket,
        ZtlExceptionHandler,
        CMSException
    }
}
