# Entity Relationship Diagram (ERD) - Small Enterprise ERP System

```
[Company (Tenants)] 1 --- * [User]
[Company (Tenants)] 1 --- * [Product]
[Product]           1 --- 1 [Inventory]
[Product]           1 --- * [StockBox]
[Product]           1 --- * [StockAuditRequest]
[Product]           1 --- * [SaleItem]
[Sale]              1 --- * [SaleItem]
[Customer]          1 --- * [Sale]
[Supplier]          1 --- * [PurchaseOrder]
[PurchaseOrder]     1 --- * [PurchaseOrderItem]
[Company]           1 --- 1 [TermsAndConditions]
```

## Entity Specifications

### 1. Company (Master & Tenant Context)
- `CompanyId` (PK, int)
- `CompanyCode` (string)
- `CompanyName` (string)
- `IsActive` (bool)
- `CreatedAt` (DateTime)

### 2. User
- `UserId` (PK, int)
- `CompanyId` (FK, int)
- `Email` (string, unique per tenant, format: `{role}{tenant_id}@email`, e.g., `owner1@email`)
- `Role` (string: Super Admin, Owner, HR Manager, Branch Manager, Cashier, Inventory Staff)
- `FullName` (string)
- `Password` (string, default: `123123`)
- `IsActive` (bool)

### 3. Product
- `ProductId` (PK, int)
- `ProductCode` (string)
- `ProductName` (string)
- `UnitPrice` (decimal)
- `IsActive` (bool)
- `CreatedAt` (DateTime)

### 4. Inventory
- `InventoryId` (PK, int)
- `ProductId` (FK, int)
- `QuantityOnHand` (decimal)
- `ReorderLevel` (decimal)
- `LastUpdatedAt` (DateTime)

### 5. StockBox
- `StockBoxId` (PK, int)
- `ProductId` (FK, int)
- `BoxNumber` (string)
- `UnitsPerBox` (decimal)
- `IsConverted` (bool)
- `ConvertedAt` (DateTime?)

### 6. StockAuditRequest
- `StockAuditRequestId` (PK, int)
- `ProductId` (FK, int)
- `PhysicalCount` (decimal)
- `SystemCount` (decimal)
- `Reason` (string)
- `Status` (string: Pending, Validated, Rejected)
- `RequestedBy` (string)
- `ValidatedBy` (string?)
- `CreatedAt` (DateTime)

### 7. Sale & SaleItem
- `Sale`: `SaleId` (PK, int), `InvoiceNumber` (string), `CustomerId` (FK, nullable int), `TotalAmount` (decimal), `SaleDate` (DateTime), `IsActive` (bool)
- `SaleItem`: `SaleItemId` (PK, int), `SaleId` (FK, int), `ProductId` (FK, int), `Quantity` (decimal), `UnitPrice` (decimal), `SubTotal` (decimal)

### 8. PayrollRecord
- `PayrollId` (PK, int)
- `EmployeeId` (int)
- `EmployeeName` (string)
- `Role` (string)
- `BaseSalary` (decimal)
- `Bonuses` (decimal)
- `Deductions` (decimal)
- `NetPay` (decimal)
- `PayPeriod` (string)
- `Status` (string: Pending, Processed, Paid)
- `ProcessedAt` (DateTime)

### 9. PurchaseOrder & PurchaseOrderItem
- `PurchaseOrder`: `PurchaseOrderId` (PK, int), `OrderNumber` (string), `SupplierId` (FK, int), `SupplierName` (string), `TotalAmount` (decimal), `Status` (string: Draft, Validated, Funds Approved, Received, Cancelled), `CreatedAt` (DateTime)
- `PurchaseOrderItem`: `PurchaseOrderItemId` (PK, int), `PurchaseOrderId` (FK, int), `ProductId` (FK, int), `ProductName` (string), `Quantity` (decimal), `UnitCost` (decimal), `SubTotal` (decimal)

### 10. TermsAndConditions
- `TermsAndConditionsId` (PK, int)
- `CompanyId` (int)
- `StoreRules` (string)
- `ReturnPolicy` (string)
- `WarrantyTerms` (string)
- `UpdatedAt` (DateTime)
