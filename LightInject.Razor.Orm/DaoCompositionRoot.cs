using Razor.Orm;
using System;
using System.Data.SqlClient;

namespace LightInject.Razor.Orm
{
    public abstract class DaoCompositionRoot : DaoGenerator, ICompositionRoot
    {
        private IServiceRegistry serviceRegistry;

        public void Compose(IServiceRegistry serviceRegistry)
        {
            this.serviceRegistry = serviceRegistry;

            if (SqlConnectionLifetime != null)
            {
                this.serviceRegistry.Register(CreateSqlConnection, SqlConnectionLifetime);
            }
            else
            {
                this.serviceRegistry.Register(CreateSqlConnection);
            }

            this.serviceRegistry.Register(f => Transformers);
            Generate();
        }

        protected abstract SqlConnection CreateSqlConnection(IServiceFactory serviceFactory);

        protected virtual ILifetime SqlConnectionLifetime
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
