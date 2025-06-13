using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Dynamic;
using Project;
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


                var list = (from d1 in dbContext.LeadAssigns
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
                                d1.Attachment,
                                d1.FileType,
                                d1.Instruction,
                                d1.CreatedBy,
                                d1.CreatedOn,
                                d1.UpdatedBy,
                                d1.UpdatedOn,
                            }).ToList();

                response.LeadAssignList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch(Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }


        [HttpPost]
        [Route("SaveLeadAssign")] // Corrected route name
        public ExpandoObject SaveLeadAssign(RequestModel requestModel)
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

                    LeadAssign leadAssign;

                    if (model.LeadId > 0)
                    {
                        // Update
                        leadAssign = dbContext.LeadAssigns.FirstOrDefault(x => x.LeadId == model.LeadId);
                        if (leadAssign == null)
                        {
                            response.Message = "Lead not found.";
                            return response;
                        }

                        leadAssign.StaffId = model.StaffId;
                        leadAssign.LeadCategoryId = model.LeadCategoryId;
                        leadAssign.LeadDate = model.LeadDate;
                        leadAssign.LeadDetail = model.LeadDetail;
                        leadAssign.Attachment = model.Attachment;
                        leadAssign.FileType = model.FileType;
                        leadAssign.Instruction = model.Instruction;
                        leadAssign.Status = model.Status;
                        leadAssign.UpdatedBy = model.UpdatedBy;
                        leadAssign.UpdatedOn = DateTime.Now;
                    }
                    else
                    {
                        // Create
                        leadAssign = new LeadAssign
                        {
                            StaffId = model.StaffId,
                            LeadCategoryId = model.LeadCategoryId,
                            LeadDate = model.LeadDate,
                            LeadDetail = model.LeadDetail,
                            Attachment = model.Attachment,
                            FileType = model.FileType,
                            Instruction = model.Instruction,
                            Status = model.Status,
                            CreatedBy = model.CreatedBy,
                            CreatedOn = DateTime.Now
                        };

                        dbContext.LeadAssigns.Add(leadAssign);
                    }

                    dbContext.SaveChanges();
                    response.Message = ConstantData.SuccessMessage;
                }
            }
            catch (Exception ex)
            {
                response.Message = "Error: " + ex.Message;
            }

            return response;
        }


        [HttpPost]
        [Route("DeleteLeadAssign")]
        public ExpandoObject DeleteLeadCategory(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                RaiverCRMEntities dataContext = new RaiverCRMEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                if (string.IsNullOrEmpty(AppKey))
                {
                    response.Message = "AppKey is missing";
                    return response;
                }
                AppData.CheckAppKey(dataContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                LeadAssign model = JsonConvert.DeserializeObject<LeadAssign>(decryptData);
                var leadAssign = dataContext.LeadAssigns.First(x => x.LeadId == model.LeadId);
                if (leadAssign == null)
                {
                    response.Message = "LeadCatogery not found";
                    return response;
                }
                dataContext.LeadAssigns.Remove(leadAssign);
                dataContext.SaveChanges();
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }


    }
}
