Razor.Orm - Um micro ORM baseado no Razor
========================================

Para baixar e instalar você deve acessar o link abaixo:

[Razor.Orm NuGet library](https://www.nuget.org/packages/Razor.Orm)

Para uma melhor integração com o LightInject foi feito o DaoCompositionRoot onde você pode baixar e instalar acessando o link abaixo:

[LightInject.Razor.Orm NuGet library](https://www.nuget.org/packages/LightInject.Razor.Orm)

Primeiros passos
========================================

Para comerçar é necessario montar uma estrutura de camada de persistencia, precisamos criar classes Dto(classes POCO) que serão responsaveis pelo trafego de dados. 

```csharp
    public class PeopleFilterDto
    {
        public string LikeFirstName { get; set; }
        public long[] EmailPromotionOptions { get; set; }
    }

    public class PeopleDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string FirstName { get; set; }
    }
```

Em seguida devemos criar uma interface que contenha o layout que será implementado pelo Razor.Orm para aquele determinado Dao

```csharp
    public interface IPeopleDao
    {
        IEnumerable<PeopleDto> GetAllPeople(PeopleFilterDto dto);
    }
```

Agora criaremos um arquivo 'cshtml' para cada metodo que foi definido na interface anteriormente, é necessario que esses arquivos fiquem no mesmo diretorio onde está a interface e que a 'Ação de Compilação' seja ajustada para opção 'Recurso inserido'

```cshtml
@using Razor.Orm.Template
@using Razor.Orm.Example.Dto
@inherits SqlTemplate<PeopleFilterDto, PeopleDto>
SELECT [BusinessEntityID] @As(e => e.Id)
      ,[FirstName]
      ,[ModifiedDate] @As(e => e.Date)
    FROM [Person].[Person]
    @using (Smart.Where)
    {
        if (!string.IsNullOrEmpty(Model.LikeFirstName))
        {
            <text>[FirstName] LIKE @Par($"%{Model.LikeFirstName}%")</text>
        }
        if (Model.EmailPromotionOptions != null && Model.EmailPromotionOptions.Length > 0)
        {
            Smart.And();
            <text>[EmailPromotion] @In(Model.EmailPromotionOptions)</text>
        }
    }
```

Nesse momento é necessario extender a classe DaoFactory criando sua propia fabrica de Daos

```csharp
    public class MyDaoFactory : DaoFactory
    {
        protected override void Setup()
        {
            Define<IPeopleDao>();
        }
    }
```