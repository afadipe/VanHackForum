namespace SleekSoftMVCFramework.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateTbl_chnages : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Application", "ApplicationName", c => c.String(nullable: false));
            AlterColumn("dbo.Application", "Description", c => c.String());
            AlterColumn("dbo.Application", "HasAdminUserConfigured", c => c.Boolean());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Application", "HasAdminUserConfigured", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Application", "Description", c => c.String(nullable: false));
            AlterColumn("dbo.Application", "ApplicationName", c => c.String());
        }
    }
}
