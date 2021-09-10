using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Catalog.FunctionalTests
{
    public class CatalogScenarios : CatalogScenariosBase
    {
        [Fact]
        public async Task Get_get_all_catalogitems_and_response_ok_status_code()
        {
            (await GetTestAsync(Get.Items())).EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Get_get_catalogitem_by_id_and_response_ok_status_code()
        {
            (await GetTestAsync(Get.ItemsById(1))).EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Get_get_catalogitem_by_id_and_response_bad_request_status_code()
        {
            var response = await GetTestAsync(Get.ItemsById(int.MinValue));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_get_catalogitem_by_id_and_response_not_found_status_code()
        {
            var response = await GetTestAsync(Get.ItemsById(int.MaxValue));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Get_get_catalogitem_by_name_and_response_ok_status_code()
        {
            (await GetTestAsync(Get.ItemByName(".NET"))).EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Get_get_paginated_catalogitem_by_name_and_response_ok_status_code()
        {
            (await GetTestAsync(Get.ItemByName(".NET", paginated: true))).EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Get_get_paginated_catalog_items_and_response_ok_status_code()
        {
            (await GetTestAsync(Get.Items(paginated: true))).EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Get_get_filtered_catalog_items_and_response_ok_status_code()
        {
            (await GetTestAsync(Get.Filtered(catalogTypeId: 1, catalogBrandId: 1))).EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Get_get_paginated_filtered_catalog_items_and_response_ok_status_code()
        {
            (await GetTestAsync(Get.Filtered(catalogTypeId: 1, catalogBrandId: 1, paginated: true))).EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Get_catalog_types_response_ok_status_code()
        {
            (await GetTestAsync(Get.Types)).EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Get_catalog_brands_response_ok_status_code()
        {
            (await GetTestAsync(Get.Brands)).EnsureSuccessStatusCode();
        }

        private async Task<HttpResponseMessage> GetTestAsync(string apiPath)
        {
            using var server = CreateServer();

            return await server.CreateClient().GetAsync(apiPath);
        }
    }
}