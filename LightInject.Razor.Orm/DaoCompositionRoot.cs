using Razor.Orm;
using System;
using System.Data.Common;

namespace LightInject.Razor.Orm
{
    public abstract class DaoCompositionRoot : DaoGenerator, ICompositionRoot
    {
        private IServiceRegistry serviceRegistry;

        public void Compose(IServiceRegistry serviceRegistry)
        {
            this.serviceRegistry = serviceRegistry;

            if (ConnectionLifetime != null)
            {
                this.serviceRegistry.Register(CreateConnection, ConnectionLifetime);
            }
            else
            {
                this.serviceRegistry.Register(CreateConnection);
            }

            this.serviceRegistry.Register(f => Extractor);
            Generate();
        }

        protected abstract DbConnection CreateConnection(IServiceFactory serviceFactory);

        protected virtual ILifetime ConnectionLifetime
        {
            get
            {
                return null;
            }
        }

        protected override void GeneratedDao(Type interfaceType, Type generatedType)
        {
            serviceRegistry.Register(interfaceType, generatedType);
        }
    }
}
