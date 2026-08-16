USE [DATABASE_NAME]
GO

/****** Object:  Table [dbo].[tblSalesOrderImport_Staging]    Script Date: 8/1/2026 12:37:31 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tblSalesOrderImport_Staging](
	[OrderID] [nvarchar](250) NULL,
	[OrderDate] [nvarchar](250) NULL,
	[CustomerName] [nvarchar](250) NULL,
	[Product] [nvarchar](250) NULL,
	[Quantity] [nvarchar](250) NULL,
	[UnitPrice] [nvarchar](250) NULL,
	[Region] [nvarchar](250) NULL
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[tblSalesOrder]    Script Date: 8/1/2026 12:42:51 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tblSalesOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NOT NULL,
	[OrderDate] [date] NULL,
	[CustomerName] [varchar](100) NULL,
	[Product] [varchar](100) NULL,
	[Quantity] [int] NULL,
	[UnitPrice] [decimal](10, 2) NULL,
	[Region] [varchar](100) NULL,
	[TotalAmount]  AS ([UnitPrice]*[Quantity]) PERSISTED,
 CONSTRAINT [PK_SalesOrder_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[tblRegion]    Script Date: 8/1/2026 12:46:01 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tblRegion](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Description] [nchar](50) NULL,
 CONSTRAINT [pk_region_id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[_tblExceptions]    Script Date: 8/1/2026 12:48:42 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[_tblExceptions](
	[OrderID] [nvarchar](250) NULL,
	[OrderDate] [nvarchar](250) NULL,
	[CustomerName] [nvarchar](250) NULL,
	[Product] [nvarchar](250) NULL,
	[Quantity] [nvarchar](250) NULL,
	[UnitPrice] [nvarchar](250) NULL,
	[Region] [nvarchar](250) NULL
) ON [PRIMARY]
GO