using Hardware.domain.Entities;
using Hardware.infrastructure.Data;
using Hardware.infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// Add services to the container with EnableRetryOnFailure to prevent transient database connection exceptions.
builder.Services.AddDbContext<MasterCoreErpDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MasterErp"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

builder.Services.AddControllers();

builder.Services.AddDbContext<TenantErpDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("TenantErp"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

builder.Services.AddScoped<ITenantDatabaseResolver, TenantDatabaseResolver>();
builder.Services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// =========================================================================
// MASTER DB ENDPOINTS
// =========================================================================
app.MapPost("/companies", async (
    Company company,
    MasterCoreErpDbContext db) =>
{
    db.Companies.Add(company);
    await db.SaveChangesAsync();

    return Results.Created($"/companies/{company.CompanyId}", company);
});

app.MapPost("/devices", async (Device device, MasterCoreErpDbContext db) =>
{
    db.Devices.Add(device);
    await db.SaveChangesAsync();

    return Results.Created($"/devices/{device.DeviceId}", device);
});

app.MapPost("/company-databases", async (CompanyDatabase companyDatabase, MasterCoreErpDbContext db) =>
{
    db.CompanyDatabases.Add(companyDatabase);
    await db.SaveChangesAsync();

    return Results.Created($"/company-databases/{companyDatabase.CompanyDatabaseId}", companyDatabase);
});

app.MapGet("/test-tenant/{companyId:int}", async (int companyId, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var productCount = await tenantDb.Products.CountAsync();
    return Results.Ok(new { companyId, productCount });
});

// =========================================================================
// TENANT CRM ENDPOINTS
// =========================================================================
app.MapPost("/tenant/{companyId:int}/customers", async (
    int companyId,
    Customer customer,
    ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);

    tenantDb.Customers.Add(customer);
    await tenantDb.SaveChangesAsync();

    return Results.Created($"/tenant/{companyId}/customers/{customer.CustomerId}", customer);
});

app.MapGet("/tenant/{companyId:int}/customers", async (
    int companyId,
    ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);

    var customers = await tenantDb.Customers
        .AsNoTracking()
        .OrderBy(x => x.CustomerId)
        .ToListAsync();

    return Results.Ok(customers);
});

app.MapPost("/tenant/{companyId:int}/suppliers", async (
    int companyId,
    Supplier supplier,
    ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);

    tenantDb.Suppliers.Add(supplier);
    await tenantDb.SaveChangesAsync();

    return Results.Created($"/tenant/{companyId}/suppliers/{supplier.SupplierId}", supplier);
});

app.MapGet("/tenant/{companyId:int}/suppliers", async (
    int companyId,
    ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);

    var suppliers = await tenantDb.Suppliers
        .AsNoTracking()
        .OrderBy(x => x.SupplierId)
        .ToListAsync();

    return Results.Ok(suppliers);
});

// ==========================================
// 1. UNIFIED INVENTORY & PRODUCT MODULE
// ==========================================

// GET: Unified Inventory & Product List
app.MapGet("/tenant/{companyId:int}/inventory", async (int companyId, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var inventory = await tenantDb.Inventories
        .Include(i => i.Product)
        .AsNoTracking()
        .Select(i => new {
            i.InventoryId,
            i.ProductId,
            ProductName = i.Product != null ? i.Product.ProductName : "Unknown",
            ProductCode = i.Product != null ? i.Product.ProductCode : "",
            UnitPrice = i.Product != null ? i.Product.UnitPrice : 0,
            i.QuantityOnHand,
            i.ReorderLevel,
            IsLowStock = i.QuantityOnHand <= i.ReorderLevel,
            i.LastUpdatedAt
        })
        .ToListAsync();

    return Results.Ok(inventory);
});

// POST: Create Product AND Auto-Create Empty Inventory
app.MapPost("/tenant/{companyId:int}/products", async (int companyId, Product product, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);

    tenantDb.Products.Add(product);
    await tenantDb.SaveChangesAsync();

    // Auto-create stock tracker
    tenantDb.Inventories.Add(new Inventory
    {
        ProductId = product.ProductId,
        QuantityOnHand = 0,
        ReorderLevel = 5,
        LastUpdatedAt = DateTime.UtcNow
    });
    await tenantDb.SaveChangesAsync();

    return Results.Created($"/tenant/{companyId}/products/{product.ProductId}", product);
});

// PUT: Update Product Details & Reorder Level
app.MapPut("/tenant/{companyId:int}/products/{productId:int}", async (int companyId, int productId, Product updatedProduct, decimal reorderLevel, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var product = await tenantDb.Products.FindAsync(productId);
    var inventory = await tenantDb.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);

    if (product == null || inventory == null) return Results.NotFound();

    product.ProductCode = updatedProduct.ProductCode;
    product.ProductName = updatedProduct.ProductName;
    product.UnitPrice = updatedProduct.UnitPrice;
    inventory.ReorderLevel = reorderLevel;

    await tenantDb.SaveChangesAsync();
    return Results.Ok();
});

