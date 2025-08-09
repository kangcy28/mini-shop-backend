using EcommerceAdminAPI.DTOs;
using EcommerceAdminAPI.Models;
using EcommerceAdminAPI.Repositories;

namespace EcommerceAdminAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IGenericRepository<Category> _categoryRepository;

        public ProductService(IProductRepository productRepository, IGenericRepository<Category> categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetProductsWithCategoryAsync();
            return products.Select(MapToProductDto);
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetProductWithCategoryAsync(id);
            return product != null ? MapToProductDto(product) : null;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            var category = await _categoryRepository.GetByIdAsync(createProductDto.CategoryId);
            if (category == null)
            {
                throw new ArgumentException("Category not found");
            }

            var product = new Product
            {
                Name = createProductDto.Name,
                Price = createProductDto.Price,
                Description = createProductDto.Description,
                CategoryId = createProductDto.CategoryId,
                Stock = createProductDto.Stock,
                IsActive = createProductDto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product);
            await _productRepository.SaveAsync();

            var createdProduct = await _productRepository.GetProductWithCategoryAsync(product.Id);
            return MapToProductDto(createdProduct!);
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return null;
            }

            var category = await _categoryRepository.GetByIdAsync(updateProductDto.CategoryId);
            if (category == null)
            {
                throw new ArgumentException("Category not found");
            }

            product.Name = updateProductDto.Name;
            product.Price = updateProductDto.Price;
            product.Description = updateProductDto.Description;
            product.CategoryId = updateProductDto.CategoryId;
            product.Stock = updateProductDto.Stock;
            product.IsActive = updateProductDto.IsActive;

            _productRepository.Update(product);
            await _productRepository.SaveAsync();

            var updatedProduct = await _productRepository.GetProductWithCategoryAsync(id);
            return MapToProductDto(updatedProduct!);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return false;
            }

            _productRepository.Delete(product);
            await _productRepository.SaveAsync();
            return true;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _productRepository.GetProductsByCategoryAsync(categoryId);
            return products.Select(MapToProductDto);
        }

        public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
        {
            var products = await _productRepository.GetActiveProductsAsync();
            return products.Select(MapToProductDto);
        }

        private static ProductDto MapToProductDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                Stock = product.Stock,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt
            };
        }
    }
}