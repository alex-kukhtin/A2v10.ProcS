/* Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved. */



------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.SCHEMATA where SCHEMA_NAME=N'A2v10.ProcS')
begin
	exec sp_executesql N'create schema [A2v10.ProcS]';
end
go
------------------------------------------------
create or alter procedure [A2v10.ProcS].[SaveInstance]
as
begin
	set nocount on;
	set transaction isolation level serializable;
	set xact_abort on;
end
go
