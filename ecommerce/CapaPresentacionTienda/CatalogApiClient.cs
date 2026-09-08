using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using CapaEntidad;
using Newtonsoft.Json;

namespace CapaPresentacionTienda
{
    public class CatalogApiClient
    {
        private static readonly HttpClient Client = new HttpClient();

        static CatalogApiClient()
        {
            Client.Timeout = TimeSpan.FromSeconds(30);
        }

        private static void EnsureConfigured()
        {
            if (Client.BaseAddress != null)
                return;

            var baseUrl = ConfigurationManager.AppSettings["CatalogApi:BaseUrl"];
            Uri uri;
            if (string.IsNullOrWhiteSpace(baseUrl) ||
                !Uri.TryCreate(baseUrl.TrimEnd('/') + "/", UriKind.Absolute, out uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ConfigurationErrorsException(
                    "CatalogApi:BaseUrl debe contener una URL HTTP o HTTPS absoluta cuando Features:UseCatalogApi está habilitado.");
            }

            Client.BaseAddress = uri;
        }

        public async Task<List<Categoria>> ListarCategoriasAsync(bool activo)
        {
            EnsureConfigured();
            var response = await Client.GetAsync($"api/v1/categories?isActive={GetLower(activo)}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var dtos = JsonConvert.DeserializeObject<List<CategoryDto>>(json) ?? new List<CategoryDto>();
            return dtos.ConvertAll(d => new Categoria { IdCategoria = d.Id, Descripcion = d.Description, Activo = d.IsActive });
        }

        public async Task<List<Marca>> ListarMarcasPorCategoriaAsync(int idcategoria)
        {
            EnsureConfigured();
            var query = new List<string> { "isActive=true" };
            if (idcategoria > 0) query.Add($"categoryId={idcategoria}");
            var response = await Client.GetAsync($"api/v1/brands?{string.Join("&", query)}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var dtos = JsonConvert.DeserializeObject<List<BrandDto>>(json) ?? new List<BrandDto>();
            return dtos.ConvertAll(d => new Marca { IdMarca = d.Id, Descripcion = d.Description, Activo = d.IsActive });
        }

        public async Task<List<Producto>> ListarProductosAsync(int idcategoria, int idmarca)
        {
            EnsureConfigured();
            var query = new List<string> { "isActive=true" };
            if (idcategoria > 0) query.Add($"categoryId={idcategoria}");
            if (idmarca > 0) query.Add($"brandId={idmarca}");
            var response = await Client.GetAsync($"api/v1/products?{string.Join("&", query)}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var dtos = JsonConvert.DeserializeObject<List<ProductDto>>(json) ?? new List<ProductDto>();
            var productos = dtos.ConvertAll(MapProduct);
            productos.RemoveAll(p => p.Stock <= 0);
            return productos;
        }

        public async Task<Producto> ObtenerProductoAsync(int idproducto)
        {
            EnsureConfigured();
            var response = await Client.GetAsync($"api/v1/products/{idproducto}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonConvert.DeserializeObject<ProductDto>(json);
            return dto == null ? null : MapProduct(dto);
        }

        private static Producto MapProduct(ProductDto d)
        {
            return new Producto
            {
                IdProducto = d.Id,
                Nombre = d.Name,
                Descripcion = d.Description,
                oMarca = d.Brand == null ? null : new Marca { IdMarca = d.Brand.Id, Descripcion = d.Brand.Description, Activo = d.Brand.IsActive },
                oCategoria = d.Category == null ? null : new Categoria { IdCategoria = d.Category.Id, Descripcion = d.Category.Description, Activo = d.Category.IsActive },
                Precio = d.Price,
                Stock = d.Stock,
                Activo = d.IsActive
            };
        }

        private static string GetLower(bool value) => value ? "true" : "false";

        private class CategoryDto
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("description")] public string Description { get; set; }
            [JsonProperty("isActive")] public bool IsActive { get; set; }
        }

        private class BrandDto
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("description")] public string Description { get; set; }
            [JsonProperty("isActive")] public bool IsActive { get; set; }
        }

        private class ProductDto
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("description")] public string Description { get; set; }
            [JsonProperty("brand")] public BrandDto Brand { get; set; }
            [JsonProperty("category")] public CategoryDto Category { get; set; }
            [JsonProperty("price")] public decimal Price { get; set; }
            [JsonProperty("stock")] public int Stock { get; set; }
            [JsonProperty("isActive")] public bool IsActive { get; set; }
        }
    }
}
