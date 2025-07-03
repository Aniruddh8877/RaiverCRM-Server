using System;
using System.Collections.Generic;
using System.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
using System.Web;
using System.Web.Http;
using System.Dynamic;
using Newtonsoft.Json;
using Project;
using ProjectAPI.Models;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/LeadDetial")]
    public class LeadDetailController : ApiController
    {
        [HttpPost]
        [Route("LeadDetailList")]
        public ExpandoObject LeadDeatilList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                using (RaiverCRMEntities dbContext = new RaiverCRMEntities())
                {
                    string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);

                    var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    LeadDetail model = JsonConvert.DeserializeObject<LeadDetail>(decryptData);


                    var leadDetails = (from ld in dbContext.LeadDetails
                                       join la in dbContext.LeadAssigns
                                       on ld.LeadId equals la.LeadId into leadAssignJoin
                                       from la in leadAssignJoin.DefaultIfEmpty() // LEFT JOIN, use only if LeadAssign may not exist
                                       where ld.LeadId == model.LeadId
                                       select new
                                       {
                                           ld.LeadDetailId,
                                           LeadDate = la.LeadDate,      // This comes from LeadAssign table
                                           ld.LeadId,
                                           ld.LeadName,
                                           ld.LeadMobileNo,
                                           ld.LeadComment,
                                           ld.LeadStatus,
                                           ld.Comment,
                                           ld.FollowUpDate
                                       }).ToList();


                    response.LeadDeatilList = leadDetails;
                    response.Count = leadDetails.Count;
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
        [Route("SaveLeadDetail")]
        public ExpandoObject SaveLeadDetail(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                using (RaiverCRMEntities dbContext = new RaiverCRMEntities())
                {
                    string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);

                    var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    LeadDetail updatedModel = JsonConvert.DeserializeObject<LeadDetail>(decryptData);

                    var existingLeadDetail = dbContext.LeadDetails.FirstOrDefault(x => x.LeadDetailId == updatedModel.LeadDetailId);
                    if (existingLeadDetail != null)
                    {
                        // Update fields
                        existingLeadDetail.LeadName = updatedModel.LeadName;
                        existingLeadDetail.LeadMobileNo = updatedModel.LeadMobileNo;
                        existingLeadDetail.LeadComment = updatedModel.LeadComment;
                        existingLeadDetail.LeadStatus = updatedModel.LeadStatus;
                        existingLeadDetail.Comment = updatedModel.Comment;
                        existingLeadDetail.FollowUpDate = updatedModel.FollowUpDate;

                        dbContext.SaveChanges();

                        response.Message = ConstantData.SuccessMessage;
                    }
                    else
                    {
                        response.Message = "Lead Detail not found.";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}

