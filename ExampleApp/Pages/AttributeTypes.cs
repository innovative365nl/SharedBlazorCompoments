namespace ExampleApp.Pages;

// Demo types and data

public record AttributeTypeModel(int Id, string Value);

public record AttributeModel(Guid Id, string Name, AttributeTypeModel Type, bool IsActive);

public interface IAttributeState
{
    IReadOnlyList<AttributeModel> Attributes { get; }

    IReadOnlyList<AttributeTypeModel> AttributeTypes { get; }

    Task RefreshDataAsync();

    Task<AttributeModel?> GetAttributeByIdAsync(Guid id);

    Task CreateAttributeAsync(AttributeModel model);

    Task UpdateAttributeAsync(AttributeModel model);

    Task DeleteAttributeAsync(Guid id);
}

internal sealed class AttributeState : IAttributeState
{
    private readonly List<AttributeModel> _attributes = [];
    private readonly List<AttributeTypeModel> _attributeTypes = [];

    public IReadOnlyList<AttributeModel> Attributes => _attributes
                                                       .OrderBy(keySelector: attr => attr.Type.Value)
                                                       .ThenBy(keySelector: attr => attr.Name)
                                                       .ToList()
                                                       .AsReadOnly();

    public IReadOnlyList<AttributeTypeModel> AttributeTypes => _attributeTypes
                                                               .OrderBy(keySelector: attributeType => attributeType.Value)
                                                               .ToList()
                                                               .AsReadOnly();

    public async Task RefreshDataAsync()
    {
        await RefreshAttributeTypesAsync().ConfigureAwait(false);
        await RefreshAttributesAsync().ConfigureAwait(false);
    }

    public async Task<AttributeModel?> GetAttributeByIdAsync(Guid id)
    {
        await Task.Delay(millisecondsDelay: 100).ConfigureAwait(continueOnCapturedContext: false);
        return _attributes.FirstOrDefault(predicate: x => x.Id == id);
    }

    public async Task CreateAttributeAsync(AttributeModel model)
    {
        if (model.Id == Guid.Empty && !string.IsNullOrEmpty(value: model.Name))
        {
            _attributes.Add(item: model with
                                  {
                                      Id = Guid.NewGuid()
                                  });
            await Task.Delay(millisecondsDelay: 500).ConfigureAwait(continueOnCapturedContext: false);
        }
    }

    public async Task UpdateAttributeAsync(AttributeModel model)
    {
        AttributeModel? attribute = _attributes.FirstOrDefault(predicate: x => x.Id == model.Id);
        if (attribute == null)
        {
            return;
        }

        _attributes[index: _attributes.IndexOf(item: attribute)] = attribute with
                                                                   {
                                                                       Name = model.Name,
                                                                       Type = model.Type,
                                                                       IsActive = model.IsActive
        };
        await Task.Delay(millisecondsDelay: 500).ConfigureAwait(continueOnCapturedContext: false);
    }

    public async Task DeleteAttributeAsync(Guid id)
    {
        AttributeModel? attribute = _attributes.SingleOrDefault(predicate: x => x.Id == id);
        if (attribute != null)
        {
            _attributes.Remove(item: attribute);
            await Task.Delay(millisecondsDelay: 500).ConfigureAwait(continueOnCapturedContext: false);
        }
    }

    private async ValueTask RefreshAttributeTypesAsync()
    {
        if (_attributeTypes.Count > 0)
        {
            return;
        }

        _attributeTypes
            .AddRange(collection:
                      [
                          new AttributeTypeModel(Id: 10, Value: "CompanyName")
                        , new AttributeTypeModel(Id: 20, Value: "Department")
                        , new AttributeTypeModel(Id: 30, Value: "JobTitle")
                        , new AttributeTypeModel(Id: 40, Value: "Location")
                        , new AttributeTypeModel(Id: 50, Value: "Country")
                        , new AttributeTypeModel(Id: 60, Value: "State")
                        , new AttributeTypeModel(Id: 70, Value: "City")
                        , new AttributeTypeModel(Id: 80, Value: "PostalCode")
                        , new AttributeTypeModel(Id: 90, Value: "UserType")
                        , new AttributeTypeModel(Id: 100, Value: "StreetAddress")
                        , new AttributeTypeModel(Id: 101, Value: "AnniversaryType")
                      ]);

        await Task.Delay(millisecondsDelay: 500).ConfigureAwait(continueOnCapturedContext: false);
    }

