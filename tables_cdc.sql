/******** Create Categories table for CDC, only primary key, no foreign keys and constraints  ********/

Use [empowerid_cdc]

CREATE TABLE [dbo].[Categories](
	[category_id] int NOT NULL,
	[category_name] [nvarchar](128) NULL,
 CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED 
(
	[category_id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO




/****** Create Products table for CDC, only primary key, no foreign keys and constraints  ********/

Use [empowerid_cdc]

CREATE TABLE [dbo].[Products](
	[product_id] [int] NOT NULL,
	[product_name] [nvarchar](256) NOT NULL,
	[category_id] [int] NOT NULL,
	[price] [decimal](12, 2) NOT NULL,
	[description] [nvarchar](max) NULL,
	[image_url] [nvarchar](512) NULL,
	[date_added] [datetime] NOT NULL,
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[product_id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO





/******** Create Orders table for CDC, only primary key, no foreign keys and constraints  ********/
Use [empowerid_cdc]

CREATE TABLE [dbo].[Orders](
	[order_id] [int] NOT NULL,
	[product_id] [int] NOT NULL,
	[order_date] [datetime] NOT NULL,
	[customer_name] [nvarchar](128) NULL,
	[quantity] [smallint] NOT NULL,
	[amount] [decimal](12, 2) NOT NULL,
 CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED 
(
	[order_id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO




