
//        [HttpPost]
//        [Route("SurgeryList")]
//        public ExpandoObject SurgeryList(RequestModel requestModel)
//        {
//            dynamic response = new ExpandoObject();
//            try
//            {
//                using (var dbContext = new MamtaNetralayEntities())
//                {
//                    string AppKey = HttpContext.Current.Request.Headers["AppKey"];
//                    AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);

//                    var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
//                    var wrapper = JsonConvert.DeserializeObject<RequestWrapper>(decryptData);
//                    var model = JsonConvert.DeserializeObject<SurgeryListFilter>(wrapper.requestPayload);
//                    model.Page = wrapper.Page;
//                    model.PageSize = wrapper.PageSize;

//                    if (string.IsNullOrEmpty(decryptData))
//                    {
//                        response.Message = "Decryption failed.";
//                        return response;
//                    }

//                    // Handle date filtering similar to OpticalBillingList
//                    if (model.StartFrom.HasValue)
//                        model.StartFrom = model.StartFrom.Value.Date;
//                    if (model.EndFrom.HasValue)
//                        model.EndFrom = model.EndFrom.Value.Date;

//                    int skip = (model.Page - 1) * model.PageSize;

//                    var query = (from surg in dbContext.SurgeryBillings
//                                 join Pat in dbContext.Patients on surg.PatientId equals Pat.PatientID
//                                 join cat in dbContext.PaymentCollections on surg.SurgeryId equals cat.SurgeryId
//                                 where (model.StartFrom == null || DbFunctions.TruncateTime(surg.SurgeryDate) >= model.StartFrom)
//                                    && (model.EndFrom == null || DbFunctions.TruncateTime(surg.SurgeryDate) <= model.EndFrom)
//                                    && (model.SurgeryId == null || model.SurgeryId == 0 || surg.SurgeryId == model.SurgeryId)
//                                    && (model.PatientId == null || model.PatientId == 0 || surg.PatientId == model.PatientId)
//                                    && (model.PaymentStatus == null || model.PaymentStatus == 0 || cat.PaymentStatus == model.PaymentStatus)
//                                 orderby surg.SurgeryId descending
//                                 select new
//                                 {
//                                     surg.SurgeryId,
//                                     surg.SurgeryCode,
//                                     surg.SurgeryDate,
//                                     surg.PatientId,
//                                     surg.CreatedBy,
//                                     surg.createdOn,
//                                     surg.UpdatedBy,
//                                     surg.UpdatedOn,
//                                     Pat.PatientName,
//                                     Pat.UHID,
//                                     Pat.Age,
//                                     Pat.Gender,
//                                     Pat.ContactNo,
//                                     cat.TotalAmount,
//                                     cat.PayableAmount,
//                                     cat.PaymentCollectionId,
//                                     cat.PaidAmount,
//                                     cat.DueAmount,
//                                     cat.DiscountAmount,
//                                     cat.PaymentStatus,
//                                 });

//                    var totalCount = query.Count();
//                    var pagedData = query
//                                     .Skip(skip)
//                                     .Take(model.PageSize)
//                                     .ToList()
//                                     .Select(x => new
//                                     {
//                                         x.SurgeryId,
//                                         x.SurgeryCode,
//                                         x.SurgeryDate,
//                                         x.PatientId,
//                                         x.CreatedBy,
//                                         x.createdOn,
//                                         x.UpdatedBy,
//                                         x.UpdatedOn,
//                                         x.PatientName,
//                                         x.UHID,
//                                         x.Age,
//                                         x.Gender,
//                                         x.ContactNo,
//                                         x.PayableAmount,
//                                         x.PaymentCollectionId,
//                                         x.PaidAmount,
//                                         x.DueAmount,
//                                         x.DiscountAmount,
//                                         x.TotalAmount,
//                                         x.PaymentStatus,
//                                         PaymentStatusName = Enum.GetName(typeof(PaymentStatus), x.PaymentStatus) ?? "Unknown"
//                                     })
//                                     .ToList();

//                    response.TotalRecords = totalCount;
//                    response.SurgeryList = pagedData;
//                    response.AmountTotal = pagedData.Sum(x => x.TotalAmount);
//                    response.DiscountAmountTotal = pagedData.Sum(x => x.DiscountAmount);
//                    response.PaidAmountTotal = pagedData.Sum(x => x.PaidAmount);
//                    response.DueAmountTotal = pagedData.Sum(x => x.DueAmount);
//                    response.TotalPayableAmount = pagedData.Sum(x => x.PayableAmount);
//                    response.Message = ConstantData.SuccessMessage;
//                }
//            }
//            catch (Exception ex)
//            {
//                // Log ex.StackTrace here
//                response.Message = "An error occurred while processing the request.";
//            }
//            return response;
//        }

