using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eShopLabs.Services.Catalog.API;
using eShopLabs.Services.Catalog.API.Controllers;
using eShopLabs.Services.Catalog.API.Infrastructure;
using eShopLabs.Services.Catalog.API.IntegrationEvents;
using eShopLabs.Services.Catalog.API.Models;
using eShopLabs.Services.Catalog.API.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Catalog.UnitTests.Application
{
    public class CatalogControllerTest
    {
        private readonly DbContextOptions<CatalogContext> _dbOptions;
        public CatalogControllerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<CatalogContext>()
                .UseInMemoryDatabase(databaseName: "in-memory")
                .Options;

            using var dbContext = new CatalogContext(_dbOptions);

            dbContext.AddRange(GetFakeCatalog());
            dbContext.SaveChanges();
        }

        [Fact]
        public async Task GetCatalogItemsSuccess()
        {
            // arrange
            var brandFilterApplied = 1;
            var typesFilterApplied = 2;
            var pageSize = 4;
            var pageIndex = 1;

            var expectedItemsInPage = 2;
            var expectedTotalItems = 6;

            var catalogContext = new CatalogContext(_dbOptions);
            var catalogSettings = new TestCatalogSettings();

            var integrationServicesMock = new Mock<ICatalogIntegrationEventService>();

            // act
            var catalogController = new CatalogController(catalogContext, catalogSettings, integrationServicesMock.Object);
            var actionResult = await catalogController.ItemsByTypeIdAndBrandIdAsync(typesFilterApplied, brandFilterApplied, pageSize, pageIndex);

            // assert
            Assert.IsType<ActionResult<PaginatedItemsViewModel<CatalogItem>>>(actionResult);

            var page = Assert.IsAssignableFrom<PaginatedItemsViewModel<CatalogItem>>(actionResult.Value);

            Assert.Equal(expectedTotalItems, page.Count);
            Assert.Equal(pageIndex, page.PageIndex);
            Assert.Equal(pageSize, page.PageSize);
            Assert.Equal(expectedItemsInPage, page.Data.Count());
        }

        private List<CatalogItem> GetFakeCatalog()
        {
            return new List<CatalogItem>()
            {
                new CatalogItem()
                {
                    Id = 1,
                    Name = "fakeItemA",
                    CatalogTypeId = 2,
                    CatalogBrandId = 1,
                    PictureFileName = "fakeItemA.png"
                },
                new CatalogItem()
                {
                    Id = 2,
                    Name = "fakeItemB",
                    CatalogTypeId = 2,
                    CatalogBrandId = 1,
                    PictureFileName = "fakeItemB.png"
                },
                new CatalogItem()
                {
                    Id = 3,
                    Name = "fakeItemC",
                    CatalogTypeId = 2,
                    CatalogBrandId = 1,
                    PictureFileName = "fakeItemC.png"
                },
                new CatalogItem()
                {
                    Id = 4,
                    Name = "fakeItemD",
                    CatalogTypeId = 2,
                    CatalogBrandId = 1,
                    PictureFileName = "fakeItemD.png"
                },
                new CatalogItem()
                {
                    Id = 5,
                    Name = "fakeItemE",
                    CatalogTypeId = 2,
                    CatalogBrandId = 1,
                    PictureFileName = "fakeItemE.png"
                },
                new CatalogItem()
                {
                    Id = 6,
                    Name = "fakeItemF",
                    CatalogTypeId = 2,
                    CatalogBrandId = 1,
                    PictureFileName = "fakeItemF.png"
                }
            };
        }
    }

    internal class TestCatalogSettings : IOptionsSnapshot<CatalogSettings>
    {
        public CatalogSettings Value => new CatalogSettings
        {
            PicBaseUrl = "http://image-server.com/",
            AzureStorageEnabled = true,
        };

        public CatalogSettings Get(string name) => Value;
    }
}