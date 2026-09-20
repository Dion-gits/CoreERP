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
// STORE CONFIGURATION, ROLES, T&CS (UC1, UC2, UC3, UC8, UC9, UC10)
// =========================================================================

// Branches (UC2)
app.MapGet("/tenant/{companyId:int}/branches", async (int companyId, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var branches = await tenantDb.StoreBranches.AsNoTracking().ToListAsync();
    if (!branches.Any())
    {
        branches = new List<StoreBranch>
        {
            new StoreBranch { BranchName = "Main Flagship Branch", Location = "Downtown Center" },
            new StoreBranch { BranchName = "Northside Depot", Location = "North Industrial Hub" },
            new StoreBranch { BranchName = "Southside Retail", Location = "South Commercial Zone" }
        };
        tenantDb.StoreBranches.AddRange(branches);
        await tenantDb.SaveChangesAsync();
    }
    return Results.Ok(branches);
});

app.MapPost("/tenant/{companyId:int}/branches", async (int companyId, StoreBranch branch, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    tenantDb.StoreBranches.Add(branch);
    await tenantDb.SaveChangesAsync();
    return Results.Created($"/tenant/{companyId}/branches/{branch.BranchId}", branch);
});

// Users / Roles (UC3)
app.MapGet("/tenant/{companyId:int}/users", async (int companyId, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var users = await tenantDb.Users.AsNoTracking().ToListAsync();
    return Results.Ok(users);
});

// Store Terms & Conditions (UC8, UC9, UC10)
app.MapGet("/tenant/{companyId:int}/terms", async (int companyId, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var terms = await tenantDb.StoreTerms.FirstOrDefaultAsync();
    if (terms == null)
    {
        terms = new StoreTerms();
        tenantDb.StoreTerms.Add(terms);
        await tenantDb.SaveChangesAsync();
    }
    return Results.Ok(terms);
});

app.MapPut("/tenant/{companyId:int}/terms", async (int companyId, StoreTerms updatedTerms, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var terms = await tenantDb.StoreTerms.FirstOrDefaultAsync();
    if (terms == null)
    {
        terms = updatedTerms;
        tenantDb.StoreTerms.Add(terms);
    }
    else
    {
        terms.ReturnPolicy = updatedTerms.ReturnPolicy;
        terms.CreditRules = updatedTerms.CreditRules;
        terms.GeneralTerms = updatedTerms.GeneralTerms;
    }
    await tenantDb.SaveChangesAsync();
    return Results.Ok(terms);
});

// =========================================================================
// AUTHENTICATION & MULTI-TENANT LOGINS
// =========================================================================
app.MapPost("/auth/tenant-login", async (TenantLoginRequest request) =>
{
    string email = request.TenantEmail.Trim().ToLower();
    if (request.Password != "123123")
    {
        return Results.BadRequest(new { Message = "Invalid password. Default password is 123123." });
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
        return Results.NotFound(new { Message = "Tenant account not found. Valid tenant emails: tenant1@email, tenant2@email, tenant3@email." });
    }

    return Results.Ok(new
    {
        CompanyId = companyId,
        TenantEmail = email,
        CompanyName = $"Tenant {companyId} Enterprise"
    });
});