//        [HttpPost]
//        [Route("ListSurgery")]
//        public ExpandoObject ListSurgery(RequestModel requestModel)
//        {
//            using (var dbContext = new MamtaNetralayEntities())

//            {
//                dynamic response = new ExpandoObject();
//                try
//                {
//                    string AppKey = HttpContext.Current.Request.Headers["AppKey"];
//                    AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);

//                    var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
//                    OpdBookingFilter model = JsonConvert.DeserializeObject<OpdBookingFilter>(decryptData);
//                    var surgery = (from s in dbContext.SurgeryBillings
//                                   join p in dbContext.Patients
//                                   on s.PatientId equals p.PatientID
//                                   where s.SurgeryId == model.SurgeryId
//                                   select new
//                                   {
//                                       GetSurgery = new
//                                       {
//                                           s.SurgeryId,
//                                           s.SurgeryCode,
//                                           s.SurgeryDate,
//                                           s.PatientId,
//                                           s.CreatedBy,
//                                           s.createdOn,
//                                           s.UpdatedBy,
//                                           s.UpdatedOn,

//                                           // Patient fields
//                                           p.PatientName,
//                                           p.Age,
//                                           p.Gender,
//                                           p.Address,
//                                           p.ContactNo,
//                                           p.UHID,
//                                       },

//                                       GetPaymentCollection = s.PaymentCollections
//                                           .Select(pc => new
//                                           {
//                                               pc.PaymentCollectionId,
//                                               pc.PaymentDate,
//                                               pc.TotalAmount,
//                                               pc.PayableAmount,
//                                               pc.DiscountAmount,
//                                               pc.PaidAmount,
//                                               pc.DueAmount,
//                                               pc.PaymentStatus,
//                                               pc.InvoiceNo,
//                                               pc.CreatedBy,
//                                               pc.CreatedOn,
//                                               pc.UpdatedBy,
//                                               pc.UpdatedOn
//                                           }).FirstOrDefault(),

//                                       GetPackageBookingDetail = s.PaymentCollections
//                                           .SelectMany(pc => pc.SurgeryPackageBookingDetails)
//                                           .Select(pbd => new
//                                           {
//                                               pbd.PackageBookingDetailId,
//                                               pbd.PackageName,
//                                               pbd.Price,
//                                               pbd.Description,
//                                               pbd.PaymentCollectionId,
//                                               pbd.PackageDetialId
//                                           }).ToList(),

//                                       GetPaymentDetails = s.PaymentCollections
//                                           .SelectMany(pc => pc.PaymentDetails)
//                                           .Select(pd => new
//                                           {
//                                               pd.PaymentDetailId,
//                                               pd.PaymentMode,
//                                               pd.Particular,
//                                               pd.Remarks,
//                                               pd.PaidAmount,
//                                               pd.PaymentDate,
//                                               pd.CreatedBy,
//                                               pd.CreatedOn,
//                                               pd.UpdatedBy,
//                                               pd.UpdatedOn,
//                                               pd.PaymentCollectionId
//                                           }).ToList()
//                                   }).FirstOrDefault();



//                    if (surgery == null)
//                    {
//                        response.Message = ConstantData.AccessDenied;
//                    }
//                    response.Message = ConstantData.SuccessMessage;
//                    response.Surgery = surgery;
//                }
//                catch (Exception ex)
//                {
//                    response.Message = ex.Message;
//                }

//                return response;
//            }
//        }



//        [HttpPost]
//        [Route("SaveSurgery")]
//        public ExpandoObject SaveSurgery(RequestModel requestModel)
//        {
//            dynamic res = new ExpandoObject();

//            using (var dbContext = new MamtaNetralayEntities())
//            using (var transaction = dbContext.Database.BeginTransaction())
//            {
//                try
//                {
//                    string AppKey = HttpContext.Current.Request.Headers["AppKey"];
//                    if (string.IsNullOrEmpty(AppKey))
//                    {
//                        res.Message = "AppKey is missing";
//                        return res;
//                    }
//                    AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
//                    var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
//                    SurgeryModel model = JsonConvert.DeserializeObject<SurgeryModel>(decryptData);
//                    SurgeryBilling surgery = null;
//                    if (model.GetSurgery.SurgeryId > 0)
//                    {
//                        surgery = dbContext.SurgeryBillings.First(x => x.SurgeryId == model.GetSurgery.SurgeryId);
//                        if (surgery == null)
//                        {
//                            res.Message = "Opd not found";
//                            return res;
//                        }

