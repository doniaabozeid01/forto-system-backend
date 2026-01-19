using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Billings;
using Forto.Application.DTOs.Billings.cashier;
using Forto.Application.DTOs.Billings.Gifts;
using Forto.Domain.Entities.Billings;
using Forto.Domain.Entities.Bookings;
using Forto.Domain.Entities.Catalog;
using Forto.Domain.Entities.Employees;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;

namespace Forto.Application.Abstractions.Services.Invoices
{
    public class InvoiceService : IInvoiceService
    {
                
        private readonly IUnitOfWork _uow;


        public InvoiceService(IUnitOfWork uow) => _uow = uow;


        public async Task<InvoiceResponse?> GetByBookingIdAsync(int bookingId)
        {
            var invRepo = _uow.Repository<Invoice>();
            var lineRepo = _uow.Repository<InvoiceLine>();

            var invList = await invRepo.FindAsync(x => x.BookingId == bookingId);
            var inv = invList.FirstOrDefault();
            if (inv == null) return null;

            var lines = await lineRepo.FindAsync(l => l.InvoiceId == inv.Id);
            inv.Lines = lines.ToList();

            return Map(inv);
        }


        //public async Task<InvoiceResponse> EnsureInvoiceForBookingAsync(int bookingId)
        //{
        //    var bookingRepo = _uow.Repository<Booking>();
        //    var invRepo = _uow.Repository<Invoice>();
        //    var lineRepo = _uow.Repository<InvoiceLine>();
        //    var itemRepo = _uow.Repository<BookingItem>();


        //    var booking = await bookingRepo.GetByIdAsync(bookingId);
        //    if (booking == null)
        //        throw new BusinessException("Booking not found", 404);

        //    if (booking.Status == BookingStatus.Cancelled)
        //        throw new BusinessException("Cannot create invoice for a cancelled booking", 409);


        //    // لو الفاتورة موجودة خلاص
        //    var existing = (await invRepo.FindAsync(x => x.BookingId == bookingId)).FirstOrDefault();
        //    if (existing != null)
        //    {
        //        var exLines = await lineRepo.FindAsync(l => l.InvoiceId == existing.Id);
        //        existing.Lines = exLines.ToList();
        //        return Map(existing);
        //    }

        //    // هات items غير cancelled فقط (تشكل الفاتورة)
        //    var items = await itemRepo.FindAsync(i =>
        //        i.BookingId == bookingId &&
        //        i.Status != BookingItemStatus.Cancelled);

        //    var subTotal = items.Sum(i => i.UnitPrice);
        //    var invoice = new Invoice
        //    {
        //        BookingId = bookingId,
        //        SubTotal = subTotal,
        //        Discount = 0,
        //        Total = subTotal,
        //        Status = InvoiceStatus.Unpaid
        //    };

        //    await invRepo.AddAsync(invoice);
        //    await _uow.SaveChangesAsync(); // عشان ناخد invoice.Id

        //    // Lines: سطر لكل خدمة (أوضح من سطر واحد)
        //    // لو عايزة سطر واحد فقط، قولي وأنا أعدله
        //    foreach (var it in items)
        //    {
        //        await lineRepo.AddAsync(new InvoiceLine
        //        {
        //            InvoiceId = invoice.Id,
        //            Description = $"Service #{it.ServiceId}",
        //            Qty = 1,
        //            UnitPrice = it.UnitPrice,
        //            Total = it.UnitPrice
        //        });
        //    }

        //    await _uow.SaveChangesAsync();

        //    var linesAfter = await lineRepo.FindAsync(l => l.InvoiceId == invoice.Id);
        //    invoice.Lines = linesAfter.ToList();

        //    return Map(invoice);
        //}


