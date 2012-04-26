USE [master]
GO

----------------------------------------------------------------
-- Create the storage folders. 
-- This extended stored procedure does not fail if the path already exists; instead it does nothing 
----------------------------------------------------------------

EXEC sys.xp_create_subdir '<OddNbrFileDrive>\mssql\data\'
EXEC sys.xp_create_subdir '<OddNbrFileDrive>\mssql\log\'

EXEC sys.xp_create_subdir '<EvenNbrFileDrive>\mssql\data\'
EXEC sys.xp_create_subdir '<EvenNbrFileDrive>\mssql\log\'

----------------------------------------------------------------

CREATE DATABASE [<DatabaseName>] ON  PRIMARY 
( NAME = N'<DatabaseName>PriData', FILENAME = N'<OddNbrFileDrive>\MSSQL\DATA\<DatabaseName>PriData.mdf' , SIZE = 4096KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB ), 
 FILEGROUP [DATA]  DEFAULT 
( NAME = N'<DatabaseName>Data01', FILENAME = N'<OddNbrFileDrive>\MSSQL\DATA\<DatabaseName>Data01.ndf' , SIZE = 1024KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB ), 
( NAME = N'<DatabaseName>Data02', FILENAME = N'<EvenNbrFileDrive>\MSSQL\DATA\<DatabaseName>Data02.ndf' , SIZE = 1024KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB ), 
( NAME = N'<DatabaseName>Data03', FILENAME = N'<OddNbrFileDrive>\MSSQL\DATA\<DatabaseName>Data03.ndf' , SIZE = 1024KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB ), 
( NAME = N'<DatabaseName>Data04', FILENAME = N'<EvenNbrFileDrive>\MSSQL\DATA\<DatabaseName>Data04.ndf' , SIZE = 1024KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'<DatabaseName>LogData01', FILENAME = N'<OddNbrFileDrive>\MSSQL\LOG\<DatabaseName>Log01.ldf' , SIZE = 1024KB , MAXSIZE = 100MB , FILEGROWTH = 1024KB ),
( NAME = N'<DatabaseName>LogData02', FILENAME = N'<EvenNbrFileDrive>\MSSQL\LOG\<DatabaseName>Log02.ldf' , SIZE = 1024KB , MAXSIZE = 100MB , FILEGROWTH = 1024KB )
GO

ALTER DATABASE [<DatabaseName>] SET RECOVERY SIMPLE WITH NO_WAIT
GO

ALTER DATABASE [<DatabaseName>] SET COMPATIBILITY_LEVEL = 100
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [<DatabaseName>].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [<DatabaseName>] SET ANSI_NULL_DEFAULT ON 
GO

ALTER DATABASE [<DatabaseName>] SET ANSI_NULLS ON 
GO

ALTER DATABASE [<DatabaseName>] SET ANSI_PADDING ON 
GO

ALTER DATABASE [<DatabaseName>] SET ANSI_WARNINGS ON 
GO

ALTER DATABASE [<DatabaseName>] SET ARITHABORT ON 
GO

ALTER DATABASE [<DatabaseName>] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [<DatabaseName>] SET AUTO_CREATE_STATISTICS ON 
GO

ALTER DATABASE [<DatabaseName>] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [<DatabaseName>] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [<DatabaseName>] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [<DatabaseName>] SET CURSOR_DEFAULT  LOCAL 
GO

ALTER DATABASE [<DatabaseName>] SET CONCAT_NULL_YIELDS_NULL ON 
GO

ALTER DATABASE [<DatabaseName>] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [<DatabaseName>] SET QUOTED_IDENTIFIER ON 
GO

ALTER DATABASE [<DatabaseName>] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [<DatabaseName>] SET  DISABLE_BROKER 
GO

ALTER DATABASE [<DatabaseName>] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [<DatabaseName>] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [<DatabaseName>] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [<DatabaseName>] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [<DatabaseName>] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [<DatabaseName>] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [<DatabaseName>] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [<DatabaseName>] SET  READ_WRITE 
GO

ALTER DATABASE [<DatabaseName>] SET RECOVERY FULL 
GO

ALTER DATABASE [<DatabaseName>] SET  MULTI_USER 
GO

ALTER DATABASE [<DatabaseName>] SET PAGE_VERIFY NONE  
GO

ALTER DATABASE [<DatabaseName>] SET DB_CHAINING OFF 
GO

EXEC sp_dboption [<DatabaseName>], 'cursor close on commit', 'TRUE'
EXEC sp_dboption [<DatabaseName>], 'trunc. log on chkpt.', 'TRUE'
EXEC sp_dboption [<DatabaseName>], 'auto create statistics', 'TRUE'
EXEC sp_dboption [<DatabaseName>], 'auto update statistics', 'TRUE'
EXEC sp_dboption [<DatabaseName>], 'torn page detection', 'TRUE'
EXEC sp_dboption [<DatabaseName>], 'autoshrink', 'TRUE'

GO
sp_configure 'show advanced options', 1
GO
RECONFIGURE
GO
sp_configure 'fill factor (%)', 90
GO
RECONFIGURE
GO
sp_configure 'lightweight pooling', 1
GO
RECONFIGURE
GO
sp_configure 'priority boost', 1
GO
RECONFIGURE


