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
    [RoutePrefix("api/ResignationDetail")]
    public class ResignationDetailController : ApiController
    {
        [HttpPost]
        [Route("ResignationDetailList")]
        public ExpandoObject ResignationDetailList(RequestModel requestModel)
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

                    var list = (from detail in dbContext.ResignationDetails
                                join resignation in dbContext.Resignations on detail.ResignationId equals resignation.ResignationId
                                join staff in dbContext.Staffs on resignation.StaffId equals staff.StaffId
                                orderby detail.ResignationDetailId descending
                                select new
                                {
                                    detail.ResignationDetailId,
                                    detail.ResignationId,
                                    detail.JoinDate,
                                    detail.DepatureDate,
                                    detail.ResignationStatus,
                                    detail.BreafReason,
                                    staff.StaffName,
                                    staff.StaffCode,
                                    detail.Instruction,
                                    detail.StatusUpdateDate,
                                }).ToList();



                    response.Message = "Success";
                    response.ResignationDetailList = list;
                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                }
            }

            return response;
        }

        [HttpPost]
        [Route("SaveResignationDetail")]
        public ExpandoObject SaveResignationDetail(RequestModel requestModel)
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
                        ResignationDetail model = JsonConvert.DeserializeObject<ResignationDetail>(decryptData);

                        if (model.ResignationDetailId > 0)
                        {
                            // Update existing
                            var existing = dbContext.ResignationDetails
                                .FirstOrDefault(x => x.ResignationDetailId == model.ResignationDetailId);

                            if (existing != null)
                            {
                                existing.JoinDate = model.JoinDate;
                                existing.DepatureDate = model.DepatureDate;
                                existing.ResignationStatus = model.ResignationStatus;
                                existing.BreafReason = model.BreafReason;
                                existing.Instruction = model.Instruction;
                                existing.StatusUpdateDate = DateTime.Now;

                                // 🔁 Also update ResignationStatus in parent table
                                var parent = dbContext.Resignations.FirstOrDefault(r => r.ResignationId == existing.ResignationId);
                                if (parent != null)
                                {
                                    parent.Resignationstatus = model.ResignationStatus;
                                    parent.UpdaedOn = DateTime.Now; // optional: update timestamp
                                }
                            }
                        }

                        else
                        {
                            // Insert new
                            ResignationDetail detail = new ResignationDetail
                            {
                                ResignationId = model.ResignationId,
                                JoinDate = model.JoinDate,
                                DepatureDate = model.DepatureDate,
                                ResignationStatus = model.ResignationStatus,
                                BreafReason = model.BreafReason
                            };

                            dbContext.ResignationDetails.Add(detail);
                        }

                        dbContext.SaveChanges();
                        transaction.Commit();

                        response.Message = "Success";
                    }
                    catch (DbEntityValidationException ex)
                    {
                        transaction.Rollback();
                        response.Message = string.Join("; ", ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => $"Property: {x.PropertyName}, Error: {x.ErrorMessage}"));
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
