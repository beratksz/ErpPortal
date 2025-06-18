using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace ErpPortal.Web.Filters
{
    /// <summary>
    ///   İstek yapan kullanıcının Session'da seçili İş Merkezi kodu "QUALITY" ise devam eder;
    ///   aksi halde 403 (Forbid) döndürür.
    /// </summary>
    public sealed class RequireQualityWorkCenterAttribute : ActionFilterAttribute
    {
        private const string QualityCode = "QUALITY"; // WorkCenter.Code sabiti

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var wc = session.GetString("WorkCenterCode");
            if (!string.Equals(wc, QualityCode, System.StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ForbidResult();
                return;
            }
            base.OnActionExecuting(context);
        }
    }
} 