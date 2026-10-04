using Moq;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Plugin.Misc.Expenses.Domain;
using Nop.Plugin.Misc.Expenses.Services;
using Nop.Services.Stores;
using Xunit;

namespace Nop.Plugin.Misc.Expenses.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ExpenseCategoryService"/> (AC3.2, AC3.6-AC3.9)
/// </summary>
public class ExpenseCategoryServiceTests
{
    private sealed class Fixture
    {
        public List<ExpenseCategory> Data { get; }
        public Mock<IRepository<ExpenseCategory>> Repository { get; } = new();
        public Mock<IStaticCacheManager> Cache { get; } = new();
        public Mock<IRepository<StoreMapping>> Mappings { get; } = new();
        public List<StoreMapping> MappingData { get; }
        public ExpenseCategoryService Service { get; }

        public Fixture(List<ExpenseCategory> data, List<StoreMapping> mappings = null)
        {
            Data = data;
            MappingData = mappings ?? new List<StoreMapping>();
            Mappings.Setup(r => r.Table).Returns(() => MappingData.AsQueryable());
            Repository.Setup(r => r.Table).Returns(() => Data.AsQueryable());
            Repository.Setup(r => r.InsertAsync(It.IsAny<ExpenseCategory>(), It.IsAny<bool>())).Returns(Task.CompletedTask);
            Repository.Setup(r => r.UpdateAsync(It.IsAny<ExpenseCategory>(), It.IsAny<bool>())).Returns(Task.CompletedTask);
            Cache.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<object[]>())).Returns(Task.CompletedTask);