//                        surgery.SurgeryDate = model.GetSurgery.SurgeryDate;
//                        //Surgery.Status = model.GetSurgery.Status;
//                        //opdBooking.Status = (byte)BookingStatus.Pending;
//                        surgery.UpdatedBy = model.GetSurgery.UpdatedBy;
//                        surgery.UpdatedOn = DateTime.Now;
//                    }
//                    else
//                    {
//                        surgery = new SurgeryBilling
//                        {
//                            SurgeryCode = AppData.GenerateSurgeryCode(dbContext),
//                            //TokenNo = AppData.GenerateToken(dbContext),
//                            PatientId = model.GetSurgery.PatientId,
//                            SurgeryDate = model.GetSurgery.SurgeryDate,
//                            //Status = model.GetOpdBooking.Status,
//                            CreatedBy = model.GetSurgery.CreatedBy,
//                            createdOn = DateTime.Now,
//                        };

//                        dbContext.SurgeryBillings.Add(surgery);
//                    }

//                    dbContext.SaveChanges();

//                    // Save Payment Collection
//                    Random random = new Random();


//                    int? paymentCollectionId = null;
//                    PaymentCollection paycol = null;
//                    if (model.GetPaymentCollection != null)

//                        if (model.GetPaymentCollection.PaymentCollectionId > 0)
//                        {
//                            paycol = dbContext.PaymentCollections.First(x => x.PaymentCollectionId == model.GetPaymentCollection.PaymentCollectionId);
//                            paycol.PaymentStatus = (byte)PaymentStatus.Due;
//                            paycol.PaymentDate = model.GetPaymentCollection.PaymentDate;
//                            paycol.TotalAmount = model.GetPaymentCollection.TotalAmount;
//                            paycol.PayableAmount = model.GetPaymentCollection.PayableAmount;
//                            paycol.DiscountAmount = model.GetPaymentCollection.DiscountAmount;
//                            paycol.TotalAmount = model.GetPaymentCollection.TotalAmount;
//                            paycol.UpdatedBy = model.GetPaymentCollection.UpdatedBy;
//                            paycol.UpdatedOn = DateTime.Now;
//                            dbContext.SaveChanges();
//                            paymentCollectionId = paycol.PaymentCollectionId;

//                        }
//                        else
//                        {
//                            PaymentCollection Paycol = new PaymentCollection();
//                            Paycol.PaymentType = (byte)PaymentType.Surgery;
//                            Paycol.PaymentStatus = (byte)PaymentStatus.Due;
//                            Paycol.PaymentDate = model.GetPaymentCollection.PaymentDate;
//                            Paycol.InvoiceNo = AppData.GenerateInvoiceNumber(dbContext);
//                            Paycol.ReceiptNo = AppData.GenerateReceiptNumber(dbContext, Paycol.PaymentType);

//                            Paycol.SurgeryId = surgery.SurgeryId;
//                            Paycol.TotalAmount = model.GetPaymentCollection.TotalAmount;
//                            Paycol.PayableAmount = model.GetPaymentCollection.PayableAmount;
//                            Paycol.DiscountAmount = model.GetPaymentCollection.DiscountAmount;
//                            Paycol.TotalAmount = model.GetPaymentCollection.TotalAmount;
//                            Paycol.CreatedBy = model.GetPaymentCollection.CreatedBy;
//                            Paycol.CreatedOn = DateTime.Now;
//                            dbContext.PaymentCollections.Add(Paycol);
//                            dbContext.SaveChanges();
//                            paymentCollectionId = Paycol.PaymentCollectionId;
//                        }
//                    {

//                    }




//                    if (model.GetPackageBookingDetail != null && model.GetPackageBookingDetail.Any())
//                    {
//                        var existingBookingDetails = dbContext.SurgeryPackageBookingDetails
//                            .Where(x => x.PaymentCollectionId == paymentCollectionId)
//                            .ToList();

//                        var incomingBookingDetailIds = model.GetPackageBookingDetail
//                            .Where(x => x.PackageBookingDetailId > 0)
//                            .Select(x => x.PackageBookingDetailId)
//                            .ToList();

