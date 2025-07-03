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
    [RoutePrefix("api/LeaveDetail")]
    public class LeaveDetailController : ApiController
    {
        [HttpPost]
        [Route("LeaveDetailList")]
        public ExpandoObject LeaveDetailList(RequestModel requestModel)
       {
            dynamic response = new ExpandoObject();

            using (RaiverCRMEntities dbContext = new RaiverCRMEntities())
            {
                try
                {
                    string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);

                    var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    dynamic data = JsonConvert.DeserializeObject<ExpandoObject>(decryptData);

                    long staffLoginId = data.CreatedBy;
                    long roleId = data.RoleId;

                    var query = dbContext.LeaveDetails.AsQueryable();

                    // If user is not admin, filter by CreatedBy
                    if (roleId != 5)
                    {
                        query = query.Where(ld => ld.CreatedBy == staffLoginId);
                    }
                    var leaveDetails = (from ld in dbContext.LeaveDetails
                                        join lr in dbContext.LeaveRequests on ld.LeaveId equals lr.LeaveId
                                        join staff in dbContext.Staffs on lr.StaffId equals staff.StaffId
                                        orderby ld.LeaveDetailsId descending
                                        select new
                                        {
                                            ld.LeaveDetailsId,
                                            staffName = staff.StaffName,
                                            staffCode = staff.StaffCode,
                                            NoOfDays = lr.NoOfDays,
                                            Message = lr.Message,
                                            LeaveDateFrom = lr.LeaveDateFrom,
                                            LeaveDateTo = lr.LeaveDateTo,
                                            ld.LeaveId,
                                            ld.LeaveStatus,
                                            ld.StatusUpdatedDate,
                                            ld.Opinion,
                                            ld.CreatedBy,
                                            ld.CreatedOn,
                                            ld.UpdatedBy,
                                            ld.UpdatedOn
                                        }).ToList();


                    response.Message = ConstantData.SuccessMessage;
                    response.LeaveDetailList = leaveDetails;
                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                }
            }

            return response;
        }

        [HttpPost]
        [Route("UpdateLeaveDetail")]
        public ExpandoObject UpdateLeaveDetail(RequestModel requestModel)
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
                        LeaveDetail model = JsonConvert.DeserializeObject<LeaveDetail>(decryptData);

                        var existing = dbContext.LeaveDetails.FirstOrDefault(ld => ld.LeaveDetailsId == model.LeaveDetailsId);
                        if (existing == null)
                        {
                            response.Message = "LeaveDetail not found.";
                            return response;
                        }

                        // ✅ Update LeaveDetail
                        existing.LeaveStatus = model.LeaveStatus;
                        existing.Opinion = model.Opinion;
                        existing.StatusUpdatedDate = DateTime.Now;
                        existing.UpdatedBy = model.UpdatedBy;
                        existing.UpdatedOn = DateTime.Now;

                        // ✅ Also update LeaveRequest table's status
                        var leaveRequest = dbContext.LeaveRequests.FirstOrDefault(lr => lr.LeaveId == existing.LeaveId);
                        if (leaveRequest != null)
                        {
                            leaveRequest.LeaveStatus = model.LeaveStatus;
                            leaveRequest.UpdatedBy = model.UpdatedBy;
                            leaveRequest.UpdatedOn = DateTime.Now;
                        }

                        dbContext.SaveChanges();
                        transaction.Commit();

                        response.Message = ConstantData.SuccessMessage;
                        response.LeaveDetailsId = existing.LeaveDetailsId;
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
