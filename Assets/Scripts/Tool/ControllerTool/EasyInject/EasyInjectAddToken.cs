using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kun.Tool
{
    public class EasyInjectAddToken<T>
    {
        public EasyInjectAddToken (EasyInject easyInject, T service)
        {
            this.easyInject = easyInject;
            this.service = service;
        }

        EasyInject easyInject;
        T service;

        /// <summary>
        /// 註冊完實體後
        /// 想再把同個refence註冊成interface
        /// </summary>
        /// <typeparam name="WithT"></typeparam>
        /// <returns></returns>
        public EasyInjectAddToken<T> With<WithT> ()
        {
            if (service is WithT withTService)
            {
                easyInject.AddService (withTService);
            }
            else
            {
                LoggerRouter.Error ($"無法將類型 '{typeof (T)}' 轉換成 '{typeof (WithT)}'");
            }
            return this;
        }
    }
}