        public async Task<InvoiceResponse> EnsureInvoiceForBookingAsync(int bookingId)
        {
            var bookingRepo = _uow.Repository<Booking>();
            var invRepo = _uow.Repository<Invoice>();
            var lineRepo = _uow.Repository<InvoiceLine>();
            var itemRepo = _uow.Repository<BookingItem>();

            var booking = await bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
                throw new BusinessException("Booking not found", 404);

            if (booking.Status == BookingStatus.Cancelled)
                throw new BusinessException("Cannot create invoice for a cancelled booking", 409);

            // لو الفاتورة موجودة خلاص
            var existing = (await invRepo.FindAsync(x => x.BookingId == bookingId)).FirstOrDefault();
            if (existing != null)
            {
                var exLines = await lineRepo.FindAsync(l => l.InvoiceId == existing.Id);
                existing.Lines = exLines.ToList();
                return Map(existing);
            }

            // items غير cancelled فقط
            var items = await itemRepo.FindAsync(i => i.BookingId == bookingId && i.Status != BookingItemStatus.Cancelled);

            // ✅ totals based on (UnitPrice + MaterialAdjustment)
            var subTotal = items.Sum(it =>
            {
                var t = it.UnitPrice + it.MaterialAdjustment;
                return t < 0 ? 0 : t;
            });

            var invoice = new Invoice
            {
                BookingId = bookingId,
                SubTotal = subTotal,
                Discount = 0,
                Total = subTotal,
                Status = InvoiceStatus.Unpaid
            };

            await invRepo.AddAsync(invoice);
            await _uow.SaveChangesAsync(); // لازم عشان invoice.Id

            //foreach (var it in items)
            //{
            //    var lineTotal = it.UnitPrice + it.MaterialAdjustment;
            //    if (lineTotal < 0) lineTotal = 0;

            //    await lineRepo.AddAsync(new InvoiceLine
            //    {
            //        InvoiceId = invoice.Id,
            //        Description = $"Service #{it.ServiceId} #{it.Service?.Name}",
            //        Qty = 1,
            //        UnitPrice = it.UnitPrice,
            //        Total = lineTotal
            //    });
            //}



            // 1) هات أسماء الخدمات
            var serviceRepo = _uow.Repository<Service>();
            var serviceIds = items.Select(i => i.ServiceId).Distinct().ToList();
            var services = await serviceRepo.FindAsync(s => serviceIds.Contains(s.Id));
            var serviceMap = services.ToDictionary(s => s.Id, s => s.Name);



            // 2) استخدم الاسم في الفاتورة
            foreach (var it in items)
            {
                serviceMap.TryGetValue(it.ServiceId, out var serviceName);

                var lineTotal = it.UnitPrice + it.MaterialAdjustment;
                if (lineTotal < 0) lineTotal = 0;

                await lineRepo.AddAsync(new InvoiceLine
                {
                    InvoiceId = invoice.Id,
                    Description = "Service: "+serviceName ?? "Service",
                    Qty = 1,
                    UnitPrice = it.UnitPrice,
                    Total = lineTotal
                });
            }


            await _uow.SaveChangesAsync();

            var linesAfter = await lineRepo.FindAsync(l => l.InvoiceId == invoice.Id);
            invoice.Lines = linesAfter.ToList();

            return Map(invoice);

        }


        //public async Task RecalculateForBookingAsync(int bookingId)
        //{
        //    var invRepo = _uow.Repository<Invoice>();
        //    var lineRepo = _uow.Repository<InvoiceLine>();
        //    var itemRepo = _uow.Repository<BookingItem>();

        //    var inv = (await invRepo.FindAsync(x => x.BookingId == bookingId)).FirstOrDefault();
        //    if (inv == null) return;

        //    if (inv.Status == InvoiceStatus.Paid)
        //        throw new BusinessException("Cannot change invoice after payment (refund flow needed)", 409);

        //    // rebuild lines based on items (not cancelled)
        //    var items = await itemRepo.FindAsync(i => i.BookingId == bookingId && i.Status != BookingItemStatus.Cancelled);

        //    // delete existing lines (soft delete عندك؟ هنا هنستخدم delete العادي عبر repo)
        //    var oldLines = await lineRepo.FindAsync(l => l.InvoiceId == inv.Id);
        //    foreach (var l in oldLines)
        //        lineRepo.Delete(l);

        //    await _uow.SaveChangesAsync();