app.MapPost("/auth/user-login", async (UserLoginRequest request) =>
{
    if (request.Password != "123123")
    {
        return Results.BadRequest(new { Message = "Invalid user password. Default password is 123123." });
    }

    var validRoles = new[] { "Super Admin", "Owner", "HR Manager", "Branch Manager", "Cashier", "Inventory Staff" };
    string role = request.Role.Trim();

    if (!validRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
    {
        return Results.BadRequest(new { Message = "Invalid role specified." });
    }

    return Results.Ok(new
    {
        CompanyId = request.CompanyId,
        UserEmail = request.UserEmail,
        Role = role,
        FullName = string.IsNullOrWhiteSpace(request.FullName) ? $"{role} User" : request.FullName
    });
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
app.MapGet("/tenant/{companyId:int}/inventory", async (int companyId, string? search, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var query = tenantDb.Inventories
        .Include(i => i.Product)
        .AsNoTracking();

    if (!string.IsNullOrWhiteSpace(search))
    {
        string term = search.Trim().ToLower();
        query = query.Where(i => i.Product != null && (i.Product.ProductName.ToLower().Contains(term) || i.Product.ProductCode.ToLower().Contains(term)));
    }

    var inventory = await query
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

    var productExists = await tenantDb.Products.AnyAsync(p => p.ProductId == productId);
    if (!productExists)
    {
        return Results.NotFound(new { Message = $"Product ID {productId} does not exist in Tenant {companyId} database." });
    }

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

// Unit Conversion: Convert Boxes to Products (UC23)
app.MapPost("/tenant/{companyId:int}/inventory/convert-boxes", async (
    int companyId,
    int productId,
    decimal boxesToConvert,
    decimal factorToUnits,
    ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var inventory = await tenantDb.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
    if (inventory == null) return Results.NotFound(new { Message = "Product inventory not found." });

    decimal itemsToAdd = boxesToConvert * factorToUnits;
    inventory.QuantityOnHand += itemsToAdd;
    inventory.LastUpdatedAt = DateTime.UtcNow;

    var conversion = await tenantDb.UnitConversions.FirstOrDefaultAsync(u => u.ProductId == productId);
    if (conversion == null)
    {
        conversion = new UnitConversion
        {
            ProductId = productId,
            UnitName = "Box",
            FactorToUnits = factorToUnits,
            BoxesInStock = 0
        };
        tenantDb.UnitConversions.Add(conversion);
    }
    else
    {
        if (conversion.BoxesInStock >= boxesToConvert)
        {
            conversion.BoxesInStock -= boxesToConvert;
        }
    }

    await tenantDb.SaveChangesAsync();
    return Results.Ok(new { Message = $"Converted {boxesToConvert} boxes into {itemsToAdd} product units.", NewStock = inventory.QuantityOnHand });
});

// Stock Audit Requests & Validation (UC25)
app.MapGet("/tenant/{companyId:int}/stock-audits", async (int companyId, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var audits = await tenantDb.StockAuditRequests.Include(a => a.Product).AsNoTracking().ToListAsync();

    if (!audits.Any())
    {
        audits = new List<StockAuditRequest>
        {
            new StockAuditRequest { ProductId = 1, RequestedBy = "Inventory Clerk", SystemQty = 50, PhysicalQty = 45, VarianceQty = -5, Status = "Pending" }
        };
        tenantDb.StockAuditRequests.AddRange(audits);
        await tenantDb.SaveChangesAsync();
    }

    return Results.Ok(audits);
});

app.MapPost("/tenant/{companyId:int}/stock-audits", async (int companyId, StockAuditRequest auditRequest, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    auditRequest.VarianceQty = auditRequest.PhysicalQty - auditRequest.SystemQty;
    auditRequest.Status = "Pending";
    auditRequest.CreatedAt = DateTime.UtcNow;

    tenantDb.StockAuditRequests.Add(auditRequest);
    await tenantDb.SaveChangesAsync();
    return Results.Created($"/tenant/{companyId}/stock-audits/{auditRequest.AuditRequestId}", auditRequest);
});

app.MapPut("/tenant/{companyId:int}/stock-audits/{auditId:int}/validate", async (int companyId, int auditId, string status, string validatedBy, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var audit = await tenantDb.StockAuditRequests.FindAsync(auditId);
    if (audit == null) return Results.NotFound();

    audit.Status = status;
    audit.ValidatedBy = validatedBy;

    if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
    {
        var inventory = await tenantDb.Inventories.FirstOrDefaultAsync(i => i.ProductId == audit.ProductId);
        if (inventory != null)
        {
            inventory.QuantityOnHand = audit.PhysicalQty;
            inventory.LastUpdatedAt = DateTime.UtcNow;
        }
    }

    await tenantDb.SaveChangesAsync();
    return Results.Ok(audit);
});

// Stock Transfers (UC11, UC12, UC13)
app.MapGet("/tenant/{companyId:int}/stock-transfers", async (int companyId, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var transfers = await tenantDb.StockTransfers.Include(t => t.Product).AsNoTracking().ToListAsync();
    return Results.Ok(transfers);
});

app.MapPost("/tenant/{companyId:int}/stock-transfers", async (int companyId, StockTransfer transfer, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    transfer.TransferNumber = "TRF-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
    transfer.Status = "Requested";
    transfer.CreatedAt = DateTime.UtcNow;

    tenantDb.StockTransfers.Add(transfer);
    await tenantDb.SaveChangesAsync();
    return Results.Created($"/tenant/{companyId}/stock-transfers/{transfer.TransferId}", transfer);
});

app.MapPut("/tenant/{companyId:int}/stock-transfers/{transferId:int}/status", async (int companyId, int transferId, string status, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var transfer = await tenantDb.StockTransfers.FindAsync(transferId);
    if (transfer == null) return Results.NotFound();

    transfer.Status = status;
    await tenantDb.SaveChangesAsync();
    return Results.Ok(transfer);
});

// ==========================================
// 2. SALES MODULE ENDPOINTS
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
// 3. HR PAYROLL, EMPLOYEES & UNPAID EXPENSES (UC4, UC6, UC7, UC14, UC15)
// ==========================================

// Employee Records (UC15)
app.MapGet("/tenant/{companyId:int}/employees", async (int companyId, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var employees = await tenantDb.EmployeeRecords.AsNoTracking().ToListAsync();

    if (!employees.Any())
    {
        employees = new List<EmployeeRecord>
        {
            new EmployeeRecord { FullName = "Alice Smith", Role = "Cashier", BaseSalary = 2500m },
            new EmployeeRecord { FullName = "Bob Jones", Role = "Inventory Staff", BaseSalary = 2800m },
            new EmployeeRecord { FullName = "Charlie Davis", Role = "Branch Manager", BaseSalary = 4500m },
            new EmployeeRecord { FullName = "Diana Prince", Role = "HR Manager", BaseSalary = 4200m }
        };
        tenantDb.EmployeeRecords.AddRange(employees);
        await tenantDb.SaveChangesAsync();
    }

    return Results.Ok(employees);
});

// Unpaid Expenses & Debts (UC6)
app.MapGet("/tenant/{companyId:int}/unpaid-expenses", async (int companyId, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var expenses = await tenantDb.UnpaidExpenses.AsNoTracking().ToListAsync();

    if (!expenses.Any())
    {
        expenses = new List<UnpaidExpense>
        {
            new UnpaidExpense { Title = "Electric & Energy Utility Bill", Category = "Utilities", Amount = 450.00m, DueDate = DateTime.UtcNow.AddDays(7), IsPaid = false },
            new UnpaidExpense { Title = "Warehouse Facility Lease", Category = "Rent", Amount = 2200.00m, DueDate = DateTime.UtcNow.AddDays(12), IsPaid = false }
        };
        tenantDb.UnpaidExpenses.AddRange(expenses);
        await tenantDb.SaveChangesAsync();
    }

    return Results.Ok(expenses);
});

app.MapPost("/tenant/{companyId:int}/unpaid-expenses", async (int companyId, UnpaidExpense expense, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    tenantDb.UnpaidExpenses.Add(expense);
    await tenantDb.SaveChangesAsync();
    return Results.Created($"/tenant/{companyId}/unpaid-expenses/{expense.ExpenseId}", expense);
});

app.MapGet("/tenant/{companyId:int}/payroll", async (int companyId, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var records = await tenantDb.PayrollRecords.AsNoTracking().ToListAsync();

    // Seed dummy payroll records if empty for test convenience
    if (!records.Any())
    {
        records = new List<PayrollRecord>
        {
            new PayrollRecord { EmployeeId = 101, EmployeeName = "Alice Smith", Role = "Cashier", BaseSalary = 2500m, Bonuses = 150m, Deductions = 100m, NetPay = 2550m, PayPeriod = "2025-05", Status = "Pending" },
            new PayrollRecord { EmployeeId = 102, EmployeeName = "Bob Jones", Role = "Inventory Staff", BaseSalary = 2800m, Bonuses = 200m, Deductions = 120m, NetPay = 2880m, PayPeriod = "2025-05", Status = "Pending" },
            new PayrollRecord { EmployeeId = 103, EmployeeName = "Charlie Davis", Role = "Branch Manager", BaseSalary = 4500m, Bonuses = 500m, Deductions = 300m, NetPay = 4700m, PayPeriod = "2025-05", Status = "Pending" }
        };
        tenantDb.PayrollRecords.AddRange(records);
        await tenantDb.SaveChangesAsync();
    }

    return Results.Ok(records);
});

app.MapPost("/tenant/{companyId:int}/payroll/process", async (int companyId, PayrollRecord record, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    record.NetPay = record.BaseSalary + record.Bonuses - record.Deductions;
    record.Status = "Processed";
    record.ProcessedAt = DateTime.UtcNow;

    tenantDb.PayrollRecords.Add(record);
    await tenantDb.SaveChangesAsync();

    return Results.Created($"/tenant/{companyId}/payroll/{record.PayrollId}", record);
});

app.MapPut("/tenant/{companyId:int}/payroll/{payrollId:int}/approve", async (int companyId, int payrollId, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var record = await tenantDb.PayrollRecords.FindAsync(payrollId);
    if (record == null) return Results.NotFound();

    record.Status = "Paid";
    record.ProcessedAt = DateTime.UtcNow;
    await tenantDb.SaveChangesAsync();

    return Results.Ok(record);
});

// ==========================================
// 4. SUPPLIER ORDERS & BUYING ENDPOINTS (UC16, UC17, UC18, UC7)
// ==========================================

app.MapGet("/tenant/{companyId:int}/purchase-orders", async (int companyId, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var orders = await tenantDb.PurchaseOrders.Include(po => po.Items).AsNoTracking().ToListAsync();

    if (!orders.Any())
    {
        orders = new List<PurchaseOrder>
        {
            new PurchaseOrder
            {
                OrderNumber = "PO-2025-001", SupplierId = 1, SupplierName = "Apex Hardware Wholesale", TotalAmount = 1500.00m, Status = "Draft", CreatedAt = DateTime.UtcNow,
                Items = new List<PurchaseOrderItem> { new PurchaseOrderItem { ProductId = 1, ProductName = "Standard Hammer", Quantity = 100, UnitCost = 15.00m, SubTotal = 1500.00m } }
            },
            new PurchaseOrder
            {
                OrderNumber = "PO-2025-002", SupplierId = 2, SupplierName = "BuildRight Corp", TotalAmount = 3200.00m, Status = "Validated", CreatedAt = DateTime.UtcNow,
                Items = new List<PurchaseOrderItem> { new PurchaseOrderItem { ProductId = 2, ProductName = "Screwdriver Set", Quantity = 160, UnitCost = 20.00m, SubTotal = 3200.00m } }
            }
        };
        tenantDb.PurchaseOrders.AddRange(orders);
        await tenantDb.SaveChangesAsync();
    }

    return Results.Ok(orders);
});

app.MapPost("/tenant/{companyId:int}/purchase-orders", async (int companyId, PurchaseOrder order, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    order.OrderNumber = "PO-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
    order.Status = "Draft";
    order.CreatedAt = DateTime.UtcNow;
    order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitCost);

    foreach (var item in order.Items)
    {
        item.SubTotal = item.Quantity * item.UnitCost;
    }

    tenantDb.PurchaseOrders.Add(order);
    await tenantDb.SaveChangesAsync();

    return Results.Created($"/tenant/{companyId}/purchase-orders/{order.PurchaseOrderId}", order);
});

app.MapPut("/tenant/{companyId:int}/purchase-orders/{orderId:int}/status", async (int companyId, int orderId, string status, ITenantDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var order = await tenantDb.PurchaseOrders.Include(p => p.Items).FirstOrDefaultAsync(p => p.PurchaseOrderId == orderId);
    if (order == null) return Results.NotFound();

    order.Status = status;

    // If status changed to Received, adjust stock in inventory automatically
    if (status.Equals("Received", StringComparison.OrdinalIgnoreCase))
    {
        foreach (var item in order.Items)
        {
            var inventory = await tenantDb.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.ProductId);
            if (inventory != null)
            {
                inventory.QuantityOnHand += item.Quantity;
                inventory.LastUpdatedAt = DateTime.UtcNow;
            }
        }
    }

    await tenantDb.SaveChangesAsync();
    return Results.Ok(order);
});

// ==========================================
// 5. REPORTS & DASHBOARD BI ENDPOINTS (UC4, UC6, UC8, UC9, UC10, UC21, UC22, UC23, UC24, UC25)
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

    var salesQuery = tenantDb.Sales
        .AsNoTracking()
        .Where(s => s.SaleDate >= start && s.SaleDate <= end && s.IsActive);

    var totalRevenue = await salesQuery.SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
    var totalTransactions = await salesQuery.CountAsync();

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

app.MapControllers();
app.Run();

public record TenantLoginRequest(string TenantEmail, string Password);
public record UserLoginRequest(int CompanyId, string UserEmail, string Role, string FullName, string Password);