// DELETE: Remove Product and Inventory
app.MapDelete("/tenant/{companyId:int}/products/{productId:int}", async (int companyId, int productId, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var product = await tenantDb.Products.FindAsync(productId);
    var inventory = await tenantDb.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);

    if (product != null) tenantDb.Products.Remove(product);
    if (inventory != null) tenantDb.Inventories.Remove(inventory);

    await tenantDb.SaveChangesAsync();
    return Results.Ok();
});

// Stock In / Adjust Stock
app.MapPost("/tenant/{companyId:int}/inventory/adjust", async (
    int companyId,
    int productId,
    decimal quantity,
    decimal reorderLevel,
    ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);

    // 1. Check if the Product exists
    var productExists = await tenantDb.Products.AnyAsync(p => p.ProductId == productId);
    if (!productExists)
    {
        return Results.NotFound(new { Message = $"Product ID {productId} does not exist in Tenant {companyId} database." });
    }

    // 2. Fetch or create the Inventory record
    var inventory = await tenantDb.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
    if (inventory == null)
    {
        inventory = new Inventory
        {
            ProductId = productId,
            QuantityOnHand = quantity,
            ReorderLevel = reorderLevel,
            LastUpdatedAt = DateTime.UtcNow
        };
        tenantDb.Inventories.Add(inventory);
    }
    else
    {
        inventory.QuantityOnHand += quantity;
        inventory.ReorderLevel = reorderLevel;
        inventory.LastUpdatedAt = DateTime.UtcNow;
    }

    await tenantDb.SaveChangesAsync();
    return Results.Ok(inventory);
});

// ==========================================
// 2. SALES MODULE ENDPOINTS (Auto Stock Deduction)
// ==========================================

app.MapPost("/tenant/{companyId:int}/sales", async (
    int companyId,
    Sale saleRequest,
    ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    await using var transaction = await tenantDb.Database.BeginTransactionAsync();

    try
    {
        decimal grandTotal = 0;

        foreach (var item in saleRequest.SaleItems)
        {
            var inventory = await tenantDb.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

            if (inventory == null || inventory.QuantityOnHand < item.Quantity)
            {
                return Results.BadRequest(new
                {
                    Message = $"Insufficient inventory stock for Product ID: {item.ProductId}. Available: {(inventory?.QuantityOnHand ?? 0)}"
                });
            }

            // Deduct inventory stock
            inventory.QuantityOnHand -= item.Quantity;
            inventory.LastUpdatedAt = DateTime.UtcNow;

            item.SubTotal = item.Quantity * item.UnitPrice;
            grandTotal += item.SubTotal;
        }

        saleRequest.TotalAmount = grandTotal;
        saleRequest.SaleDate = DateTime.UtcNow;

        tenantDb.Sales.Add(saleRequest);
        await tenantDb.SaveChangesAsync();
        await transaction.CommitAsync();

        return Results.Created($"/tenant/{companyId}/sales/{saleRequest.SaleId}", saleRequest);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return Results.Problem($"Failed to process sale: {ex.Message}");
    }
});

// ==========================================
// 3. REPORTS MODULE ENDPOINTS
// ==========================================

app.MapGet("/tenant/{companyId:int}/reports/sales-summary", async (
    int companyId,
    DateTime? startDate,
    DateTime? endDate,
    ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);

    var start = startDate ?? DateTime.UtcNow.AddDays(-30);
    var end = endDate ?? DateTime.UtcNow;

    // 1. Build base query for active sales within the date range
    var salesQuery = tenantDb.Sales
        .AsNoTracking()
        .Where(s => s.SaleDate >= start && s.SaleDate <= end && s.IsActive);

    // 2. Calculate aggregated metrics (declaring local variables)
    var totalRevenue = await salesQuery.SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
    var totalTransactions = await salesQuery.CountAsync();

    // 3. Query top selling products
    var topProducts = await tenantDb.SaleItems
        .AsNoTracking()
        .Where(si => si.Sale != null && si.Sale.SaleDate >= start && si.Sale.SaleDate <= end)
        .GroupBy(si => new { si.ProductId, si.Product!.ProductName })
        .Select(g => new {
            ProductId = g.Key.ProductId,
            ProductName = g.Key.ProductName,
            TotalQuantitySold = g.Sum(x => x.Quantity),
            TotalRevenue = g.Sum(x => x.SubTotal)
        })
        .OrderByDescending(x => x.TotalRevenue)
        .Take(5)
        .ToListAsync();

    // 4. Return the calculated values
    return Results.Ok(new
    {
        TenantId = companyId,
        StartDate = start,
        EndDate = end,
        TotalRevenue = totalRevenue,
        TotalTransactions = totalTransactions,
        TopSellingProducts = topProducts
    });
});

app.MapPost("/auth/login", async (LoginRequest request, MasterCoreErpDbContext db) =>
{
    string email = request.Email.Trim().ToLower();

    if (request.Password != "123123")
    {
        return Results.Unauthorized();
    }

    int companyId = email switch
    {
        "tenant1@email" => 1,
        "tenant2@email" => 2,
        "tenant3@email" => 3,
        _ => 0
    };

    if (companyId == 0)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new
    {
        CompanyId = companyId,
        CompanyName = $"Tenant {companyId} (Company)"
    });
});

app.MapControllers();
app.Run();

public record LoginRequest(string Email, string Password);