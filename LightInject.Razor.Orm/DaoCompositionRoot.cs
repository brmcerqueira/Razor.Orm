using Razor.Orm;
using System;
using System.Data.SqlClient;
using System.Reflection;

namespace LightInject.Razor.Orm
{
    public abstract class DaoCompositionRoot : DaoGenerator, ICompositionRoot
    {
        private Assembly assembly;
        private IServiceRegistry serviceRegistry;

        public DaoCompositionRoot(ILifetime sqlConnectionlifetime = null)
        {
            assembly = GetType().Assembly;
            SqlConnectionlifetime = sqlConnectionlifetime;
        }

        private ILifetime SqlConnectionlifetime { get; }

        public void Compose(IServiceRegistry serviceRegistry)
        {
            this.serviceRegistry = serviceRegistry;

            if (SqlConnectionlifetime != null)
            {
                this.serviceRegistry.Register(CreateSqlConnection, SqlConnectionlifetime);
            }
            else
            {
                this.serviceRegistry.Register(CreateSqlConnection);
            }

            this.serviceRegistry.Register(f => Transformers);
            Generate(assembly);
        }

        protected abstract SqlConnection CreateSqlConnection(IServiceFactory serviceFactory);

        protected override void GeneratedDao(Type interfaceType, Type generatedType)
        {
            serviceRegistry.Register(interfaceType, generatedType);
        }
    }
}
