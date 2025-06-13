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
    [RoutePrefix("api/LeadCategroy")]
    public class LeadCategoryController : ApiController
    {
        [HttpPost]
        [Route("LeadCategoryList")]
        public ExpandoObject LeadCategoryList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                RaiverCRMEntities dbContext = new RaiverCRMEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                LeadCategory model = JsonConvert.DeserializeObject<LeadCategory>(decryptData);

                var list = (from d1 in dbContext.LeadCategories
                            select new
                            {
                                d1.LeadCategoryId,
                                d1.LeadCategoryName,
                                d1.Status,
                                d1.CreatedBy,
                                d1.CreatedOn,
                                d1.UpdatedBy,
                                d1.UpdatedOn,
                            }).ToList();

                response.LeadCategoryList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch(Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("SaveLeadCategory")]
        public ExpandoObject SaveLeadCategory(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                RaiverCRMEntities dbContext = new RaiverCRMEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                LeadCategory model = JsonConvert.DeserializeObject<LeadCategory>(decryptData);

                LeadCategory leadCategory = null;
                if (model.LeadCategoryId > 0)
                {
                    leadCategory = dbContext.LeadCategories.FirstOrDefault(x => x.LeadCategoryId == model.LeadCategoryId);
                    if (leadCategory == null)
                    {
                        response.Message = "Lead Category not found.";
                        return response;
                    }

                    leadCategory.LeadCategoryName = model.LeadCategoryName;
                    leadCategory.Status = model.Status;
                    leadCategory.UpdatedBy = model.UpdatedBy;
                    leadCategory.UpdatedOn = model.UpdatedOn;
                }
                else
                {

                    leadCategory = new LeadCategory
                    {
                        LeadCategoryName = model.LeadCategoryName,
                        Status = model.Status,
                        CreatedBy = model.CreatedBy,
                        CreatedOn = DateTime.Now
                    };

                    
                    dbContext.LeadCategories.Add(leadCategory);
                }
                dbContext.SaveChanges();
                response.Message = ConstantData.SuccessMessage;
            }
            catch(Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }


        [HttpPost]
        [Route("DeleteLeadCategory")]
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
                LeadCategory model = JsonConvert.DeserializeObject<LeadCategory>(decryptData);
                var leadCategory = dataContext.LeadCategories.First(x => x.LeadCategoryId == model.LeadCategoryId);
                if (leadCategory == null)
                {
                    response.Message = "LeadCatogery not found";
                    return response;
                }


                dataContext.LeadCategories.Remove(leadCategory);
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