        //    foreach (var it in items)
        //    {
        //        await lineRepo.AddAsync(new InvoiceLine
        //        {
        //            InvoiceId = inv.Id,
        //            Description = $"Service #{it.ServiceId}",
        //            Qty = 1,
        //            UnitPrice = it.UnitPrice,
        //            Total = it.UnitPrice
        //        });
        //    }

        //    inv.SubTotal = items.Sum(i => i.UnitPrice);
        //    inv.Total = inv.SubTotal - inv.Discount;

        //    invRepo.Update(inv);
        //    await _uow.SaveChangesAsync();
        //}



        //public async Task RecalculateForBookingAsync(int bookingId)
        //{
        //    var invRepo = _uow.Repository<Invoice>();
        //    var lineRepo = _uow.Repository<InvoiceLine>();
        //    var itemRepo = _uow.Repository<BookingItem>();

        //    var inv = (await invRepo.FindAsync(x => x.BookingId == bookingId)).FirstOrDefault();
        //    if (inv == null) return;

        //    if (inv.Status == InvoiceStatus.Paid)
        //        throw new BusinessException("Cannot change invoice after payment (refund flow needed)", 409);

        //    // rebuild lines based on items (not cancelled)
        //    var items = await itemRepo.FindAsync(i => i.BookingId == bookingId && i.Status != BookingItemStatus.Cancelled);

        //    // delete existing lines (soft delete عندك؟ هنا هنستخدم delete العادي عبر repo)
        //    var oldLines = await lineRepo.FindAsync(l => l.InvoiceId == inv.Id);
        //    foreach (var l in oldLines)
        //        lineRepo.Delete(l);

        //    await _uow.SaveChangesAsync();

        //    foreach (var it in items)
        //    {
        //        await lineRepo.AddAsync(new InvoiceLine
        //        {
        //            InvoiceId = inv.Id,
        //            Description = $"Service #{it.ServiceId}",
        //            Qty = 1,
        //            UnitPrice = it.UnitPrice,
        //            Total = it.UnitPrice
        //        });
        //    }

        //    inv.SubTotal = items.Sum(i => i.UnitPrice);
        //    inv.Total = inv.SubTotal - inv.Discount;

        //    invRepo.Update(inv);
        //    await _uow.SaveChangesAsync();
        //}






        //public async Task RecalculateForBookingAsync(int bookingId, bool save = true)
        //{
        //    var invRepo = _uow.Repository<Invoice>();
        //    var lineRepo = _uow.Repository<InvoiceLine>();
        //    var itemRepo = _uow.Repository<BookingItem>();

        //    var inv = (await invRepo.FindAsync(x => x.BookingId == bookingId)).FirstOrDefault();
        //    if (inv == null) return;

        //    if (inv.Status == InvoiceStatus.Paid)
        //        throw new BusinessException("Cannot change invoice after payment (refund flow needed)", 409);

        //    // items not cancelled
        //    var items = await itemRepo.FindAsync(i => i.BookingId == bookingId && i.Status != BookingItemStatus.Cancelled);

        //    // delete old lines (no SaveChanges here)
        //    var oldLines = await lineRepo.FindAsync(l => l.InvoiceId == inv.Id);
        //    foreach (var l in oldLines)
        //        lineRepo.Delete(l);

        //    // add new lines
        //    foreach (var it in items)
        //    {
        //        await lineRepo.AddAsync(new InvoiceLine
        //        {
        //            InvoiceId = inv.Id,
        //            Description = $"Service #{it.ServiceId}",
        //            Qty = 1,
        //            UnitPrice = it.UnitPrice,
        //            Total = it.UnitPrice
        //        });
        //    }

        //    // update invoice totals
        //    inv.SubTotal = items.Sum(i => i.UnitPrice);
        //    inv.Total = inv.SubTotal - inv.Discount;

        //    invRepo.Update(inv);

        //    // ✅ single save
        //    //try
        //    //{
        //    //    await _uow.SaveChangesAsync();
        //    //}
        //    //catch (DbUpdateConcurrencyException)
        //    //{
        //    //    // conflict -> يرجع 409 بدل 500
        //    //    throw new BusinessException("Invoice was updated by another operation. Please retry.", 409);
        //    //}


        //    if (!save) return;

