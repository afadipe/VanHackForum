namespace SleekSoftMVCFramework.Data.Migrations
{
    using AppEntities;
    using Entities;
    using IdentityModel;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<SleekSoftMVCFramework.Data.APPContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(SleekSoftMVCFramework.Data.APPContext context)
        {

            context.ApplicationRoles.AddOrUpdate(p => p.Name, new ApplicationRole { Name = "PortalAdmin", DateCreated = DateTime.Now, IsActive = true, IsDeleted = false });
            context.ApplicationRoles.AddOrUpdate(p => p.Name, new ApplicationRole { Name = "ForumUser", DateCreated = DateTime.Now, IsActive = true, IsDeleted = false });


            context.TopicCategories.AddOrUpdate(p => p.Title, new TopicCategory { Title = "Entertainment", DateCreated = DateTime.Now, IsActive = true, IsDeleted = false });
            context.TopicCategories.AddOrUpdate(p => p.Title, new TopicCategory { Title = "Science/Technology", DateCreated = DateTime.Now, IsActive = true, IsDeleted = false });

            context.TopicCategories.AddOrUpdate(p => p.Title, new TopicCategory { Title = "Politics", DateCreated = DateTime.Now, IsActive = true, IsDeleted = false });
            context.TopicCategories.AddOrUpdate(p => p.Title, new TopicCategory { Title = "Religion", DateCreated = DateTime.Now, IsActive = true, IsDeleted = false });

            context.TopicCategories.AddOrUpdate(p => p.Title, new TopicCategory { Title = "Agriculture", DateCreated = DateTime.Now, IsActive = true, IsDeleted = false });
            context.TopicCategories.AddOrUpdate(p => p.Title, new TopicCategory { Title = "Education", DateCreated = DateTime.Now, IsActive = true, IsDeleted = false });

            context.TopicCategories.AddOrUpdate(p => p.Title, new TopicCategory { Title = "Career", DateCreated = DateTime.Now, IsActive = true, IsDeleted = false });
            context.TopicCategories.AddOrUpdate(p => p.Title, new TopicCategory { Title = "Investment", DateCreated = DateTime.Now, IsActive = true, IsDeleted = false });


            context.PortalVersions.AddOrUpdate(p => p.FrameworkName,
               new PortalVersion
               {
                   FrameworkName = "SMF MVC Framework",
                   FrameworkDescription = "An MVC Customized Framework built on ASP.Net Identity 2.0 to aid fast application development with built in logger and activitylog",
                   FrameworkVersion = "2.0.0.0",
                   TargetServer = "MSSQL,Postgress,MangoDB,MYSQL",
                   PackagesUsed = "Microsoft.ASPNET.Identity,Microsoft.OWIN,Log4net,EntityFramework,JQuery DataTable,Select 2,Boostrap,Autofac,Autofac.MVC,Autofac.WebAPI2,CQRS RepositoryPattern",
                   DefaultDatabaseEngine = "MSSQL Server",
                   IOC = "Autofac",
                   DateCreated = DateTime.Now,
                   DevelopedBy = "Fadipe Ayobami  || ayfadipe@gmail.com",
                   UX = "Open Source AdminLTE2 Template"
               });


            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
