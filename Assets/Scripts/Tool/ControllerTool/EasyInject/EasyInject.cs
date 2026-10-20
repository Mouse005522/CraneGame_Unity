using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kun.Tool
{
    /// <summary>
    /// 每個inject可能用途不同所以不設定成靜態
    /// </summary>
    public class EasyInject
    {
        public EasyInject () 
        {
            AddService (this);
        }

        public static void CopyServices (EasyInject from, EasyInject to) 
        {
            foreach (var pair in from.serviceTable) 
            {
                if (pair.Key == typeof (EasyInject)) 
                {
                    continue;
                }

                if (to.serviceTable.ContainsKey (pair.Key) == false)
                {
                    to.serviceTable.Add (pair.Key, pair.Value);
                }
                else
                {
                    LoggerRouter.Error ($"重覆註冊的Service -> {pair.Key.Name}");
                }
            }
        }

        public void AddInjectServiceHandle (InjectServiceHandle injectServiceHandle)
        {
            injectServiceHandles.Add (injectServiceHandle);
        }

        List<InjectServiceHandle> injectServiceHandles = new List<InjectServiceHandle> ();

        /// <summary>
        /// 繼承之前的service
        /// 並產生新的副本
        /// </summary>
        /// <returns></returns>
        public EasyInject CreateClone () 
        {
            var copyService = new Dictionary<Type, object> (this.serviceTable);
            copyService.Remove (typeof (EasyInject));

            EasyInject copyInject = new EasyInject ();
            copyInject.serviceTable = copyService;
            copyInject.AddService (copyInject);

            foreach (var handle in this.injectServiceHandles)
            {
                copyInject.AddInjectServiceHandle (handle);
            }

            return copyInject;
        }

        /// <summary>
        /// 取得符合的Services
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryGetServicesOfType<T> (out T t)
        {
            List<T> services = new List<T> ();

            foreach (var pair in serviceTable)
            {
                if (pair.Value is T _t)
                {
                    t= _t;
                    return true;
                }
            }

            t = default;
            return false;
        }

        /// <summary>
        /// 取得所有符合的Services
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetServicesOfType<T> () 
        {
            List<T> services = new List<T> ();

            foreach (var pair in serviceTable)
            {
                if (pair.Value is T t) 
                {
                    if (services.Contains (t) == false) 
                    {
                        services.Add (t);
                    }
                }
            }

            return services;
        }

        /// <summary>
        /// 如果希望被以基底型別譬如interface之類的
        /// T要傳入想被記住的Type
        /// 透過無參數建構式觸發
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public EasyInjectAddToken<RequestT> AddService<RequestT, ServiceType> (out RequestT requestT) where RequestT : ServiceType, new()
        {
            requestT = new RequestT ();
            AddService<ServiceType> (requestT);

            EasyInjectAddToken<RequestT> token = new EasyInjectAddToken<RequestT> (this, requestT);
            return token;
        }

        /// <summary>
        /// 透過無參數建構式觸發
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public EasyInjectAddToken<RequestT> AddService<RequestT> () where RequestT : new()
        {
            var requestT = new RequestT ();
            AddService (requestT);

            EasyInjectAddToken<RequestT> token = new EasyInjectAddToken<RequestT> (this, requestT);
            return token;
        }

        /// <summary>
        /// 透過無參數建構式觸發
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public EasyInjectAddToken<RequestT> AddService<RequestT> (out RequestT requestT) where RequestT : new()
        {
            requestT = new RequestT ();
            AddService (requestT);

            EasyInjectAddToken<RequestT> token = new EasyInjectAddToken<RequestT>(this, requestT);
            return token;
        }

        /// <summary>
        /// 透過無參數建構式觸發
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public void AddServiceWithRealType (object obj)
        {
            Type serviceType = obj.GetType ();

            AddServiceInternal (obj, serviceType);
        }

        /// <summary>
        /// 透過Inject觸發
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public EasyInjectAddToken<RequestT> AddServiceByRequest<RequestT> ()
        {
            return AddServiceByRequest<RequestT> (out _);
        }

        /// <summary>
        /// 透過Inject觸發
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public EasyInjectAddToken<RequestT> AddServiceByRequest<RequestT> (out RequestT req)
        {
            if (TryCreateRequest (out req))
            {
                AddService (req);

                EasyInjectAddToken<RequestT> token = new EasyInjectAddToken<RequestT> (this, req);
                return token;
            }

            return null;
        }

        /// <summary>
        /// 如果希望被以基底型別譬如interface之類的
        /// T要傳入想被記住的Type
        /// 透過Inject觸發
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public EasyInjectAddToken<ServiceType> AddServiceByRequest<RequestT, ServiceType> (out RequestT req) where RequestT : ServiceType
        {
            if (TryCreateRequest (out req)) 
            {
                AddService<ServiceType> (req);

                EasyInjectAddToken<ServiceType> token = new EasyInjectAddToken<ServiceType> (this, req);
                return token;
            }

            return null;
        }

        /// <summary>
        /// 如果希望被以基底型別譬如interface之類的
        /// T要傳入想被記住的Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public EasyInjectAddToken<T> AddService<T> (T service)
        {
            Type serviceType = typeof (T);

            AddServiceInternal (service, serviceType);

            EasyInjectAddToken<T> token = new EasyInjectAddToken<T> (this, service);
            return token;
        }

        void AddServiceInternal (object service, Type serviceType) 
        {
            if (serviceTable.ContainsKey (serviceType) == false)
            {
                serviceTable.Add (serviceType, service);
            }
            else
            {
                LoggerRouter.Error ($"此type已經註冊 -> {serviceType.Name}");
            }
        }

        Dictionary<Type, object> serviceTable = new Dictionary<Type, object> ();

        public void RemoveService<T> () 
        {
            serviceTable.Remove (typeof (T));
        }

        /// <summary>
        /// 請求產生物件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryCreateRequest<T> (out T request)
        {
            Type reqType = typeof (T);

            if (TryCreateRequest (reqType, out object req)) 
            {
                request = (T) req;
                return true;
            }
            else
            {
                request = default;
                return false;
            }
        }

        /// <summary>
        /// 請求產生物件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryCreateRequest (Type reqType ,out object request)
        {
            bool findCtors = false;

            List<object> pars = new List<object> ();

            var ctors = reqType.GetConstructors ().ToList ();

            List<List<ParameterInfo>> ctorTable = ctors.ConvertAll (c => c.GetParameters ().ToList ());
            //長度0可能是struct
            //應該不會有人長度0還想DI吧
            ctorTable.RemoveAll (ctor => ctor.Count == 0);
            if (ctorTable.Count == 0) 
            {
                request = default;
                LoggerRouter.Error ($"不具有符合的建構式 -> {reqType.Name}");
                return false;
            }

            ctorTable = ctorTable.OrderByDescending (pair => pair.Count).ToList ();

            foreach (var constructor in ctorTable)
            {
                if (TryGetMappingPars (constructor, ref pars)) 
                {
                    findCtors = true;
                    break;
                }
                else
                {
                    pars.Clear ();
                }
            }

            if (findCtors) 
            {
                request = Activator.CreateInstance (reqType, pars.ToArray ());
            }
            else
            {
                request = default;
                LoggerRouter.Error ($"不具有符合的建構式 -> {reqType.Name}");
            }

            return findCtors;
        }

        bool TryGetMappingPars (List<ParameterInfo> parameters, ref List<object> pars)
        {
            bool fail = false;

            for (int i = 0; i < parameters.Count; i++)
            {
                var parType = parameters[i].ParameterType;

                if (TryGetService (parType, out object value))
                {
                    pars.Add (value);
                }
                else
                {
                    fail = true;
                    pars.Clear ();
                }
            }

            return fail == false;
        }

        /// <summary>
        /// 如果是檢查interface這種沒有就不執行的就不用特別出現警告
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="needError"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public bool TryGetService<T> (out T t)
        {
            var type = typeof (T);

            if (TryGetService (type, out object obj)) 
            {
                t = (T)obj;
                return true;
            }
            else
            {
                t = default;
                return false;
            }
        }

        /// <summary>
        /// 如果是檢查interface這種沒有就不執行的就不用特別出現警告
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="needError"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public bool TryGetService (Type serviceType, out object service)
        {
            //內部註冊先找
            if (serviceTable.TryGetValue (serviceType, out service)) 
            {
                return true;
            }
            else
            {
                //外部註冊找
                foreach (var injectServiceHandle in injectServiceHandles)
                {
                    if (injectServiceHandle.TryGetService (serviceType, out service))
                    {
                        return true;
                    }
                }

                LoggerRouter.Error ($"取得service失敗 -> {serviceType}");

                return false;
            }
        }

        /// <summary>
        /// 針對已經實例化的物件透過attribute找出來並綁定服務
        /// </summary>
        /// <param name="target"></param>
        /// <param name="flag"></param>
        public void BindingService (object target) 
        {
            BindingService<EasyInjectAttribute> (target);
        }

        /// <summary>
        /// 針對已經實例化的物件透過attribute找出來並綁定服務
        /// 如果不同attribute被放在不同的Inject裡使用這個做出區分
        /// </summary>
        /// <param name="target"></param>
        /// <param name="flag"></param>
        public void BindingService<T> (object target) where T : Attribute
        {
            var setters = EasyInjectFieldTable<T>.GetSetters (target.GetType ());

            foreach (var setter in setters)
            {
                if (TryGetService (setter.MemberType, out object service))
                {
                    setter.SetValue (target, service);
                }
                else
                {
                    LoggerRouter.Error ($"找不到對應的service -> {target.GetType ().Name}, {setter.MemberType}");
                }
            }
        }
    }
}