        //    try
        //    {
        //        await _uow.SaveChangesAsync();
        //    }
        //    catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        //    {
        //        throw new BusinessException("Invoice was updated by another operation. Please retry.", 409);
        //    }


        //}


        public async Task RecalculateForBookingAsync(int bookingId, bool save = true)
        {
            var invRepo = _uow.Repository<Invoice>();
            var lineRepo = _uow.Repository<InvoiceLine>();
            var itemRepo = _uow.Repository<BookingItem>();

            var inv = (await invRepo.FindAsync(x => x.BookingId == bookingId)).FirstOrDefault();
            if (inv == null) return;

            if (inv.Status == InvoiceStatus.Paid)
                throw new BusinessException("Cannot change invoice after payment (refund flow needed)", 409);

            // items not cancelled
            var items = await itemRepo.FindAsync(i => i.BookingId == bookingId && i.Status != BookingItemStatus.Cancelled);

            // delete old lines
            var oldLines = await lineRepo.FindAsync(l => l.InvoiceId == inv.Id);
            foreach (var l in oldLines)
                lineRepo.Delete(l);

            // rebuild lines using (UnitPrice + MaterialAdjustment)
            foreach (var it in items)
            {
                var lineTotal = it.UnitPrice + it.MaterialAdjustment;
                if (lineTotal < 0) lineTotal = 0;

                await lineRepo.AddAsync(new InvoiceLine
                {
                    InvoiceId = inv.Id,
                    Description = $"Service #{it.ServiceId}",
                    Qty = 1,
                    UnitPrice = it.UnitPrice,   // base price (optional keep)
                    Total = lineTotal           // final after materials
                });
            }

            // totals
            var subTotal = items.Sum(it =>
            {
                var t = it.UnitPrice + it.MaterialAdjustment;
                return t < 0 ? 0 : t;
            });

            inv.SubTotal = subTotal;
            inv.Total = inv.SubTotal - inv.Discount;

            invRepo.Update(inv);

            if (!save) return;

            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                throw new BusinessException("Invoice was updated by another operation. Please retry.", 409);
            }
        }


        public async Task<InvoiceResponse> PayCashAsync(int invoiceId, int cashierId)
        {
            // check cashier role
            var empRepo = _uow.Repository<Employee>();
            var cashier = await empRepo.GetByIdAsync(cashierId);
            if (cashier == null || !cashier.IsActive)
                throw new BusinessException("Cashier not found", 404);

            if (!(cashier.Role == EmployeeRole.Cashier || cashier.Role == EmployeeRole.Supervisor || cashier.Role == EmployeeRole.Admin))
                throw new BusinessException("Not allowed", 403);

            var invRepo = _uow.Repository<Invoice>();
            var inv = await invRepo.GetByIdAsync(invoiceId);
            if (inv == null)
                throw new BusinessException("Invoice not found", 404);

            if (inv.Status == InvoiceStatus.Paid)
                throw new BusinessException("Invoice already paid", 409);

            if (inv.Status == InvoiceStatus.Cancelled)
                throw new BusinessException("Invoice is cancelled", 409);

            inv.Status = InvoiceStatus.Paid;
            inv.PaymentMethod = PaymentMethod.Cash;
            inv.PaidByEmployeeId = cashierId;
            inv.PaidAt = DateTime.UtcNow;

            invRepo.Update(inv);
            await _uow.SaveChangesAsync();

            // load lines
            var lines = await _uow.Repository<InvoiceLine>().FindAsync(l => l.InvoiceId == inv.Id);
            inv.Lines = lines.ToList();

            return Map(inv);
        }



