﻿
namespace MalusAdmin.Common;

/// <summary>
///  用户上下文服务接口
/// </summary>
public interface IUserContextService
{

    /// <summary>
    ///   获取当前用户的TokenData信息
    /// </summary>
    /// <returns></returns>
    TokenData GetUserTokenData();
}