    private async ValueTask RefreshAttributesAsync()
    {
        if (_attributes.Count > 0)
        {
            return;
        }

        _attributes.AddRange(collection:
                             [ new AttributeModel(Id: Guid.Parse(input: "32DCF3DB-F1B2-4A93-9AEA-08DC4BF24B47"), Name: "Innovative", Type: _attributeTypes.Single(predicate: x => x.Id          == 10), IsActive: true)
                             , new AttributeModel(Id: Guid.Parse(input: "F1D07FEC-7B7E-425B-9AEB-08DC4BF24B47"), Name: "ODNHN", Type: _attributeTypes.Single(predicate: x => x.Id               == 10), IsActive: true)
                             , new AttributeModel(Id: Guid.Parse(input: "9283F6D1-0C11-4777-9051-08DC5197DFDF"), Name: "Development", Type: _attributeTypes.Single(predicate: x => x.Id         == 20), IsActive: false)
                             , new AttributeModel(Id: Guid.Parse(input: "7F793953-4270-4EEC-9054-08DC5197DFDF"), Name: "Fullstack developer", Type: _attributeTypes.Single(predicate: x => x.Id == 30), IsActive: false)
                             , new AttributeModel(Id: Guid.Parse(input: "2F972E79-56BB-4152-9055-08DC5197DFDF"), Name: "Frontend developer", Type: _attributeTypes.Single(predicate: x => x.Id  == 30), IsActive: true)
                             , new AttributeModel(Id: Guid.Parse(input: "1BB0EC46-E7C3-4C6E-9056-08DC5197DFDF"), Name: "Backend developer", Type: _attributeTypes.Single(predicate: x => x.Id   == 30), IsActive: true)
                             , new AttributeModel(Id: Guid.Parse(input: "3B50D46F-030F-47CF-D7B4-08DCC3490EB6"), Name: "Medewerker", Type: _attributeTypes.Single(predicate: x => x.Id          == 90), IsActive: false)
                             , new AttributeModel(Id: Guid.Parse(input: "08ABE2BC-24DF-4F2F-B706-08DCFF43F0D7"), Name: "Extern", Type: _attributeTypes.Single(predicate: x => x.Id              == 90), IsActive: false)
                             , new AttributeModel(Id: Guid.Parse(input: "D94FF357-98C6-4F48-B707-08DCFF43F0D7"), Name: "Externe", Type: _attributeTypes.Single(predicate: x => x.Id             == 90), IsActive: true)
                             , new AttributeModel(Id: Guid.Parse(input: "D89863CF-026E-45A8-8E7E-08DD448B031A"), Name: "JPO20250206", Type: _attributeTypes.Single(predicate: x => x.Id         == 20), IsActive: true)
                             , new AttributeModel(Id: Guid.Parse(input: "436E3AB1-5C3B-4E31-63B5-08DD66BA4A96"), Name: "Nederland", Type: _attributeTypes.Single(predicate: x => x.Id           == 50), IsActive: false)
                             , new AttributeModel(Id: Guid.Parse(input: "9EDBEF00-D7DD-4F51-63B6-08DD66BA4A96"), Name: "Test", Type: _attributeTypes.Single(predicate: x => x.Id                == 30), IsActive: false)
                             , new AttributeModel(Id: Guid.Parse(input: "DC3E143F-BF40-42E3-9FB0-08DD6C72B6CC"), Name: "Belgie", Type: _attributeTypes.Single(predicate: x => x.Id              == 50), IsActive: true)
                             , new AttributeModel(Id: Guid.Parse(input: "D047AFF8-C291-4776-9FB1-08DD6C72B6CC"), Name: "Sales", Type: _attributeTypes.Single(predicate: x => x.Id               == 30), IsActive: true)
                             ]);

        await Task.Delay(millisecondsDelay: 500).ConfigureAwait(continueOnCapturedContext: false);
    }
}
