/* Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved. 

LastModified: 31 Mar 2020

*/
------------------------------------------------
create or alter procedure A2v10_ProcS.[Test.Simple]
@Instance uniqueidentifier,
@X int,
@Y int
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;
	set xact_abort on;
	if @Instance is null
		throw 6000, N'Instance is null', 0
	if @X <> 5
		throw 6000, N'Invalid X. Expected 5', 0
	if @Y <> 10
		throw 6000, N'Invalid Y. Expected 10', 0

end
go
