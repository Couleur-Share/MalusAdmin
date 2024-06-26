﻿using MalusAdmin.Common.Helper;
using MalusAdmin.Servers.SysData.Dto;

namespace MalusAdmin.Servers;

public class SysDataService
{
    /// <summary>
    /// 获取服务器信息
    /// </summary>
    /// <returns></returns>
    public async Task<ServerInfo> GetServerInfo()
    {
        var serverInfo = new ServerInfo
        {
            MachineName = ServerInfoUtil.MachineName,
            OSName = ServerInfoUtil.OSName,
            OSArchitecture = ServerInfoUtil.OSArchitecture,
            DoNetName = ServerInfoUtil.DoNetName,
            IP = ServerInfoUtil.IP[0],
            CpuCount = ServerInfoUtil.CpuCount,
            UseRam = ServerInfoUtil.UseRam,
            StartTime = ServerInfoUtil.StartTime,
            RunTime = ServerInfoUtil.RunTime,

            DiskInfo = ServerInfoUtil.DiskInfo,
            MemoryInfo = ServerInfoUtil.MemoryInfo
        };

        return serverInfo;
    }
}