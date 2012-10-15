using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Concursos.Infra.NH;
using System.IO;

/************
 *  Adicionar no web.config
 *  <httpModules>
      <add name="NHSessionModule" type="NamespaceDaApp.NHSessionModule" />
    </httpModules>
 *
 */


public class NHSessionModule : IHttpModule
{

    private static readonly string[] NoPersistenceFileExtensions = new string[] { ".jpg", ".gif", ".png", ".css", ".js", ".swf", ".xap" };


    private void ApplicationBeginRequest(object sender, EventArgs e)
    {
        if (RequestMayNeedIterationWithPersistence(sender as HttpApplication))
        {
            NHibernateHelper.BeginTransaction();
        }
    }

    private void ApplicationEndRequest(object sender, EventArgs e)
    {
        if (RequestMayNeedIterationWithPersistence(sender as HttpApplication))
        {
            try
            {
                if (NHibernateHelper.GetCurrentSession().IsOpen && NHibernateHelper.GetCurrentSession().Transaction.IsActive)
                {
                    NHibernateHelper.CommitTransaction();
                }
            }
            catch (Exception)
            {
                NHibernateHelper.RollbackTransaction();
                throw;
            }
            finally
            {
                NHibernateHelper.DisposeSession();
            }
        }
    }


    public void Init(HttpApplication context)
    {
        context.BeginRequest += new EventHandler(this.ApplicationBeginRequest);
        context.EndRequest += new EventHandler(this.ApplicationEndRequest);
    }

    private static bool RequestMayNeedIterationWithPersistence(HttpApplication application)
    {
        if (application == null)
        {
            return false;
        }
        HttpContext context = application.Context;
        if (context == null)
        {
            return false;
        }
        string extension = Path.GetExtension(context.Request.PhysicalPath);
        return ((extension != null) && (Array.IndexOf<string>(NoPersistenceFileExtensions, extension.ToLower()) < 0));
    }

    public void Dispose()
    {

    }
}