namespace SleekSoftMVCFramework.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class spQuery : DbMigration
    {
        public override void Up()
        {

            CreateStoredProcedure("dbo.SpGetLastPostUserByTopicId",
          p => new
          {
              TopicId = p.Int()
          },
         body:
         @"declare @CreateduserId int
        declare @PostId int
        declare @DateCreated varchar(Max)
        SELECT Top 1 @CreateduserId=p.CreatedBy,
        @DateCreated=convert(varchar(10),p.[DateCreated], 111) from [Posts] p 
        where p.TopicId=@TopicId 
        order by p.Id desc

            SELECT  a.[AspNetUserId]
          ,a.[FirstName]  + ' ' + a.[LastName] FullName
          ,a.[Email]
          ,a.[UserName]
          ,a.[ProfileImage] Avatar
	      ,@DateCreated DatePosted
          FROM  [AspNetUsers] a
          where a.[AspNetUserId]=@CreateduserId");

            CreateStoredProcedure("dbo.SpGetTopicByTopicId",
              p => new
              {
                  TopicId = p.Int()
              },
             body:
             @"SELECT  t.[Id]
                      , t.[Title]
                      , t.[Description]
                      , t.[TopicImage]
                      , isnull(t.[CreatedBy],0) CreatedBy
	                  ,a.LastName + ' ' + a.FirstName CreateByFullName
	                  ,a.UserName  CreateByUserName
                      ,a.[ProfileImage] CreateByUserAvatar
                      , CONVERT(VARCHAR(12),t.[DateCreated], 113) DateCreated
	                  , convert(varchar(10),t.[DateCreated], 111)  DateCreated2
	                  ,DATEDIFF(dd,GetDate(),t.[DateCreated]) NoOfDays
                      , t.[CategoryId]
	                  ,tc.Title  TopicCategory
                      ,(select count(*) from Posts where TopicId=t.Id) NoOfPost
                      ,isnull(t.[ViewCount],0) TopicView
                  FROM [Topics] t
                  inner join [TopicCategories] tc on  t.[CategoryId]=tc.Id
                  inner join [AspNetUsers]  a on t.[CreatedBy]=a.AspNetUserId
                  where   t.[IsDeleted]=0 and  t.[IsActive]=1 and t.Id=@TopicId");

            CreateStoredProcedure("dbo.SpGetAllPostByTopicId",
                  p => new
                  {
                      TopicId = p.Int()
                  },
                 body:
                 @"SELECT  p.[Id]
                  ,p.[TopicId]
                  ,p.[IpAddress] IPAddress
                  ,p.[Content]
                  ,p.[VoteCount]
                  ,p.[IsTopicStarter] 
	            , isnull(p.[CreatedBy],0) CreatedBy
                ,a.LastName + ' ' + a.FirstName CreateByFullName
                ,a.UserName  CreateByUserName
	            ,a.[ProfileImage] PostCreateByUserAvatar
	            , CONVERT(VARCHAR(12),p.[DateCreated], 113) DateCreated
	            , convert(varchar(10),p.[DateCreated], 111)  DateCreated2

              FROM [Posts] p
             inner join [AspNetUsers]  a on p.[CreatedBy]=a.AspNetUserId
             where  p.[IsDeleted]=0 and  p.[IsActive]=1  and  p.[TopicId]=@TopicId
             order by p.Id asc");

            CreateStoredProcedure("dbo.SpGetAllTopic",
     body:
     @"SELECT  t.[Id]
                      , t.[Title]
                      , t.[Description]
                      , t.[TopicImage]
                      , isnull(t.[CreatedBy],0) CreatedBy
	                  ,a.LastName + ' ' + a.FirstName CreateByFullName
	                  ,a.UserName  CreateByUserName
                      ,a.[ProfileImage] CreateByUserAvatar
                      , CONVERT(VARCHAR(12),t.[DateCreated], 113) DateCreated
	                  , convert(varchar(10),t.[DateCreated], 111)  DateCreated2
	                  ,DATEDIFF(dd,GetDate(),t.[DateCreated]) NoOfDays
                      , t.[CategoryId]
	                  ,tc.Title  TopicCategory
                      ,(select count(*) from Posts where TopicId=t.Id) NoOfPost
                      ,isnull(t.[ViewCount],0) TopicView
                  FROM [Topics] t
                  inner join [TopicCategories] tc on  t.[CategoryId]=tc.Id
                  inner join [AspNetUsers]  a on t.[CreatedBy]=a.AspNetUserId
                  where   t.[IsDeleted]=0 and  t.[IsActive]=1");


            CreateStoredProcedure("dbo.SpGetAllTopicByUserId",
            p => new
            {
                UserId = p.Int()
            },
           body:
           @"SELECT  t.[Id]
                      , t.[Title]
                      , t.[Description]
                      , t.[TopicImage]
                      , isnull(t.[CreatedBy],0) CreatedBy
	                  ,a.LastName + ' ' + a.FirstName CreateByFullName
	                  ,a.UserName  CreateByUserName
                      ,a.[ProfileImage] CreateByUserAvatar
                      , CONVERT(VARCHAR(12),t.[DateCreated], 113) DateCreated
	                  , convert(varchar(10),t.[DateCreated], 111)  DateCreated2
	                  ,DATEDIFF(dd,GetDate(),t.[DateCreated]) NoOfDays
                      , t.[CategoryId]
	                  ,tc.Title  TopicCategory
                      ,(select count(*) from Posts where TopicId=t.Id) NoOfPost
                      ,isnull(t.[ViewCount],0) TopicView
                  FROM [Topics] t
                  inner join [TopicCategories] tc on  t.[CategoryId]=tc.Id
                  inner join [AspNetUsers]  a on t.[CreatedBy]=a.AspNetUserId
                  where   t.[IsDeleted]=0 and  t.[IsActive]=1  and  t.CreatedBy=@UserId");


            CreateStoredProcedure("dbo.SpGetAllPostByTopicIdByUserId",
              p => new
              {
                  TopicId = p.Int(),
                  UserId = p.Int()
              },
             body:
             @"declare @TopicViewCount int
                select @TopicViewCount=isnull(ViewCount,0) from Topics where Id= @TopicId
                update Topics set ViewCount=(@TopicViewCount + 1) where Id= @TopicId
                    SELECT  p.[Id]
                    ,p.[TopicId]
                    ,p.[IpAddress] IPAddress
                    ,p.[Content]
                    ,p.[VoteCount]
                    ,p.[IsTopicStarter] 
    	                            , isnull(p.[CreatedBy],0) CreatedBy
                    ,a.LastName + ' ' + a.FirstName CreateByFullName
                    ,a.UserName  CreateByUserName
    	                            ,a.[ProfileImage] PostCreateByUserAvatar
    	                            , CONVERT(VARCHAR(12),p.[DateCreated], 113) DateCreated
    	                            , convert(varchar(10),p.[DateCreated], 111)  DateCreated2
					                ,(select count(*) from PostLikes where  PostId=p.[Id] and  TopicId= @TopicId and CreatedBy=@UserId and IsDeleted=0 and IsActive=1) HasVoted
    
                    FROM [Posts] p
                    inner join [AspNetUsers]  a on p.[CreatedBy]=a.AspNetUserId
                    where  p.[IsDeleted]=0 and  p.[IsActive]=1  and  p.[TopicId]=@TopicId
                    order by p.Id asc");



          

            CreateStoredProcedure("dbo.SpGetAllTopicCeatedToday",
             body:
             @" SELECT  t.[Id]
            , t.[Title]
            , t.[Description]
            , t.[TopicImage]
            , isnull(t.[CreatedBy],0) CreatedBy
    	                          ,a.LastName + ' ' + a.FirstName CreateByFullName
    	                          ,a.UserName  CreateByUserName
            ,a.[ProfileImage] CreateByUserAvatar
            , CONVERT(VARCHAR(12),t.[DateCreated], 113) DateCreated
    	                          , convert(varchar(10),t.[DateCreated], 111)  DateCreated2
    	                          ,DATEDIFF(dd,GetDate(),t.[DateCreated]) NoOfDays
            , t.[CategoryId]
    	                          ,tc.Title  TopicCategory
            ,(select count(*) from Posts where TopicId=t.Id) NoOfPost
            ,isnull(t.[ViewCount],0) TopicView
            FROM [Topics] t
            inner join [TopicCategories] tc on  t.[CategoryId]=tc.Id
            inner join [AspNetUsers]  a on t.[CreatedBy]=a.AspNetUserId
            where   t.[IsDeleted]=0 and  t.[IsActive]=1 and t.DateCreated=GETDATE()");


            CreateStoredProcedure("dbo.SpGetAllMostViewedTopic",
             body:
             @" SELECT  t.[Id]
            , t.[Title]
            , t.[Description]
            , t.[TopicImage]
            , isnull(t.[CreatedBy],0) CreatedBy
    	                          ,a.LastName + ' ' + a.FirstName CreateByFullName
    	                          ,a.UserName  CreateByUserName
            ,a.[ProfileImage] CreateByUserAvatar
            , CONVERT(VARCHAR(12),t.[DateCreated], 113) DateCreated
    	                          , convert(varchar(10),t.[DateCreated], 111)  DateCreated2
    	                          ,DATEDIFF(dd,GetDate(),t.[DateCreated]) NoOfDays
            , t.[CategoryId]
    	                          ,tc.Title  TopicCategory
            ,(select count(*) from Posts where TopicId=t.Id) NoOfPost
            ,isnull(t.[ViewCount],0) TopicView
            FROM [Topics] t
            inner join [TopicCategories] tc on  t.[CategoryId]=tc.Id
            inner join [AspNetUsers]  a on t.[CreatedBy]=a.AspNetUserId
            where   t.[IsDeleted]=0 and  t.[IsActive]=1
            order by t.[ViewCount]");


           

            CreateStoredProcedure("dbo.SpGetTopicByTopicIdByUserId",
               p => new
               {
                   TopicId = p.Int(),
                   UserId = p.Int()
               },
         body:
         @" SELECT  t.[Id]
                    , t.[Title]
                    , t.[Description]
                    , t.[TopicImage]
                    , isnull(t.[CreatedBy],0) CreatedBy
    	                            ,a.LastName + ' ' + a.FirstName CreateByFullName
    	                            ,a.UserName  CreateByUserName
                    ,a.[ProfileImage] CreateByUserAvatar
                    , CONVERT(VARCHAR(12),t.[DateCreated], 113) DateCreated
    	                            , convert(varchar(10),t.[DateCreated], 111)  DateCreated2
    	                            ,DATEDIFF(dd,GetDate(),t.[DateCreated]) NoOfDays
                    , t.[CategoryId]
    	                            ,tc.Title  TopicCategory
                    ,(select count(*) from Posts where TopicId=t.Id) NoOfPost
                    ,isnull(t.[ViewCount],0) TopicView
                    ,(select count(*) from [FollowTopics] where TopicId=@TopicId and (UserId=@UserId or @UserId=0) and IsDeleted=0 and  IsActive=1 ) IsFollowing
                    FROM [Topics] t
                    inner join [TopicCategories] tc on  t.[CategoryId]=tc.Id
                    inner join [AspNetUsers]  a on t.[CreatedBy]=a.AspNetUserId
                    where   t.[IsDeleted]=0 and  t.[IsActive]=1 and t.Id=@TopicId");


            CreateStoredProcedure("dbo.SpGetTopicContentByTopicId",
               p => new
               {
                   TopicId = p.Int(),
               },
         body:
         @" SELECT p.Id,p.Content
              FROM [Posts] p
              where p.TopicId=@TopicId and IsTopicStarter=1 and p.[IsDeleted]=0 and p.[IsActive]=1");



            CreateStoredProcedure("dbo.SpGetAllTopicByCategoryId",
               p => new
               {
                   CategoryId = p.Int()
               },
         body:
         @"SELECT  t.[Id]
                    , t.[Title]
                    , t.[Description]
                    , t.[TopicImage]
                    , isnull(t.[CreatedBy],0) CreatedBy
    	                                  ,a.LastName + ' ' + a.FirstName CreateByFullName
    	                                  ,a.UserName  CreateByUserName
                    ,a.[ProfileImage] CreateByUserAvatar
                    , CONVERT(VARCHAR(12),t.[DateCreated], 113) DateCreated
    	                                  , convert(varchar(10),t.[DateCreated], 111)  DateCreated2
    	                                  ,DATEDIFF(dd,GetDate(),t.[DateCreated]) NoOfDays
                    , t.[CategoryId]
    	                                  ,tc.Title  TopicCategory
                    ,(select count(*) from Posts where TopicId=t.Id) NoOfPost
                    ,isnull(t.[ViewCount],0) TopicView
                    FROM [Topics] t
                    inner join [TopicCategories] tc on  t.[CategoryId]=tc.Id
                    inner join [AspNetUsers]  a on t.[CreatedBy]=a.AspNetUserId
                    where   t.[IsDeleted]=0 and  t.[IsActive]=1 and t.[CategoryId]=@CategoryId");
        }
        
        public override void Down()
        {
            
            DropStoredProcedure("dbo.SpGetLastPostUserByTopicId");
            DropStoredProcedure("dbo.SpGetTopicByTopicId");
            DropStoredProcedure("dbo.SpGetAllPostByTopicId");
            DropStoredProcedure("dbo.SpGetAllPostByTopicIdByUserId");
            DropStoredProcedure("dbo.SpGetAllTopic");
            DropStoredProcedure("dbo.SpGetAllTopicByUserId");

            DropStoredProcedure("dbo.SpGetAllTopicCeatedToday");
            DropStoredProcedure("dbo.SpGetAllMostViewedTopic");
            DropStoredProcedure("dbo.SpGetTopicByTopicIdByUserId");
            DropStoredProcedure("dbo.SpGetTopicContentByTopicId");
            DropStoredProcedure("dbo.SpGetAllTopicByCategoryId");

        }
    }
}
