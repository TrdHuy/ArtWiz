using SPRNetTool.Utils;
using System;
using System.Collections.Generic;

namespace SPRNetTool.Domain.Base
{
    public interface IDomainAccessors
    {
        protected class DomainContext
        {
            public static DomainContext ApplicationDomainContext = new DomainContext();

            public Dictionary<Type, object?[]> domainsList;

            DomainContext()
            {
                domainsList = new Dictionary<Type, object?[]>();
                domainsList.Add(typeof(IBitmapDisplayManager), BuildValue(null, () => new BitmapDisplayManager()));
                domainsList.Add(typeof(ISprWorkManager), BuildValue(null, () => new WorkManager()));
            }

            ~DomainContext()
            {
                domainsList.Clear();
            }

            private object?[] BuildValue(params object?[] values)
            {
                return values;
            }

            public static T GetDomain<T>() where T : IObservableDomain
            {
                var type = typeof(T);
                if (ApplicationDomainContext.domainsList.ContainsKey(type))
                {
                    var item = ApplicationDomainContext.domainsList[type];
                    return (T)(item[0] ?? ((Func<IObservableDomain>)item[1]!)().Also((it) =>
                    {
                        item[0] = it;
                    }));
                }
                else
                {
                    throw new InvalidOperationException("Domain was not registered.");
                }
            }
        }


    }
}