//                        // Delete ho raha package booking
//                        var toDelete = existingBookingDetails
//                            .Where(x => !incomingBookingDetailIds.Contains(x.PackageBookingDetailId))
//                            .ToList();

//                        foreach (var detail in toDelete)
//                        {
//                            dbContext.SurgeryPackageBookingDetails.Remove(detail);
//                        }

//                        // yaha se Insert or update ho raha hai package booking
//                        foreach (var detail in model.GetPackageBookingDetail)
//                        {
//                            detail.PaymentCollectionId = paymentCollectionId ?? 0;
//                            if (detail.PackageBookingDetailId > 0)
//                            {

//                                var existingDetail = dbContext.SurgeryPackageBookingDetails
//                                    .First(x => x.PackageBookingDetailId == detail.PackageBookingDetailId);
//                                existingDetail.PackageBookingDetailId = detail.PackageBookingDetailId;
//                                existingDetail.PackageName = detail.PackageName;
//                                existingDetail.Price = detail.Price;
//                            }
//                            else
//                            {
//                                // Insert
//                                //detail.CreatedBy = (int)model.GetSurgery.CreatedBy;
//                                //detail.CreatedOn = DateTime.Now;
//                                dbContext.SurgeryPackageBookingDetails.Add(detail);
//                            }
//                        }

//                        dbContext.SaveChanges();
//                    }

//                    // Then, update PaymentDetails 
//                    // Step 1: Handle PaymentDetail list from frontend

//                    // Update PaymentCollection
//                    var paymentCollection = dbContext.PaymentCollections
//                        .First(x => x.PaymentCollectionId == paymentCollectionId);

//                    if (paymentCollection != null)
//                        if (model.GetPaymentDetails != null && model.GetPaymentDetails.Any())
//                        {
//                            // Fetch existing PaymentDetails
//                            var existingPaymentDetails = dbContext.PaymentDetails
//                                .Where(x => x.PaymentCollectionId == paymentCollectionId)
//                                .ToList();

//                            // Get IDs from frontend
//                            var incomingPaymentDetailIds = model.GetPaymentDetails
//                                .Where(x => x.PaymentDetailId > 0)
//                                .Select(x => x.PaymentDetailId)
//                                .ToList();

//                            // Delete removed PaymentDetails
//                            var toDelete = existingPaymentDetails
//                                .Where(x => !incomingPaymentDetailIds.Contains(x.PaymentDetailId))
//                                .ToList();

//                            foreach (var detail in toDelete)
//                            {
//                                dbContext.PaymentDetails.Remove(detail);
//                            }

//                            // Insert/update PaymentDetails
//                            foreach (var detail in model.GetPaymentDetails)
//                            {
//                                detail.PaymentCollectionId = paymentCollectionId ?? 0;

//                                if (detail.PaymentDetailId > 0)
//                                {
//                                    // Update
//                                    var existingDetail = dbContext.PaymentDetails
//                                        .First(x => x.PaymentDetailId == detail.PaymentDetailId);

//                                    existingDetail.Particular = detail.Particular;
//                                    existingDetail.PaidAmount = detail.PaidAmount;
//                                    existingDetail.RefundAmont = detail.RefundAmont;
//                                    existingDetail.PaymentMode = detail.PaymentMode;
//                                    existingDetail.PaymentDate = model.GetPaymentCollection.PaymentDate;
//                                    existingDetail.Remarks = detail.Remarks;
//                                    existingDetail.UpdatedBy = (int)model.GetSurgery.UpdatedBy;
//                                    existingDetail.UpdatedOn = DateTime.Now;
//                                }
//                                else
//                                {
//                                    // Insert
//                                    detail.CreatedBy = (int)model.GetSurgery.CreatedBy;
//                                    detail.CreatedOn = DateTime.Now;
//                                    detail.PaymentDate = model.GetPaymentCollection.PaymentDate;
//                                    dbContext.PaymentDetails.Add(detail);
//                                }
//                            }

//                            dbContext.SaveChanges();
//                            {
//                                paymentCollection.PaidAmount = dbContext.PaymentDetails
//                                    .Where(x => x.PaymentCollectionId == paymentCollectionId)
//                                    .Sum(x => x.PaidAmount);