        public async Task<InvoiceResponse> SellProductAsync(int invoiceId, SellProductOnInvoiceRequest request)
        {
            // 1) cashier role
            var empRepo = _uow.Repository<Employee>();
            var cashier = await empRepo.GetByIdAsync(request.CashierId);
            if (cashier == null || !cashier.IsActive)
                throw new BusinessException("Cashier not found", 404);

            if (!(cashier.Role == EmployeeRole.Cashier || cashier.Role == EmployeeRole.Admin))
                throw new BusinessException("Not allowed", 403);

            // 2) invoice exists + unpaid
            var invRepo = _uow.Repository<Invoice>();
            var lineRepo = _uow.Repository<InvoiceLine>();
            var bookingRepo = _uow.Repository<Booking>();

            var invoice = await invRepo.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new BusinessException("Invoice not found", 404);

            if (invoice.Status != InvoiceStatus.Unpaid)
                throw new BusinessException("Invoice must be Unpaid to add products", 409);

            // 3) booking to know branch
            var booking = await bookingRepo.GetByIdAsync(invoice.BookingId);
            if (booking == null)
                throw new BusinessException("Booking not found", 404);

            var branchId = booking.BranchId;

            // 4) product exists
            var productRepo = _uow.Repository<Product>();
            var product = await productRepo.GetByIdAsync(request.ProductId);
            if (product == null || !product.IsActive)
                throw new BusinessException("Product not found", 404);

            // 5) check stock
            var stockRepo = _uow.Repository<BranchProductStock>();
            var stock = (await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && s.ProductId == request.ProductId))
                .FirstOrDefault();

            if (stock == null)
                throw new BusinessException("Product stock not found for this branch", 409);

            var available = stock.OnHandQty - stock.ReservedQty;
            if (available < 0) available = 0;

            if (available < request.Qty)
                throw new BusinessException("Not enough product stock", 409,
                    new Dictionary<string, string[]>
                    {
                        ["stock"] = new[] { $"Available={available}, Requested={request.Qty}" }
                    });

            // 6) decrease stock
            stock.OnHandQty -= request.Qty;
            stockRepo.Update(stock);

            // 7) add invoice line (product sale)
            var unitPrice = product.SalePrice;
            var lineTotal = request.Qty * unitPrice;

            await lineRepo.AddAsync(new InvoiceLine
            {
                InvoiceId = invoice.Id,
                Description = $"Product: {product.Name}",
                Qty = (int)request.Qty == request.Qty ? (int)request.Qty : 1, // لو qty decimal، نخليها 1 ونحط السعر في total
                UnitPrice = unitPrice,
                Total = lineTotal
            });

            // 8) update invoice totals
            invoice.SubTotal += lineTotal;
            invoice.Total = invoice.SubTotal - invoice.Discount;
            invRepo.Update(invoice);

            // 9) add product movement SELL
            var moveRepo = _uow.Repository<ProductMovement>();
            var occurred = request.OccurredAt ?? DateTime.UtcNow;

            await moveRepo.AddAsync(new ProductMovement
            {
                BranchId = branchId,
                ProductId = product.Id,
                MovementType = ProductMovementType.Sell,
                Qty = request.Qty,
                UnitCostSnapshot = product.CostPerUnit,
                TotalCost = request.Qty * product.CostPerUnit,
                OccurredAt = occurred,
                InvoiceId = invoice.Id,
                BookingId = booking.Id,
                BookingItemId = null,
                RecordedByEmployeeId = request.CashierId,
                Notes = request.Notes
            });

            // ✅ ONE SAVE
            await _uow.SaveChangesAsync();

            // reload lines for response
            var lines = await lineRepo.FindAsync(l => l.InvoiceId == invoice.Id);
            invoice.Lines = lines.ToList();

