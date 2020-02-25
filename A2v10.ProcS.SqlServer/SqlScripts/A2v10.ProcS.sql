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
		IsComplete bit not null constraint DF_Instances_IsCompelte default(0),
		WorkflowState nvarchar(max) null,
		InstanceState nvarchar(max) null,
		DateCreated datetime2 not null constraint DF_Instances_DateCreated default(getutcdate())
	);
end
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.SEQUENCES where SEQUENCE_SCHEMA=N'A2v10.ProcS' and SEQUENCE_NAME=N'SQ_MessageQueue')
	create sequence [A2v10.ProcS].SQ_MessageQueue as bigint start with 1000 increment by 1;
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'A2v10.ProcS' and TABLE_NAME=N'MessageQueue')
begin
	create table [A2v10.ProcS].MessageQueue
	(
		Id	bigint	not null constraint PK_MessageQueue primary key
			constraint DF_MessageQueue_PK default(next value for [A2v10.ProcS].SQ_MessageQueue),
		[Message] nvarchar(max),
		[State] nvarchar(32) not null
			constraint DF_MessageQueue_State default(N'Init'),
		DateCreated datetime2 not null constraint DF_MessageQueue_DateCreated default(getutcdate())
	)
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
		insert(Id, Parent, Workflow, [Version], IsComplete, WorkflowState, InstanceState)
		values (@Id, @Parent, @Workflow, @Version, @IsComplete, @WorkflowState, @InstanceState);
end
go
------------------------------------------------
create or alter procedure [A2v10.ProcS].[Instance.Load]
@Id uniqueidentifier
as
begin
	set nocount on;
	set transaction isolation level read committed;

	select Id, Parent, Workflow, [Version], IsComplete, WorkflowState, InstanceState
	from [A2v10.ProcS].Instances where Id=@Id;
end
go
------------------------------------------------
create or alter procedure [A2v10.ProcS].[Message.Send]
@Message nvarchar(max)
as
begin
	set nocount on;
	set transaction isolation level serializable;
	set xact_abort on;
	insert into [A2v10.ProcS].MessageQueue ([Message]) values (@Message)
end
go

------------------------------------------------
create or alter procedure [A2v10.ProcS].[Message.Peek]
@Message nvarchar(max)
as
begin
	set nocount on;
	set transaction isolation level serializable;
	set xact_abort on;

	declare @tmp table([Message] nvarchar(max), Id bigint);

	update [A2v10.ProcS].MessageQueue set [State] = N'Hold'
	output inserted.[Message], inserted.Id into @tmp([Message], Id)
	from [A2v10.ProcS].MessageQueue where [State] = N'Init';

	select [Message] from @tmp order by Id;
end
go

select * from [A2v10.ProcS].[MessageQueue] order by Id desc;
select * from [A2v10.ProcS].Instances order by DateCreated desc

