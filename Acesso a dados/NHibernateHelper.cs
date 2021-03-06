using NHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using System.Reflection;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using Infra.NH.Conventions;
using NHibernate.Context;
using System;
using System.IO;

public static class NHibernateHelper
{
    private static ISessionFactory _sessionFactory;
    private static object syncRoot = new Object();

    public static ISessionFactory SessionFactory
    {
        get
        {
            if (_sessionFactory == null)
            {
                lock (syncRoot)
                {
                    if (_sessionFactory == null)
                    {

                        System.Runtime.Serialization.IFormatter serializer = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        NHibernate.Cfg.Configuration cfg = null;
                        string arquivoComCaminho = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration.serialized");

                        if (File.Exists(arquivoComCaminho))
                        {
                            using (Stream stream = File.OpenRead(arquivoComCaminho))
                            {
                                cfg = serializer.Deserialize(stream) as Configuration;
                            }
                        }
                        else
                        {

                            cfg = Fluently.Configure()
                                .Database(MsSqlConfiguration.MsSql2008
                                .ConnectionString(c => c.FromConnectionStringWithKey("Conexao")))
                                .Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly())
                                                               .Conventions.AddFromAssemblyOf<ReferenceConvention>())
                                //.ExposeConfiguration(BuildSchema)
                                .ExposeConfiguration(p => p.SetProperty("current_session_context_class", "web"))
                                .BuildConfiguration();

                            using (Stream stream = File.OpenWrite(arquivoComCaminho))
                            {
                                serializer.Serialize(stream, cfg);
                            }
                        }


                        _sessionFactory = cfg.BuildSessionFactory();
                        return _sessionFactory;
                    }
                }
            }
            return _sessionFactory;
        }
    }



    private static void BuildSchema(Configuration config)
    {
        SchemaExport schema = new SchemaExport(config);
        schema.Drop(false, true);
        schema.Create(false, true);
    }

    public static ISession GetCurrentSession()
    {
        if (!CurrentSessionContext.HasBind(SessionFactory))
        {
            CurrentSessionContext.Bind(SessionFactory.OpenSession());
        }
        return SessionFactory.GetCurrentSession();
    }

    public static void DisposeSession()
    {
        var session = GetCurrentSession();
        session.Close();
        session.Dispose();
    }

    public static void BeginTransaction()
    {
        GetCurrentSession().BeginTransaction();
    }

    public static void CommitTransaction()
    {
        var session = GetCurrentSession();
        if (session.Transaction.IsActive)
            session.Transaction.Commit();
    }

    public static void RollbackTransaction()
    {
        var session = GetCurrentSession();
        if (session.Transaction.IsActive)
            session.Transaction.Rollback();
    }
}