            //the service filters by store with its own StoreMapping query and takes no store-mapping service or
            //CatalogSettings dependency, so CatalogSettings.IgnoreStoreLimitations cannot influence the result (BUG-010)
            Service = new ExpenseCategoryService(Repository.Object, Cache.Object, Mappings.Object);
        }
    }

    private static List<ExpenseCategory> Categories(params string[] names) =>
        names.Select((n, i) => new ExpenseCategory { Id = i + 1, Name = n, ExpenseCategoryType = ExpenseCategoryType.Expense }).ToList();

    [Fact]
    public async Task GetAll_PageSizeLargerThanMax_IsClampedTo100()
    {
        var data = Enumerable.Range(1, 150).Select(i => new ExpenseCategory { Id = i, Name = $"C{i:000}" }).ToList();
        var f = new Fixture(data);

        var page = await f.Service.GetAllExpenseCategoriesAsync(pageSize: 5000);

        Assert.Equal(100, page.Count);
        Assert.Equal(100, page.PageSize);
        Assert.Equal(150, page.TotalCount);
    }

    [Fact]
    public async Task GetAll_DefaultPageSize_IsClampedTo100()
    {
        var data = Enumerable.Range(1, 120).Select(i => new ExpenseCategory { Id = i, Name = $"C{i:000}" }).ToList();
        var f = new Fixture(data);

        var page = await f.Service.GetAllExpenseCategoriesAsync();

        Assert.Equal(100, page.Count);
    }

    [Fact]
    public async Task GetAll_SecondPage_ReturnsRemainingRows()
    {
        var data = Enumerable.Range(1, 130).Select(i => new ExpenseCategory { Id = i, Name = $"C{i:000}" }).ToList();
        var f = new Fixture(data);

        var page = await f.Service.GetAllExpenseCategoriesAsync(pageIndex: 1, pageSize: 100);

        Assert.Equal(30, page.Count);
    }

    [Fact]
    public async Task GetAll_NoCategories_ReturnsEmptyPage()
    {
        var f = new Fixture(new List<ExpenseCategory>());

        var page = await f.Service.GetAllExpenseCategoriesAsync();

        Assert.Empty(page);
        Assert.Equal(0, page.TotalCount);
    }

    [Fact]
    public async Task GetAll_OrdersByNameThenId()
    {
        var data = new List<ExpenseCategory>
        {
            new() { Id = 3, Name = "Beta" },
            new() { Id = 2, Name = "Alpha" },
            new() { Id = 1, Name = "Beta" }
        };
        var f = new Fixture(data);

        var page = await f.Service.GetAllExpenseCategoriesAsync();

        Assert.Equal(new[] { 2, 1, 3 }, page.Select(c => c.Id));
    }

    [Fact]
    public async Task GetAll_NameFilter_IsTrimmedAndCaseInsensitive()
    {
        var f = new Fixture(Categories("Office Supplies", "Travel", "supplies and more"));

        var page = await f.Service.GetAllExpenseCategoriesAsync("  SUPPLIES  ");

        Assert.Equal(2, page.Count);
        Assert.DoesNotContain(page, c => c.Name == "Travel");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetAll_EmptyName_ListsAll(string name)
    {
        var f = new Fixture(Categories("A", "B", "C"));

        var page = await f.Service.GetAllExpenseCategoriesAsync(name);

        Assert.Equal(3, page.Count);
    }

    private static StoreMapping Mapping(int categoryId, int storeId, string entityName = "ExpenseCategory") =>
        new() { EntityId = categoryId, EntityName = entityName, StoreId = storeId };

    /// <summary>
    /// 1 = not limited, 2 = limited and mapped to store 1, 3 = limited and mapped only to store 2,
    /// 4 = limited with no mapping rows, 5 = limited, but a Product mapping row with the same entity id exists for store 1
    /// </summary>
    private static Fixture StoreFixture() => new(
        new List<ExpenseCategory>
        {
            new() { Id = 1, Name = "Free", LimitedToStores = false },
            new() { Id = 2, Name = "Mapped to 1", LimitedToStores = true },
            new() { Id = 3, Name = "Mapped to 2", LimitedToStores = true },
            new() { Id = 4, Name = "Limited, no rows", LimitedToStores = true },
            new() { Id = 5, Name = "Other entity row", LimitedToStores = true }
        },
        new List<StoreMapping>
        {
            Mapping(2, 1), Mapping(3, 2), Mapping(5, 1, "Product"),
            //a not-limited category keeps stale rows for another store; it must still be listed
            Mapping(1, 2)
        });

    [Fact]
    public async Task GetAll_StoreIdZero_ListsEverythingRegardlessOfMapping()
    {
        var f = StoreFixture();

        var page = await f.Service.GetAllExpenseCategoriesAsync(storeId: 0);

        Assert.Equal(5, page.Count);
    }

    [Fact]
    public async Task GetAll_StoreChosen_ListsNotLimitedAndMappedToThatStoreOnly()
    {
        var f = StoreFixture();

        var page = await f.Service.GetAllExpenseCategoriesAsync(storeId: 1);

        //AC3.8 / AC5.4: not limited (1) and mapped to store 1 (2); not mapped to 1: 3, 4, and 5 (row belongs to another entity)
        Assert.Equal(new[] { 1, 2 }, page.Select(c => c.Id).OrderBy(i => i));
    }

    [Fact]
    public async Task GetAll_OtherStoreChosen_ListsNotLimitedAndMappedToThatOtherStore()
    {
        var f = StoreFixture();

        var page = await f.Service.GetAllExpenseCategoriesAsync(storeId: 2);

        Assert.Equal(new[] { 1, 3 }, page.Select(c => c.Id).OrderBy(i => i));
    }

    [Fact]
    public async Task GetAll_StoreWithNoMappingsAtAll_ListsOnlyNotLimitedCategories()
    {
        //the stock ApplyStoreMapping would return everything when no rows exist for the entity; this filter must not
        var f = new Fixture(new List<ExpenseCategory>
        {
            new() { Id = 1, Name = "Free", LimitedToStores = false },
            new() { Id = 2, Name = "Limited", LimitedToStores = true }
        });

        var page = await f.Service.GetAllExpenseCategoriesAsync(storeId: 7);

        Assert.Equal(new[] { 1 }, page.Select(c => c.Id));
    }

    [Fact]
    public void Ctor_DoesNotDependOnCatalogSettingsOrStoreMappingService()
    {
        //IgnoreStoreLimitations (on or off) therefore cannot change the store filter result
        var parameters = typeof(ExpenseCategoryService).GetConstructors().Single().GetParameters().Select(p => p.ParameterType).ToList();

        Assert.DoesNotContain(typeof(CatalogSettings), parameters);
        Assert.DoesNotContain(typeof(IStoreMappingService), parameters);
    }

    [Fact]
    public async Task GetAll_NameAndStoreFilters_AreCombined()
    {
        var f = StoreFixture();
        f.Data.Add(new ExpenseCategory { Id = 6, Name = "Free two", LimitedToStores = false });

        var page = await f.Service.GetAllExpenseCategoriesAsync("free", 1);

        //"Free" and "Free two" match both; "Mapped to 1" is in the store but not the name; "Limited, no rows" no store
        Assert.Equal(new[] { 1, 6 }, page.Select(c => c.Id).OrderBy(i => i));
    }

    [Fact]
    public async Task GetAll_StoreFilter_StillClampsPageSizeTo100()
    {
        var data = Enumerable.Range(1, 150).Select(i => new ExpenseCategory { Id = i, Name = $"C{i:000}", LimitedToStores = false }).ToList();
        var f = new Fixture(data);

        var page = await f.Service.GetAllExpenseCategoriesAsync(storeId: 1, pageSize: 5000);

        Assert.Equal(100, page.Count);
        Assert.Equal(150, page.TotalCount);
    }

    [Fact]
    public async Task Insert_SetsBothTimestampsAndClearsCachePrefix()
    {
        var f = new Fixture(new List<ExpenseCategory>());
        var category = new ExpenseCategory { Name = "New" };
        var before = DateTime.UtcNow;

        await f.Service.InsertExpenseCategoryAsync(category);

        Assert.True(category.CreatedOnUtc >= before);
        Assert.Equal(category.CreatedOnUtc, category.UpdatedOnUtc);
        f.Repository.Verify(r => r.InsertAsync(category, It.IsAny<bool>()), Times.Once);
        f.Cache.Verify(c => c.RemoveByPrefixAsync(NopExpensesDefaults.ExpenseCategoriesPatternCacheKey, It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task Update_SetsUpdatedTimestampOnlyAndClearsCachePrefix()
    {
        var f = new Fixture(new List<ExpenseCategory>());
        var created = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var category = new ExpenseCategory { Id = 1, Name = "X", CreatedOnUtc = created, UpdatedOnUtc = created };

        await f.Service.UpdateExpenseCategoryAsync(category);

        Assert.Equal(created, category.CreatedOnUtc);
        Assert.True(category.UpdatedOnUtc > created);
        f.Repository.Verify(r => r.UpdateAsync(category, It.IsAny<bool>()), Times.Once);
        f.Cache.Verify(c => c.RemoveByPrefixAsync(NopExpensesDefaults.ExpenseCategoriesPatternCacheKey, It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public void Service_HasNoDeleteMethod()
    {
        Assert.DoesNotContain(typeof(IExpenseCategoryService).GetMethods(), m => m.Name.Contains("Delete"));
    }
}
