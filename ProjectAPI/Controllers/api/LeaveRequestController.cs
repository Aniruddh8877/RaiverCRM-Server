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
    [RoutePrefix("api/LeaveRequest")]
    public class LeaveRequestController : ApiController
    {
        [HttpPost]
        [Route("LeaveRequestList")]
        public ExpandoObject LeaveRequestList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                RaiverCRMEntities dbContext = new RaiverCRMEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                LeaveRequest model = JsonConvert.DeserializeObject<LeaveRequest>(decryptData);

                var staffLoginId = dbContext.StaffLogins.First(x => x.StaffId == model.StaffId).StaffLoginId;
                var staffRoleId = dbContext.StaffLoginRoles.First(x => x.StaffLoginId == staffLoginId).RoleId;

                var AdminList = (from d1 in dbContext.LeaveRequests
                                 join staff in dbContext.StaffLogins on d1.StaffId equals staff.StaffId
                                 //where (model.StaffId == 0 || model.StaffId == d1.StaffId)
                                 select new
                                 {
                                     d1.LeaveId,
                                     d1.StaffId,
                                     d1.Staff.StaffName,
                                     d1.LeaveDateFrom,
                                     d1.LeaveDateTo,
                                     d1.LeaveStatus,
                                     d1.NoOfDays,
                                     d1.LeaveType,
                                     d1.Message,
                                     d1.CreatedBy,
                                     d1.CreatedOn,
                                     d1.UpdatedBy,
                                     d1.UpdatedOn,
                                 }).ToList();
                var InstituteList = (from d1 in dbContext.LeaveRequests
                                     join staff in dbContext.StaffLogins on d1.StaffId equals staff.StaffId
                                     where d1.StaffId == model.StaffId

                                     select new
                                     {
                                         d1.LeaveId,
                                         d1.StaffId,
                                         d1.Staff.StaffName,
                                         d1.LeaveDateFrom,
                                         d1.LeaveDateTo,
                                         d1.LeaveStatus,
                                         d1.LeaveType,
                                         d1.NoOfDays,
                                         d1.Message,
                                         d1.CreatedBy,
                                         d1.CreatedOn,
                                         d1.UpdatedBy,
                                         d1.UpdatedOn,
                                     }).ToList();

                if (staffRoleId == 5) // Admin
                {
                    response.LeaveRequestList = AdminList;
                    response.Count = AdminList.Count;
                }
                else // Institute
                {
                    response.LeaveRequestList = InstituteList;
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
        [Route("SaveLeaveRequest")]
        public ExpandoObject SaveLeaveRequest(RequestModel requestModel)
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
                        LeaveRequestModel model = JsonConvert.DeserializeObject<LeaveRequestModel>(decryptData);

                        LeaveRequest leaveRequest;
                        int leaveId;

                        if (model.GetLeaveRequest.LeaveId > 0)
                        {
                            // Update existing LeaveRequest
                            leaveRequest = dbContext.LeaveRequests.FirstOrDefault(x => x.LeaveId == model.GetLeaveRequest.LeaveId);
                            if (leaveRequest != null)
                            {
                                leaveRequest.StaffId = model.GetLeaveRequest.StaffId;
                                leaveRequest.LeaveType = model.GetLeaveRequest.LeaveType;
                                leaveRequest.LeaveDateFrom = model.GetLeaveRequest.LeaveDateFrom;
                                leaveRequest.LeaveDateTo = model.GetLeaveRequest.LeaveDateTo;
                                leaveRequest.NoOfDays = model.GetLeaveRequest.NoOfDays;
                                leaveRequest.Message = model.GetLeaveRequest.Message;
                                leaveRequest.LeaveStatus = model.GetLeaveRequest.LeaveStatus;
                                leaveRequest.UpdatedBy = model.GetLeaveRequest.UpdatedBy;
                                leaveRequest.UpdatedOn = DateTime.Now;
                            }
                            leaveId = model.GetLeaveRequest.LeaveId;
                        }
                        else
                        {
                            // Insert new LeaveRequest
                            leaveRequest = new LeaveRequest
                            {
                                StaffId = model.GetLeaveRequest.StaffId,
                                LeaveType = model.GetLeaveRequest.LeaveType,
                                LeaveDateFrom = model.GetLeaveRequest.LeaveDateFrom,
                                LeaveDateTo = model.GetLeaveRequest.LeaveDateTo,
                                NoOfDays = model.GetLeaveRequest.NoOfDays,
                                Message = model.GetLeaveRequest.Message,
                                LeaveStatus = (int)Leavestatus.Pending, // Example: Setting to Pending
                                CreatedBy = model.GetLeaveRequest.CreatedBy,
                                CreatedOn = DateTime.Now
                            };

                            dbContext.LeaveRequests.Add(leaveRequest);
                            dbContext.SaveChanges();
                            leaveId = leaveRequest.LeaveId;
                        }

                        // ✅ Handle LeaveDetails
                        if (leaveId > 0 || model.GetLeaveDetails != null)
                        {
                            if (model.GetLeaveDetails != null )
                            {
                                // ✅ Update existing LeaveDetail
                                var leaveDetail = dbContext.LeaveDetails.FirstOrDefault(x => x.LeaveDetailsId == model.GetLeaveDetails.LeaveDetailsId);
                                if (leaveDetail != null)
                                {
                                    leaveDetail.LeaveStatus = model.GetLeaveRequest.LeaveStatus;
                                    leaveDetail.StatusUpdatedDate = DateTime.Now;
                                    leaveDetail.Opinion = model.GetLeaveDetails.Opinion;
                                    leaveDetail.UpdatedBy = model.GetLeaveDetails.UpdatedBy;
                                    leaveDetail.UpdatedOn = DateTime.Now;
                                }
                            }
                            else
                            {
                                // ✅ Insert new LeaveDetail
                                LeaveDetail leaveDetail = new LeaveDetail
                                {
                                    LeaveId = leaveId,
                                    LeaveStatus = (int)Leavestatus.Pending,
                                    StatusUpdatedDate = null,
                                    Opinion = null,
                                    CreatedBy = model.GetLeaveRequest.CreatedBy,
                                    CreatedOn = DateTime.Now
                                };

                                dbContext.LeaveDetails.Add(leaveDetail);
                            }
                        }


                        dbContext.SaveChanges();
                        transaction.Commit();

                        response.Message = ConstantData.SuccessMessage;
                        response.LeaveId = leaveId;
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
