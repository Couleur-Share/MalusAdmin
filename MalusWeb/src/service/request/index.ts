import type { AxiosResponse } from 'axios';
import { BACKEND_ERROR_CODE, createFlatRequest, createRequest } from '@sa/axios';
import { useAuthStore } from '@/store/modules/auth';
import { localStg } from '@/utils/storage';
// import { getServiceBaseURL } from '@/utils/service';
import { $t } from '@/locales';
import { handleRefreshToken } from './shared';

// const isHttpProxy = import.meta.env.DEV && import.meta.env.VITE_HTTP_PROXY === 'Y';
// const { baseURL, otherBaseURL } = getServiceBaseURL(import.meta.env, isHttpProxy);

const baseURL = import.meta.env.VITE_SERVICE_BASE_URL;
// console.log('baseURL', baseURL);

interface InstanceState {
  /** whether the request is refreshing token */
  isRefreshingToken: boolean;
}

export const request = createFlatRequest<App.Service.Response, InstanceState>(
  {
    baseURL
  },
  {
    /** 请求发送之前执行，用来修改请求配置，例如：添加请求头 token */
    async onRequest(config) {
      const { headers } = config;

      // set token
      const token = localStg.get('token');
      // const Authorization = token ? `${token}` : null;
      // 更改 请求头信息
      headers.Token = token ? `${token}` : null;
      // Object.assign(headers, { Authorization });

      return config;
    },
    /** 判断后端响应是否成功，通过对比后端返回的 code 来判断 */
    isBackendSuccess(response) {
      // when the backend response code is "0000"(default), it means the request is success
      // to change this logic by yourself, you can modify the `VITE_SERVICE_SUCCESS_CODE` in `.env` file
      // console.log('response1', response.data.code === import.meta.env.VITE_SERVICE_SUCCESS_CODE);
      // 状态码等于200等于请求成功
      return response.data.code == '200';
    },
    /** 后端请求在业务上表示失败时调用的异步函数，例如：处理 token 过期 */
    async onBackendFail(response, instance) {
      console.log('接口响应错误123123', response, instance);
      const authStore = useAuthStore();

      function handleLogout() {
        authStore.resetStore();
      }

      function logoutAndCleanup() {
        handleLogout();
        window.removeEventListener('beforeunload', handleLogout);
      }
      debugger;

      // 当后端响应代码在logoutCodes中时，意味着用户将会登出并被重定向到登录页面。
      const logoutCodes = [401];
      if (logoutCodes.includes(response.data.code)) {
        window.$message?.error?.(response.data.message);
        handleLogout();
        return null;
      }

      const errorCodes = [207, 403];
      if (errorCodes.includes(response.data.code)) {
        window.$message?.error?.(response.data.message);
        return null;
      }

      // 当后端响应代码在modalLogoutCodes中时，意味着用户将会通过显示一个模态框来被登出。
      const modalLogoutCodes = import.meta.env.VITE_SERVICE_MODAL_LOGOUT_CODES?.split(',') || [];
      if (modalLogoutCodes.includes(response.data.code)) {
        // 阻止用户刷新页面
        window.addEventListener('beforeunload', handleLogout);

        window.$dialog?.error({
          title: 'Error',
          content: response.data.message,
          positiveText: $t('common.confirm'),
          maskClosable: false,
          onPositiveClick() {
            logoutAndCleanup();
          },
          onClose() {
            logoutAndCleanup();
          }
        });

        return null;
      }

      // 当后端响应代码处于“expiredTokeCodes”中时，表示令牌已过期，并刷新令牌
      // api“refreshToken”不能在“expiredTokeCodes”中返回错误代码，否则它将是一个死循环，应返回“logoutCodes”或“modalLogoutCodes”`
      // const expiredTokenCodes = import.meta.env.VITE_SERVICE_EXPIRED_TOKEN_CODES?.split(',') || [];
      // if (expiredTokenCodes.includes(response.data.code) && !request.state.isRefreshingToken) {
      //   request.state.isRefreshingToken = true;

      //   const refreshConfig = await handleRefreshToken(response.config);

      //   request.state.isRefreshingToken = false;

      //   if (refreshConfig) {
      //     return instance.request(refreshConfig) as Promise<AxiosResponse>;
      //   }
      // }

      return null;
    },
    /** 当 responseType 为 json 时，转换后端响应的数据 */
    transformBackendResponse(response) {
      return response.data.body;
    },
    /** 当请求失败时调用的函数(包括请求失败和后端业务上的失败请求)，例如：处理错误信息 */
    onError(error) {
      // 当请求失败时，您可以显示错误消息
      const message = error.message;

      window.$message?.error?.(message);
    }
  }
);
