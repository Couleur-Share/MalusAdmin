﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalusAdmin.Servers.SysRolePermission
{
    public  interface  ISysRolePermission
    {
        Task<bool> HavePermission(string RouthPath);
    }
}
