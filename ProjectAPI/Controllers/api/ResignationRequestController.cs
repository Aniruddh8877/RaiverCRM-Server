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
    [RoutePrefix("api/ResignationRequest")]
    public class ResignationRequestController : ApiController
    {
        [HttpPost]
        [Route("ResignationList")]
        public ExpandoObject ResignationList(RequestModel requestModel)
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

                    long staffLoginId = data.StaffId;
                    long roleId = data.RoleId;

                    var query = dbContext.LeaveDetails.AsQueryable();

                    // If user is not admin, filter by CreatedBy
                    if (roleId != 5)
                    {
                        query = query.Where(r => r.CreatedBy == staffLoginId);
                    }
                    var result = (from r in dbContext.Resignations
                                  join staff in dbContext.Staffs on r.StaffId equals staff.StaffId
                                  orderby r.ResignationId descending
                                  select new
                                  {
                                      r.ResignationId,
                                      r.JoinDate,
                                      r.DepatureDate,
                                      r.Statement,
                                      r.BreafReason,
                                      r.Resignationstatus,
                                      r.Instruction,
                                      r.StaffId,
                                      StaffName = staff.StaffName,
                                      StaffCode = staff.StaffCode,
                                      r.CreatedBy,
                                      r.CreatedOn,
                                      r.UpdatedBy,
                                      r.UpdaedOn
                                  }).ToList();
                    response.Message = "Success";
                    response.ResignationList = result;
                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                }
            }

            return response;
        }




        [HttpPost]
        [Route("SaveResignation")]
        public ExpandoObject SaveResignation(RequestModel requestModel)
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
                        ResignationMode model = JsonConvert.DeserializeObject<ResignationMode>(decryptData);

                        Resignation resignation;
                        int ResignationId;

                        if (model.GetResignation.ResignationId > 0)
                        {
                            // Update existing LeaveRequest
                            resignation = dbContext.Resignations.FirstOrDefault(x => x.ResignationId == model.GetResignation.ResignationId);
                            if (resignation != null)
                            {
                                resignation.StaffId = model.GetResignation.StaffId;
                                resignation.JoinDate = model.GetResignation.JoinDate;
                                resignation.DepatureDate = model.GetResignation.DepatureDate;
                                resignation.BreafReason = model.GetResignation.BreafReason;
                                resignation.Statement = model.GetResignation.Statement;
                                resignation.Resignationstatus = model.GetResignation.Resignationstatus;
                                resignation.Instruction = model.GetResignation.Instruction;
                                resignation.UpdatedBy = model.GetResignation.UpdatedBy;
                                resignation.UpdaedOn = DateTime.Now;
                            }
                            ResignationId = model.GetResignation.ResignationId;
                        }
                        else
                        {
                            // Insert new LeaveRequest
                            resignation = new Resignation
                            {
                                StaffId = model.GetResignation.StaffId,
                                JoinDate = model.GetResignation.JoinDate,
                                DepatureDate = model.GetResignation.DepatureDate,
                                Statement = model.GetResignation.Statement,
                                BreafReason = model.GetResignation.BreafReason,
                                Instruction = model.GetResignation.Instruction,
                                Resignationstatus = (int)ResignationStatus.Pending, // Example: Setting to Pending
                                CreatedBy = model.GetResignation.CreatedBy,
                                CreatedOn = DateTime.Now
                            };

                            dbContext.Resignations.Add(resignation);
                            dbContext.SaveChanges();
                            ResignationId = resignation.ResignationId;
                        }

                        // ✅ Handle LeaveDetails
                        if (ResignationId > 0 || model.GetResignationDetail != null)
                        {
                            if (model.GetResignationDetail != null)
                            {
                                // ✅ Update existing LeaveDetail
                                var resignationDetail = dbContext.ResignationDetails.FirstOrDefault(x => x.ResignationDetailId == model.GetResignationDetail.ResignationDetailId);
                                if (resignationDetail != null)
                                {
                                    resignationDetail.ResignationStatus = model.GetResignation.Resignationstatus;
                                    resignationDetail.Instruction = model.GetResignation.Instruction;
                                    
                                }
                            }
                            else
                            {
                                // ✅ Insert new LeaveDetail
                                ResignationDetail resignationDetail = new ResignationDetail
                                {

                                    ResignationId = ResignationId,
                                    ResignationStatus = (int)Leavestatus.Pending,
                                    //StatusUpdatedDate = null,
                                    BreafReason = model.GetResignation.BreafReason,
                                    DepatureDate = model.GetResignation.DepatureDate,
                                    JoinDate= model.GetResignation.JoinDate,
                                };

                                dbContext.ResignationDetails.Add(resignationDetail);
                            }
                        }


                        dbContext.SaveChanges();
                        transaction.Commit();

                        response.Message = ConstantData.SuccessMessage;
                        response.ResignationId = ResignationId;
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
