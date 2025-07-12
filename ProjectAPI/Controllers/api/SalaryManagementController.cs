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
    [RoutePrefix("api/SalaryManagement")]
    public class SalaryManagementController : ApiController
    {
        [HttpPost]
        [Route("SalaryManagementList")]
        public ExpandoObject SalaryManagementList(RequestModel requestModel)
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

                    var result = (from s in dbContext.SalaryManagements
                                  join staff in dbContext.Staffs on s.StaffId equals staff.StaffId
                                  orderby s.SalaryId descending
                                  select new
                                  {
                                      s.SalaryId,
                                      s.StaffId,
                                      staff.StaffName,
                                      staff.StaffCode,
                                      s.Months,
                                      s.TotalWorkingDay,
                                      s.AbsentDay,
                                      s.WorkingDay,
                                      s.PaymentDate,
                                      s.PaymentMode,
                                      s.Amount,
                                      s.BasicSalary,
                                  }).ToList();

                    response.Message = "Success";
                    response.SalaryList = result;
                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                }
            }

            return response;
        }

        [HttpPost]
        [Route("SaveSalaryManagement")]
        public ExpandoObject SaveSalaryManagement(RequestModel requestModel)
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
                        SalaryManagement model = JsonConvert.DeserializeObject<SalaryManagement>(decryptData);

                        SalaryManagement salary;
                        if (model.SalaryId > 0)
                        {
                            // Update
                            salary = dbContext.SalaryManagements.FirstOrDefault(s => s.SalaryId == model.SalaryId);
                            if (salary != null)
                            {
                                salary.StaffId = model.StaffId;
                                salary.Months = model.Months;
                                salary.TotalWorkingDay = model.TotalWorkingDay;
                                salary.AbsentDay = model.AbsentDay;
                                salary.WorkingDay = model.WorkingDay;
                                salary.Amount = model.Amount;
                                salary.BasicSalary = model.BasicSalary;
                                salary.PaymentDate = DateTime.Now;
                                salary.PaymentMode = (int)PaymentMode.CASH;
                                salary.Remarks = model.Remarks;
                            }
                        }
                        else
                        {
                            // Insert
                            salary = new SalaryManagement
                            {
                                StaffId = model.StaffId,
                                Months = model.Months,
                                TotalWorkingDay = model.TotalWorkingDay,
                                AbsentDay = model.AbsentDay,
                                WorkingDay = model.WorkingDay,
                                Amount = model.Amount,
                                BasicSalary = model.BasicSalary,
                                PaymentDate = DateTime.Now,
                                PaymentMode = (int)PaymentMode.CASH,
                                Remarks = model.Remarks
                            };
                            dbContext.SalaryManagements.Add(salary);
                        }

                        dbContext.SaveChanges();
                        transaction.Commit();

                        response.Message = "Success";
                        response.SalaryId = salary.SalaryId;
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