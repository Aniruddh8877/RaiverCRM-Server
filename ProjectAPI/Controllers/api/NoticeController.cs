using Newtonsoft.Json;
using Project;
using ProjectAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Notice")]
    public class NoticeController : ApiController
    {
        [HttpPost]
        [Route("SaveNotice")]
        public ExpandoObject SaveNotic(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();

            using (RaiverCRMEntities db = new RaiverCRMEntities())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                        AppData.CheckAppKey(db, AppKey, (byte)KeyFor.Admin);

                        var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                        Notice model = JsonConvert.DeserializeObject<Notice>(decryptData);

                        Notice notice;

                        if (model.NoticeId > 0)
                        {
                            // Update existing notice
                            notice = db.Notices.FirstOrDefault(n => n.NoticeId == model.NoticeId);

                            if (notice != null)
                            {
                                notice.NoticeDate = model.NoticeDate;
                                notice.NoticeDetail = model.NoticeDetail;
                                notice.NoticeTitle = model.NoticeTitle;
                                notice.NoticeStatus = model.NoticeStatus;
                                notice.UpdatedBy = model.UpdatedBy;
                                notice.UpdaedOn = DateTime.Now;

                                // Update attachment only if changed
                                if (!string.IsNullOrEmpty(model.Attachments) && model.Attachments != notice.Attachments)
                                {
                                    notice.Attachments = Utils.SaveFile(model.Attachments, ConstantString.FileLocation, model.FileFormat);
                                    notice.FileFormat = model.FileFormat;
                                }
                            }
                        }
                        else
                        {
                            // Create new notice
                            notice = new Notice
                            {
                                StaffId = model.StaffId,
                                NoticeDate = model.NoticeDate,
                                NoticeDetail = model.NoticeDetail,
                                NoticeTitle = model.NoticeTitle,
                                NoticeStatus = model.NoticeStatus,
                                CreatedBy = model.CreatedBy,
                                CreatedOn = DateTime.Now
                            };

                            if (!string.IsNullOrEmpty(model.Attachments))
                            {
                                notice.Attachments = Utils.SaveFile(model.Attachments, ConstantString.FileLocation, model.FileFormat);
                                notice.FileFormat = model.FileFormat;
                            }

                            db.Notices.Add(notice);
                        }

                        db.SaveChanges();
                        tran.Commit();

                        response.Message = "Success";
                        response.NoticeId = notice.NoticeId;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        response.Message = ex.Message;
                    }
                }
            }

            return response;
        }


        [HttpPost]
        [Route("NoticeList")]
        public ExpandoObject NoticeList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();

            try
            {
                using (var db = new RaiverCRMEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(db, appKey, (byte)KeyFor.Admin);

                    var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    Notice model = JsonConvert.DeserializeObject<Notice>(decryptData);

                    var list = (from n in db.Notices
                                where model.StaffId == 5 || n.StaffId == model.StaffId // ✅ filter by StaffId if provided
                                orderby n.NoticeDate
                                select new
                                {
                                    n.NoticeId,
                                    n.NoticeDate,
                                    n.NoticeTitle,
                                    n.NoticeDetail,
                                    n.Attachments,
                                    n.FileFormat,
                                    n.NoticeStatus,
                                    n.CreatedOn,
                                    n.UpdatedBy,
                                    n.UpdaedOn,
                                    n.CreatedBy,
                                    n.Staff.StaffName,
                                    n.Staff.StaffCode,
                                }).ToList();

                    response.NoticeList = list;
                    response.Message = ConstantData.SuccessMessage;

                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }



        //[HttpPost]
        //[Route("DeleteNotice")]
        //public ExpandoObject DeteteNotice(RequestModel requestModel)
        //{
        //    dynamic res = new ExpandoObject();
        //    try
        //    {

        //    }
        //    catch(Exception ex)
        //    {
        //        res.Message = ex.Message;
        //    }
        //    return res;
        //}
    }

}
