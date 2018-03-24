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

        public DaoCompositionRoot(Func<IServiceFactory, SqlConnection> createSqlConnection, ILifetime lifetime = null)
        {
            assembly = Assembly.GetCallingAssembly();
            CreateSqlConnection = createSqlConnection;
            Lifetime = lifetime;
        }

        private Func<IServiceFactory, SqlConnection> CreateSqlConnection { get; }
        private ILifetime Lifetime { get; }

        public void Compose(IServiceRegistry serviceRegistry)
        {
            this.serviceRegistry = serviceRegistry;

            if (Lifetime != null)
            {
                this.serviceRegistry.Register(CreateSqlConnection, Lifetime);
            }
            else
            {
                this.serviceRegistry.Register(CreateSqlConnection, Lifetime);
            }

            this.serviceRegistry.Register(f => Transformers);
            Generate(assembly);
        }

        protected override void GeneratedDao(Type interfaceType, Type generatedType)
        {
            serviceRegistry.Register(interfaceType, generatedType);
        }
    }
}
