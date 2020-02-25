/* Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved. */

------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.SCHEMATA where SCHEMA_NAME=N'A2v10.ProcS')
begin
	exec sp_executesql N'create schema [A2v10.ProcS]';
end
go
------------------------------------------------
begin
	set nocount on;
	grant execute on schema ::[A2v10.ProcS] to public;
end
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'A2v10.ProcS' and TABLE_NAME=N'Instances')
begin
	create table [A2v10.ProcS].Instances
	(
		Id	uniqueidentifier not null constraint PK_Instances primary key,
		Parent uniqueidentifier null
			constraint FK_Instances_Parent_Instances references [A2v10.ProcS].Instances(Id),
		Workflow nvarchar(255) not null,
		[Version] int not null,
		IsComplete bit not null constraint DF_Instacnes_IsCompelte default(0),
		WorkflowState nvarchar(max) null,
		InstanceState nvarchar(max) null,
		DateCreated datetime not null constraint DF_Instacnes_DateCreated default(getutcdate())
	);
end
go

------------------------------------------------
create or alter procedure [A2v10.ProcS].[Instance.Save]
@Id uniqueidentifier,
@Parent uniqueidentifier = null,
@Workflow nvarchar(255),
@Version int,
@IsComplete bit,
@WorkflowState nvarchar(max),
@InstanceState nvarchar(max)
as
begin
	set nocount on;
	set transaction isolation level serializable;
	set xact_abort on;
	merge [A2v10.ProcS].Instances as target
	using (select Id=@Id) as source
	on target.Id = source.Id
	when matched then update set
		target.IsComplete = @IsComplete,
		target.WorkflowState = @WorkflowState,
		target.InstanceState = @InstanceState
	when not matched by target then 
		insert(Id, Parent, Workflow, [Version])
		values (@Id, @Parent, @Workflow, @Version)
end
go

------------------------------------------------
create or alter procedure [A2v10.ProcS].[Instance.Load]
@Id uniqueidentifier
as
begin
	set nocount on;
	set transaction isolation level serializable;
	set xact_abort on;
end
go