            return Map(invoice);
        }



        private static InvoiceResponse Map(Invoice inv) => new()
        {
            Id = inv.Id,
            BookingId = inv.BookingId,
            SubTotal = inv.SubTotal,
            Discount = inv.Discount,
            Total = inv.Total,
            Status = inv.Status,
            PaidByEmployeeId = inv.PaidByEmployeeId,
            PaidAt = inv.PaidAt,
            PaymentMethod = inv.PaymentMethod,
            Lines = inv.Lines.Select(l => new InvoiceLineResponse
            {
                Id = l.Id,
                Description = l.Description,
                Qty = l.Qty,
                UnitPrice = l.UnitPrice,
                Total = l.Total
            }).ToList()
        };










        public async Task<InvoiceGiftOptionsResponse> GetGiftOptionsAsync(int invoiceId)
        {
            var invRepo = _uow.Repository<Invoice>();
            var bookingRepo = _uow.Repository<Booking>();
            var itemRepo = _uow.Repository<BookingItem>();
            var giftOptRepo = _uow.Repository<ServiceGiftOption>();
            var redemptionRepo = _uow.Repository<BookingGiftRedemption>();
            var productRepo = _uow.Repository<Product>();
            var stockRepo = _uow.Repository<BranchProductStock>();

            var invoice = await invRepo.GetByIdAsync(invoiceId);
            if (invoice == null) throw new BusinessException("Invoice not found", 404);

            var booking = await bookingRepo.GetByIdAsync(invoice.BookingId);
            if (booking == null) throw new BusinessException("Booking not found", 404);

            var branchId = booking.BranchId;

            // Already selected?
            var existing = (await redemptionRepo.FindAsync(r => r.BookingId == booking.Id)).FirstOrDefault();

            // Done services فقط
            var doneItems = await itemRepo.FindAsync(i =>
                i.BookingId == booking.Id &&
                i.Status == BookingItemStatus.Done);

            var serviceIds = doneItems.Select(i => i.ServiceId).Distinct().ToList();
            if (serviceIds.Count == 0)
            {
                return new InvoiceGiftOptionsResponse
                {
                    InvoiceId = invoiceId,
                    BookingId = booking.Id,
                    AlreadySelected = existing != null,
                    SelectedProductId = existing?.ProductId,
                    Options = new()
                };
            }

            var giftOptions = await giftOptRepo.FindAsync(o =>
                o.IsActive &&
                serviceIds.Contains(o.ServiceId));

            var productIds = giftOptions.Select(o => o.ProductId).Distinct().ToList();
            if (productIds.Count == 0)
            {
                return new InvoiceGiftOptionsResponse
                {
                    InvoiceId = invoiceId,
                    BookingId = booking.Id,
                    AlreadySelected = existing != null,
                    SelectedProductId = existing?.ProductId,
                    Options = new()
                };
            }

            var products = await productRepo.FindAsync(p => productIds.Contains(p.Id) && p.IsActive);
            var productMap = products.ToDictionary(p => p.Id, p => p);

            var stocks = await stockRepo.FindAsync(s => s.BranchId == branchId && productIds.Contains(s.ProductId));
            var stockMap = stocks.ToDictionary(s => s.ProductId, s => s);

            // Options = union of products (distinct)
            var options = productIds
                .Distinct()
                .Select(pid =>
                {
                    productMap.TryGetValue(pid, out var p);
                    stockMap.TryGetValue(pid, out var st);

                    var onHand = st?.OnHandQty ?? 0m;
                    var reserved = st?.ReservedQty ?? 0m;
                    var available = onHand - reserved;
                    if (available < 0) available = 0;

                    return new GiftOptionDto
                    {
                        ProductId = pid,
                        ProductName = p?.Name ?? "",
                        Sku = p?.Sku,
                        AvailableQty = available
                    };
                })
                .OrderByDescending(x => x.AvailableQty)
                .ThenBy(x => x.ProductName)
                .ToList();

            return new InvoiceGiftOptionsResponse
            {
                InvoiceId = invoiceId,
                BookingId = booking.Id,
                AlreadySelected = existing != null,
                SelectedProductId = existing?.ProductId,
                Options = options
            };
        }














        public async Task<InvoiceResponse> SelectGiftAsync(int invoiceId, SelectInvoiceGiftRequest request)
        {
            // cashier role check (زي PayCash)
            var empRepo = _uow.Repository<Employee>();
            var cashier = await empRepo.GetByIdAsync(request.CashierId);
            if (cashier == null || !cashier.IsActive)
                throw new BusinessException("Cashier not found", 404);

            if (!(cashier.Role == EmployeeRole.Cashier || cashier.Role == EmployeeRole.Supervisor || cashier.Role == EmployeeRole.Admin))
                throw new BusinessException("Not allowed", 403);

            var invRepo = _uow.Repository<Invoice>();
            var lineRepo = _uow.Repository<InvoiceLine>();
            var bookingRepo = _uow.Repository<Booking>();
            var itemRepo = _uow.Repository<BookingItem>();
            var giftOptRepo = _uow.Repository<ServiceGiftOption>();
            var redemptionRepo = _uow.Repository<BookingGiftRedemption>();
            var productRepo = _uow.Repository<Product>();
            var stockRepo = _uow.Repository<BranchProductStock>();
            var moveRepo = _uow.Repository<ProductMovement>();

            var invoice = await invRepo.GetByIdAsync(invoiceId);
            if (invoice == null) throw new BusinessException("Invoice not found", 404);

            if (invoice.Status != InvoiceStatus.Unpaid)
                throw new BusinessException("Invoice must be Unpaid to select gift", 409);

            var booking = await bookingRepo.GetByIdAsync(invoice.BookingId);
            if (booking == null) throw new BusinessException("Booking not found", 404);

            // ✅ واحدة فقط لكل booking
            var existing = (await redemptionRepo.FindAsync(r => r.BookingId == booking.Id)).FirstOrDefault();
            if (existing != null)
                throw new BusinessException("A gift was already selected for this booking", 409);

            // Done items only
            var doneItems = await itemRepo.FindAsync(i => i.BookingId == booking.Id && i.Status == BookingItemStatus.Done);
            var serviceIds = doneItems.Select(i => i.ServiceId).Distinct().ToList();
            if (serviceIds.Count == 0)
                throw new BusinessException("No completed services to unlock gifts", 409);

            // Validate product is an option from any done service
            var isAllowed = await giftOptRepo.AnyAsync(o =>
                o.IsActive &&
                o.ProductId == request.ProductId &&
                serviceIds.Contains(o.ServiceId));

            if (!isAllowed)
                throw new BusinessException("Selected product is not an available gift for this booking", 409);

            var product = await productRepo.GetByIdAsync(request.ProductId);
            if (product == null || !product.IsActive)
                throw new BusinessException("Product not found", 404);

            var branchId = booking.BranchId;

            // Stock check (qty=1 ثابتة)
            var stock = (await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && s.ProductId == request.ProductId))
                .FirstOrDefault();

            if (stock == null)
                throw new BusinessException("Gift product stock not found in this branch", 409);

            var available = stock.OnHandQty - stock.ReservedQty;
            if (available < 0) available = 0;

            if (available < 1)
                throw new BusinessException("Gift out of stock", 409);

            // Decrease stock by 1
            stock.OnHandQty -= 1;
            stockRepo.Update(stock);

            var occurredAt = request.OccurredAt ?? DateTime.UtcNow;

            // Add invoice line price=0
            var line = new InvoiceLine
            {
                InvoiceId = invoice.Id,
                Description = $"Gift: {product.Name}",
                Qty = 1,
                UnitPrice = 0,
                Total = 0
            };
            await lineRepo.AddAsync(line);

            // Save once to get line.Id (MVP)
            await _uow.SaveChangesAsync();

            // Movement GIFT
            await moveRepo.AddAsync(new ProductMovement
            {
                BranchId = branchId,
                ProductId = product.Id,
                MovementType = ProductMovementType.Gift,
                Qty = 1,
                UnitCostSnapshot = product.CostPerUnit,
                TotalCost = 1 * product.CostPerUnit,
                OccurredAt = occurredAt,
                InvoiceId = invoice.Id,
                BookingId = booking.Id,
                BookingItemId = null,
                RecordedByEmployeeId = request.CashierId,
                Notes = request.Notes ?? "Gift selected"
            });

            // Redemption record (one per booking)
            await redemptionRepo.AddAsync(new BookingGiftRedemption
            {
                BookingId = booking.Id,
                ProductId = product.Id,
                InvoiceId = invoice.Id,
                InvoiceLineId = line.Id,
                SelectedByCashierId = request.CashierId,
                OccurredAt = occurredAt,
                Notes = request.Notes
            });

            await _uow.SaveChangesAsync();

            // reload invoice lines
            var lines = await lineRepo.FindAsync(l => l.InvoiceId == invoice.Id);
            invoice.Lines = lines.ToList();

            return Map(invoice);
        }

    }
}
