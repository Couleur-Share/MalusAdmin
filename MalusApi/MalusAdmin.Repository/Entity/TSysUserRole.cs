﻿using System;
using System.Linq;
using System.Text;
using MalusAdmin.Common;
using MalusAdmin.Common.Model;
using MalusAdmin.Entity;
using SqlSugar;

namespace Models
{
    ///<summary>
    /// 用户角色表
    ///</summary>
    [SugarTable("t_sys_user_role")]
    public  class TSysUserRole : ModelBase
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 角色id
        /// </summary>
        public int RoleId { get; set; }

         
    }
}
