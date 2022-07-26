﻿using MongoDB.Driver;
using Rest.Products.DataAccess;
using Rest.Products.Models.Product;

namespace Rest.Products.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAll(int? page, int? size, Guid[]? categoryIds = null);
    
    Task<Product?> Get(Guid id);

    Task<Product> Create(CreateProduct product);

    Task<Product> Update(Guid id, UpdateProduct product);

    Task Delete(Guid id);
}

public class ProductService : IProductService
{
    private readonly IMongoCollection<Product> _products;

    public ProductService(MongoContext context)
    {
        _products = context.Collection<Product>();
    }

    public async Task<IEnumerable<Product>> GetAll(int? page, int? size, Guid[]? categoryIds = null)
    {
        var filter = Builders<Product>.Filter.Empty;

        if (categoryIds is not null && categoryIds.Any())
        {
            filter = Builders<Product>.Filter.In(x => x.CategoryId, categoryIds);
        }

        var query = _products.Find(filter);

        if (page is > 0 && size.HasValue)
        {
            var skip = (page - 1) * size;
            query = query
                .Skip(skip.Value)
                .Limit(size.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<Product?> Get(Guid id)
    {
        return await _products.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Product> Create(CreateProduct product)
    {
        var newProduct = new Product(Guid.NewGuid(), product.Name, product.CategoryId);
        await _products.InsertOneAsync(newProduct);
        return newProduct;
    }

    public async Task<Product> Update(Guid id, UpdateProduct product)
    {
        var newProduct = new Product(id, product.Name, product.CategoryId);
        await _products.ReplaceOneAsync(x => x.Id == id, newProduct);
        return newProduct;
    }

    public async Task Delete(Guid id)
    {
        await _products.DeleteOneAsync(x => x.Id == id);
    }
}