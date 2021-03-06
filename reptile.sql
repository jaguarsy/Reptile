USE [test]
GO
/****** Object:  Table [dbo].[SHRoadIndex]    Script Date: 01/16/2015 09:13:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SHRoadIndex](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[State] [nvarchar](50) NULL,
	[Name] [nvarchar](50) NULL,
	[CurrentIndex] [decimal](18, 2) NULL,
	[ReferenceIndex] [decimal](18, 2) NULL,
	[DValue] [decimal](18, 2) NULL,
	[Time] [nvarchar](50) NULL,
	[Type] [int] NULL,
	[Date] [nvarchar](50) NULL,
 CONSTRAINT [PK_SHRoadIndex] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Packing]    Script Date: 01/16/2015 09:13:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Packing](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[PackingSpace] [int] NULL,
	[Time] [nvarchar](50) NULL,
	[date] [nvarchar](50) NULL,
 CONSTRAINT [PK_Packing] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[News]    Script Date: 01/16/2015 09:13:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[News](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RoadName] [nvarchar](50) NULL,
	[Detail] [nvarchar](250) NULL,
	[Time] [nvarchar](50) NULL,
	[RoadType] [int] NULL,
	[date] [nvarchar](50) NULL,
 CONSTRAINT [PK_News] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AMap]    Script Date: 01/16/2015 09:13:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AMap](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[DelayIndex] [decimal](18, 2) NULL,
	[Speed] [decimal](18, 2) NULL,
	[TravelTime] [decimal](18, 2) NULL,
	[DelayTime] [decimal](18, 2) NULL,
	[Date] [nvarchar](50) NULL,
	[Time] [nvarchar](50) NULL,
	[Type] [int] NULL,
 CONSTRAINT [PK_AMap] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
