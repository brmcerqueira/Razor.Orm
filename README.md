Razor.Orm - Um micro ORM baseado no Razor
========================================

Para baixar e instalar voc� deve acessar o link abaixo:

[Razor.Orm NuGet library](https://www.nuget.org/packages/Razor.Orm)

Para uma melhor integra��o com o LightInject foi feito o DaoCompositionRoot onde voc� pode baixar e instalar acessando o link abaixo:

[LightInject.Razor.Orm NuGet library](https://www.nuget.org/packages/LightInject.Razor.Orm)

Primeiros passos
========================================

Para comer�ar � necessario montar uma estrutura de camada de persistencia, precisamos criar classes Dto(classes POCO) que ser�o responsaveis pelo trafego de dados. 

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

Em seguida devemos criar uma interface que contenha o layout que ser� implementado pelo Razor.Orm para aquele determinado Dao

```csharp
    public interface IPeopleDao
    {
        IEnumerable<PeopleDto> GetAllPeople(PeopleFilterDto dto);
    }
```

Agora criaremos um arquivo 'cshtml' para cada metodo que foi definido na interface anteriormente, � necessario que esses arquivos fiquem no mesmo diretorio onde est� a interface e que a 'A��o de Compila��o' seja ajustada para op��o 'Recurso inserido'

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

Nesse momento � necessario extender a classe DaoFactory criando sua propia fabrica de Daos

```csharp
    public class MyDaoFactory : DaoFactory
    {
        protected override void Setup()
        {
            Define<IPeopleDao>();
        }
    }
```