
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AjaxErrorHandlerAttribute : FilterAttribute, IExceptionFilter
    {
        /// <summary>
        /// Disparado quando ocorre um erro <see cref="ModelStateException"/> object.
        /// </summary>
        /// <param name="filterContext">Filter context.</param>
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest() && filterContext.Exception != null)
            {
                filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                filterContext.Result = new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        filterContext.Exception.Message,
                        filterContext.Exception.StackTrace,
                        ExceptionType = filterContext.Exception.GetType().ToString()
                    }
                };
                filterContext.ExceptionHandled = true;
            }

        }
    }

/*
Para utilizar assim no jquery:

numa master page:
        $.ajaxSetup({
            //type: "POST",
            cache: false,
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                var jsonErro = JSON.parse(XMLHttpRequest.responseText);
		alert(jsonErro.Message);
            }
        });

*/