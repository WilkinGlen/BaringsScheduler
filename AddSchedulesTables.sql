USE [BaringsSchedules]
GO
/****** Object:  Table [dbo].[OneOffTriggerDefinitions]    Script Date: 07/03/2025 09:36:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OneOffTriggerDefinitions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ScheduleName] [nvarchar](450) NOT NULL,
	[JobName] [nvarchar](450) NOT NULL,
	[JobDescription] [nvarchar](450) NULL,
	[JobClassName] [nvarchar](450) NOT NULL,
	[JobGroupName] [nvarchar](450) NOT NULL,
	[JobCompleted] [datetime] NULL,
 CONSTRAINT [PK_OneOffTriggerDefinitions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TriggerDefinitions]    Script Date: 07/03/2025 09:36:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TriggerDefinitions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ScheduleName] [nvarchar](450) NOT NULL,
	[JobName] [nvarchar](450) NOT NULL,
	[JobDescription] [nvarchar](450) NULL,
	[JobClassName] [nvarchar](450) NOT NULL,
	[JobGroupName] [nvarchar](450) NOT NULL,
	[CronSchedule] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_TriggerDefinitions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