//                                paymentCollection.DueAmount = paymentCollection.PayableAmount - paymentCollection.PaidAmount;
//                                paymentCollection.PaymentDate = model.GetPaymentCollection.PaymentDate;
//                                paymentCollection.UpdatedBy = (int)model.GetSurgery.UpdatedBy;
//                                paymentCollection.UpdatedOn = DateTime.Now;
//                                paymentCollection.PaymentStatus = paymentCollection.DueAmount == 0
//                                    ? (byte)PaymentStatus.Paid
//                                    : (byte)PaymentStatus.Due;
//                            }

//                            dbContext.SaveChanges();
//                        }


//                    transaction.Commit();

//                    res.Message = ConstantData.SuccessMessage;
//                    res.SurgeryId = surgery.SurgeryId;
//                }
//                catch (DbEntityValidationException ex)
//                {
//                    transaction.Rollback();

//                    var errorMessages = ex.EntityValidationErrors
//                        .SelectMany(x => x.ValidationErrors)
//                        .Select(x => $"Property: {x.PropertyName}, Error: {x.ErrorMessage}");

//                    string fullError = string.Join("; ", errorMessages);
//                    res.Message = fullError;
//                }


//            }

//            return res;
//        }

//        [HttpPost]
//        [Route("DeleteSurgery")]
//        public ExpandoObject DeleteSurgery(RequestModel requestModel)
//        {
//            dynamic res = new ExpandoObject();

//            using (var dbContext = new MamtaNetralayEntities())
//            using (var transaction = dbContext.Database.BeginTransaction())
//            {
//                try
//                {
//                    string AppKey = HttpContext.Current.Request.Headers["AppKey"];
//                    if (string.IsNullOrEmpty(AppKey))
//                    {
//                        res.Message = "AppKey is missing";
//                        return res;
//                    }

//                    AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);

//                    var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
//                    SurgeryModel model = JsonConvert.DeserializeObject<SurgeryModel>(decryptData);

//                    if (model.SurgeryId == null || model.SurgeryId <= 0)
//                    {
//                        res.Message = "Invalid SurgeryId";
//                        return res;
//                    }

//                    int surgeryId = model.SurgeryId;

//                    // Fetch the Surgery
//                    var surgery = dbContext.SurgeryBillings.FirstOrDefault(x => x.SurgeryId == surgeryId);
//                    if (surgery == null)
//                    {
//                        res.Message = "Surgery not found";
//                        return res;
//                    }

//                    // Delete PaymentDetails
//                    var paymentCollections = dbContext.PaymentCollections
//                        .Where(pc => pc.SurgeryId == surgeryId)
//                        .ToList();

//                    foreach (var pc in paymentCollections)
//                    {
//                        var paymentDetails = dbContext.PaymentDetails
//                            .Where(pd => pd.PaymentCollectionId == pc.PaymentCollectionId)
//                            .ToList();

//                        dbContext.PaymentDetails.RemoveRange(paymentDetails);
//                    }

//                    dbContext.SaveChanges();

//                    // Delete PackageBookingDetails
//                    var allPaymentCollectionIds = paymentCollections.Select(pc => pc.PaymentCollectionId).ToList();
//                    var packageBookingDetails = dbContext.SurgeryPackageBookingDetails
//                        .Where(pbd => allPaymentCollectionIds.Contains(pbd.PaymentCollectionId))
//                        .ToList();

//                    dbContext.SurgeryPackageBookingDetails.RemoveRange(packageBookingDetails);
//                    dbContext.SaveChanges();

//                    // Delete PaymentCollections
//                    dbContext.PaymentCollections.RemoveRange(paymentCollections);
//                    dbContext.SaveChanges();

//                    // Finally, delete the Surgery
//                    dbContext.SurgeryBillings.Remove(surgery);
//                    dbContext.SaveChanges();

//                    transaction.Commit();
//                    res.Message = ConstantData.SuccessMessage;
//                    res.message = "Surgery and related data deleted successfully.";
//                }
//                catch (DbEntityValidationException ex)
//                {
//                    transaction.Rollback();

//                    var errorMessages = ex.EntityValidationErrors
//                        .SelectMany(x => x.ValidationErrors)
//                        .Select(x => $"Property: {x.PropertyName}, Error: {x.ErrorMessage}");

//                    string fullError = string.Join("; ", errorMessages);
//                    res.Message = fullError;
//                }
//                catch (Exception ex)
//                {
//                    transaction.Rollback();
//                    res.Message = "An error occurred: " + ex.Message;
//                }
//            }

//            return res;
//        }

//    }
//}
//}
