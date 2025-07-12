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
                            notice = db.Notices.FirstOrDefault(n => n.NoticeId == model.NoticeId);
                            if (notice != null)
                            {
                                notice.Noticedate = model.Noticedate;
                                notice.NoticeDetail = model.NoticeDetail;
                                notice.Attachments = model.Attachments;
                                notice.FileFormate = model.FileFormate;
                                notice.NoticeStatus = model.NoticeStatus;
                                notice.UpdatedBy = model.UpdatedBy;
                                notice.UpdaedOn = DateTime.Now;
                            }
                        }
                        else
                        {
                            notice = new Notice
                            {
                                StaffId = model.StaffId,
                                Noticedate = model.Noticedate,
                                NoticeDetail = model.NoticeDetail,
                                Attachments = model.Attachments,
                                FileFormate = model.FileFormate,
                                NoticeStatus = model.NoticeStatus,
                                CreatedBy = model.CreatedBy,
                                CreatedOn = DateTime.Now
                            };
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
        public ExpandoObject NoticList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();

            using (RaiverCRMEntities db = new RaiverCRMEntities())
            {
                try
                {
                    string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(db, AppKey, (byte)KeyFor.Admin);

                    var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    dynamic data = JsonConvert.DeserializeObject<ExpandoObject>(decryptData);

                    var list = (from n in db.Notices
                                join s in db.Staffs on n.StaffId equals s.StaffId
                                orderby n.NoticeId descending
                                select new
                                {
                                    n.NoticeId,
                                    n.Noticedate,
                                    n.NoticeDetail,
                                    n.Attachments,
                                    n.FileFormate,
                                    n.NoticeStatus,
                                    n.CreatedOn,
                                    n.UpdatedBy,
                                    n.UpdaedOn,
                                    n.CreatedBy,
                                    StaffName = s.StaffName,
                                    StaffCode = s.StaffCode
                                }).ToList();

                    response.Message = "Success";
                    response.NoticeList = list;
                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                }
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
