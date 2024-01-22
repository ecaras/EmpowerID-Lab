/***** Enable Change data capture at database level *****/ 

USE [empowerid] 
EXEC sys.sp_cdc_enable_db  
;



/***** Enable Change data capture at table level *****/ 

EXEC sys.sp_cdc_enable_table
    @source_schema = N'dbo',
    @source_name   = N'Categories',
    @role_name     = NULL,
    @supports_net_changes = 1
;


EXEC sys.sp_cdc_enable_table
    @source_schema = N'dbo',
    @source_name   = N'Products',
    @role_name     = NULL,
    @supports_net_changes = 1
;


EXEC sys.sp_cdc_enable_table
    @source_schema = N'dbo',
    @source_name   = N'Orders',
    @role_name     = NULL,
    @supports_net_changes = 1
;
