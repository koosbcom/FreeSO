﻿using FSO.Common.Security;
using FSO.Server.Framework.Aries;
using Mina.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Server.Framework.Voltron
{
    public class VoltronSession : AriesSession, IVoltronSession
    {
        public uint UserId { get; set; }
        public uint AvatarId { get; set; }

        public bool IsAnonymous
        {
            get
            {
                return AvatarId == 0;
            }
        }

        public VoltronSession(IoSession ioSession) : base(ioSession){
        }



        public void DemandAvatar(uint id, AvatarPermissions permission)
        {
            if(AvatarId != id){
                throw new SecurityException("Permission " + permission + " denied for avatar " + id);
            }
        }
    }
}