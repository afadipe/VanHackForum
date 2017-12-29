namespace SleekSoftMVCFramework.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class spusermegt : DbMigration
    {
        public override void Up()
        {
            CreateStoredProcedure("dbo.spFetchUserPermissionAndRole",
           p => new
           {
               UserId = p.Int()
           },
           body:

           @"select p.Name PermissionName,p.Code PermissionCode,r.Name RoleName
                    ,rp.PermissionId,rp.RoleId
                    from [AspNetUsers] e  
                    inner join [AspNetUserRole] ur on e.AspNetUserId=ur.AspNetUserId
                    inner join [AspNetRole] r on ur.AspNetRoleId=r.AspNetRoleId
                    inner join [RolePermission] rp on ur.AspNetRoleId=rp.RoleId
                    inner join [Permission] p on rp.PermissionId=p.Id
                    where (e.AspNetUserId =@UserId)"
       );


            CreateStoredProcedure("dbo.spPasswordHistorySelect",
              p => new
              {
                  UserId = p.Int(),
                  numberOfRecentPasswordsToKeep = p.Int()
              },
              body:
              @"SELECT TOP(@numberOfRecentPasswordsToKeep)
                    HashPassword,
                    PasswordSalt
                FROM
                    dbo.PasswordHistory
                WHERE
                    AspNetUserId = @UserId
                ORDER BY
                    DateCreated DESC"
          );



            CreateStoredProcedure("dbo.spPasswordHistoryDeleteNonRecentPasswords",
               p => new
               {
                   UserId = p.Int(),
                   numberOfRecentPasswordsToKeep = p.Int()
               },
               body:

               @"DECLARE @minimumDate DATETIME
               SELECT @minimumDate = MIN(DateCreated) FROM  dbo.PasswordHistory
               WHERE
                AspNetUserId = @userId
                AND DateCreated IN
                (
                    SELECT
                        TOP(@numberOfRecentPasswordsToKeep) DateCreated
                    FROM
                        dbo.PasswordHistory
                    WHERE
                        AspNetUserId = @UserId
                    ORDER BY
                        DateCreated DESC
                )
             IF(@minimumDate IS NOT NULL)
            BEGIN
                DELETE FROM dbo.PasswordHistory
                WHERE AspNetUserId = @UserId AND DateCreated < @minimumDate
            END"
           );


            CreateStoredProcedure("dbo.spDeletePermissionByRoleID",
                p => new
                {
                    RoleId = p.Int()
                },
                body:
                @"delete from RolePermission where RoleId= @RoleId"
            );


            CreateStoredProcedure("dbo.SpGetActivityLog",
              p => new
              {
                  UserId = p.Int(0),
                  ControllerName = p.String(null),
                  StartDate = p.DateTime(null),
                  EndDate = p.DateTime(null)
              },
              body:
              @"select  isnull(e.FirstName + ' ' + e.MiddleName + ' ' + e.LastName,'N/A') FullName,
	              a.Id,
                  a.[UserID]
                  ,a.[Description]
                  ,a.[DateCreated]
                  ,a.[ModuleName]
                  ,a.[ModuleAction]
                  ,isnull(a.[Record],'N/A') Record
              FROM [dbo].[ActivityLog] a
              left outer join [dbo].[AspNetUsers] e on  a.[UserID]=e.AspNetUserId
              where (@UserId=0 or e.AspNetUserId=@UserId)
              and (@ControllerName='' or a.ModuleName=@ControllerName)
              and  
                (a.DateCreated >= @StartDate OR @StartDate is null)
                AND
                (a.DateCreated  <= @EndDate OR  @EndDate is null)
              order by a.Id desc");


            CreateStoredProcedure("dbo.SpGetAllActivityLog",
              body:
              @"select  isnull(e.FirstName + ' ' + e.MiddleName + ' ' + e.LastName,'N/A') FullName,
	            a.Id,
                a.[UserID]
                ,a.[Description]
                ,a.[DateCreated]
                ,a.[ModuleName]
                ,a.[ModuleAction]
                ,isnull(a.[Record],'N/A') Record
            FROM [dbo].[ActivityLog] a
            left outer join [dbo].[AspNetUsers] e on  a.[UserID]=e.AspNetUserId
            order by a.Id desc");

            CreateStoredProcedure("dbo.SpGetUserRole",
       p => new
       {
           UserId = p.Int(0)
       },
       body:
       @"select  
	              a.AspNetRoleId RoleId
                  ,e.[Name] 
              FROM [dbo].[AspNetUserRole] a
              inner join [dbo].[AspNetRole] e on  a.[AspNetRoleId]=e.AspNetRoleId
              where (@UserId=0 or a.AspNetUserId=@UserId)
              ");


            CreateStoredProcedure("dbo.SpDeleteUserRoleByUserId",
             p => new
             {
                 UserId = p.Int(0)
             },
             body:
             @"Delete from AspNetUserRole  where AspNetRoleId=@UserId");
        }
        
        public override void Down()
        {
            DropStoredProcedure("dbo.spFetchUserPermissionAndRole");
            DropStoredProcedure("dbo.spPasswordHistorySelect");
            DropStoredProcedure("dbo.spPasswordHistoryDeleteNonRecentPasswords");
            DropStoredProcedure("dbo.spDeletePermissionByRoleID");

            DropStoredProcedure("dbo.SpGetActivityLog");
            DropStoredProcedure("dbo.SpGetAllActivityLog");
            DropStoredProcedure("dbo.SpGetUserRole");
            DropStoredProcedure("dbo.SpDeleteUserRoleByUserId");
        }
    }
}
