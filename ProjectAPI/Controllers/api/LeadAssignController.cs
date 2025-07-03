using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Dynamic;
using Project;
using ProjectAPI.Models;
using Newtonsoft.Json;
using System.Data.Entity.Validation;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/LeadAssign")]
    public class LeadAssignController : ApiController
    {
        [HttpPost]
        [Route("LeadAssignList")]
        public ExpandoObject LeadAssign(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                RaiverCRMEntities dbContext = new RaiverCRMEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                LeadAssign model = JsonConvert.DeserializeObject<LeadAssign>(decryptData);

                var staffLoginId = dbContext.StaffLogins.First(x => x.StaffId == model.StaffId).StaffLoginId;
                var staffRoleId = dbContext.StaffLoginRoles.First(x => x.StaffLoginId == staffLoginId).RoleId;

                var AdminList = (from d1 in dbContext.LeadAssigns
                                 join staff in dbContext.StaffLogins on d1.StaffId equals staff.StaffId
                                 //where (model.StaffId == 0 || model.StaffId == d1.StaffId)
                                 select new
                                 {
                                     d1.LeadId,
                                     d1.StaffId,
                                     d1.Staff.StaffName,
                                     d1.Status,
                                     d1.LeadCategoryId,
                                     d1.LeadCategory.LeadCategoryName,
                                     d1.LeadDate,
                                     d1.LeadDetail,
                                     d1.Attachments,
                                     d1.FileFormat,
                                     d1.Instruction,
                                     d1.CreatedBy,
                                     d1.CreatedOn,
                                     d1.UpdatedBy,
                                     d1.UpdatedOn,
                                 }).ToList();
                var InstituteList = (from s in dbContext.LeadAssigns
                                     join staff in dbContext.StaffLogins on s.StaffId equals staff.StaffId
                                     where s.StaffId == model.StaffId

                                     select new
                                     {
                                         s.LeadId,
                                         s.StaffId,
                                         s.Staff.StaffName,
                                         s.Status,
                                         s.LeadCategoryId,
                                         s.LeadCategory.LeadCategoryName,
                                         s.LeadDate,
                                         s.LeadDetail,
                                         s.Attachments,
                                         s.FileFormat,
                                         s.Instruction,
                                         s.CreatedBy,
                                         s.CreatedOn,
                                         s.UpdatedBy,
                                         s.UpdatedOn,
                                     }).ToList();

                if (staffRoleId == 5) // Admin
                {
                    response.LeadAssignList = AdminList;
                    response.Count = AdminList.Count;
                }
                else // Institute
                {
                    response.LeadAssignList = InstituteList;
                }
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }


        [HttpPost]
        [Route("LeadAssignListByStaff")]
        public ExpandoObject LeadAssignListByStaff(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                using (RaiverCRMEntities dbContext = new RaiverCRMEntities())
                {
                    string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);

                    var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    LeadAssign model = JsonConvert.DeserializeObject<LeadAssign>(decryptData);
                    int leadId = model.LeadId;
                    int staffId = model.StaffId;

                    var leadAssignList = (from d1 in dbContext.LeadAssigns
                                          join staff in dbContext.Staffs on d1.StaffId equals staff.StaffId
                                          where (staffId == 0 || d1.StaffId == staffId|| d1.StaffId==5)
                                          select new
                                          {
                                              d1.LeadId,
                                              d1.StaffId,
                                              StaffName = staff.StaffName,
                                              d1.Status,
                                              d1.LeadCategoryId,
                                              LeadCategoryName = d1.LeadCategory.LeadCategoryName,
                                              d1.LeadDate,
                                              d1.LeadDetail,
                                              d1.Attachments,
                                              d1.FileFormat,
                                              d1.Instruction,
                                              d1.CreatedBy,
                                              d1.CreatedOn,
                                              d1.UpdatedBy,
                                              d1.UpdatedOn,
                                          }).ToList();
                    response.LeadAssignList = leadAssignList;
                    response.Count = leadAssignList.Count;
                    response.Message = ConstantData.SuccessMessage;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }


        [HttpPost]
        [Route("SaveLeadAssign")]
        public ExpandoObject SaveLeadAssign(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            using (RaiverCRMEntities dbContext = new RaiverCRMEntities())
            {
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                        AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);

                        var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                        LeadAssignModel model = JsonConvert.DeserializeObject<LeadAssignModel>(decryptData);

                        LeadAssign leadAssign;
                        int leadId;

                        if (model.GetLeadAssign.LeadId > 0)
                        {
                            // Update existing LeadAssign
                            leadAssign = dbContext.LeadAssigns.FirstOrDefault(x => x.LeadId == model.GetLeadAssign.LeadId);
                            if (leadAssign != null)
                            {
                                leadAssign.LeadDate = model.GetLeadAssign.LeadDate;
                                leadAssign.StaffId = model.GetLeadAssign.StaffId;
                                leadAssign.LeadCategoryId = model.GetLeadAssign.LeadCategoryId;
                                leadAssign.Attachments = model.GetLeadAssign.Attachments;
                                leadAssign.FileFormat = model.GetLeadAssign.FileFormat;
                                leadAssign.LeadDetail = model.GetLeadAssign.LeadDetail;
                                leadAssign.Instruction = model.GetLeadAssign.Instruction;
                                leadAssign.Status = model.GetLeadAssign.Status;
                                leadAssign.UpdatedBy = model.GetLeadAssign.UpdatedBy;
                                leadAssign.UpdatedOn = DateTime.Now;
                            }
                            leadId = model.GetLeadAssign.LeadId;
                        }
                        else
                        {
                            // Insert new LeadAssign
                            leadAssign = new LeadAssign
                            {
                                LeadDate = model.GetLeadAssign.LeadDate,
                                StaffId = model.GetLeadAssign.StaffId,
                                LeadCategoryId = model.GetLeadAssign.LeadCategoryId,
                                Attachments = model.GetLeadAssign.Attachments,
                                FileFormat = model.GetLeadAssign.FileFormat,
                                LeadDetail = model.GetLeadAssign.LeadDetail,
                                Instruction = model.GetLeadAssign.Instruction,
                                Status = model.GetLeadAssign.Status,
                                CreatedBy = model.GetLeadAssign.CreatedBy,
                                CreatedOn = DateTime.Now
                            };

                            dbContext.LeadAssigns.Add(leadAssign);
                            dbContext.SaveChanges();

                            leadId = leadAssign.LeadId;
                        }

                        // ✅ Lead Details Add/Update/Delete
                        if (model.GetLeadDetails != null && model.GetLeadDetails.Any())
                        {
                            // Fetch existing details for this LeadId
                            var existing = dbContext.LeadDetails.Where(x => x.LeadId == leadId).ToList();
                            var incomingIds = model.GetLeadDetails.Where(x => x.LeadDetailId > 0).Select(x => x.LeadDetailId).ToList();

                            // Delete details not present in incoming list
                            var toDelete = existing.Where(x => !incomingIds.Contains(x.LeadDetailId)).ToList();
                            dbContext.LeadDetails.RemoveRange(toDelete);

                            foreach (var detail in model.GetLeadDetails)
                            {
                                if (detail.LeadDetailId > 0)
                                {
                                    // Update existing
                                    var existingDetail = dbContext.LeadDetails.FirstOrDefault(x => x.LeadDetailId == detail.LeadDetailId);
                                    if (existingDetail != null)
                                    {
                                        existingDetail.LeadName = detail.LeadName;
                                        existingDetail.LeadMobileNo = detail.LeadMobileNo;
                                        existingDetail.LeadComment = detail.LeadComment;
                                    }
                                }
                                else
                                {
                                    // Insert new
                                    var newDetail = new LeadDetail
                                    {
                                        LeadId = leadId,
                                        LeadName = detail.LeadName,
                                        LeadMobileNo = detail.LeadMobileNo,
                                        LeadComment = detail.LeadComment,
                                        LeadStatus = (int)LeadStatus.Pending,
                                        Comment = ""
                                    };
                                    dbContext.LeadDetails.Add(newDetail);
                                }
                            }

                            dbContext.SaveChanges();
                        }

                        transaction.Commit();
                        response.Message = ConstantData.SuccessMessage;
                        response.LeadId = leadId;
                    }
                    catch (DbEntityValidationException ex)
                    {
                        transaction.Rollback();

                        var errorMessages = ex.EntityValidationErrors
                             .SelectMany(x => x.ValidationErrors)
                             .Select(x => $"Property: {x.PropertyName}, Error: {x.ErrorMessage}");

                        string fullError = string.Join("; ", errorMessages);
                        response.Message = fullError;
                    }
                }
            }

            return response;
        }


        [HttpPost]
        [Route("DeleteLeadAssign")]
        public ExpandoObject DeleteLeadAssign(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            using (RaiverCRMEntities dbContext = new RaiverCRMEntities())
            {
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                        if (string.IsNullOrEmpty(AppKey))
                        {
                            response.Message = "AppKey is missing";
                            return response;
                        }

                        AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);

                        var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                        LeadAssign model = JsonConvert.DeserializeObject<LeadAssign>(decryptData);

                        var leadAssign = dbContext.LeadAssigns.FirstOrDefault(x => x.LeadId == model.LeadId);

                        if (leadAssign == null)
                        {
                            response.Message = "LeadAssign not found";
                            return response;
                        }

                        // ✅ Delete all related LeadDetails (child records)
                        var leadDetails = dbContext.LeadDetails.Where(x => x.LeadId == leadAssign.LeadId).ToList();
                        if (leadDetails.Count > 0)
                        {
                            dbContext.LeadDetails.RemoveRange(leadDetails);
                        }

                        // ✅ Delete parent LeadAssign
                        dbContext.LeadAssigns.Remove(leadAssign);

                        dbContext.SaveChanges();
                        transaction.Commit();

                        response.Message = ConstantData.SuccessMessage;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        response.Message = ex.Message;
                    }
                }
            }

            return response;
        }


    }
}
