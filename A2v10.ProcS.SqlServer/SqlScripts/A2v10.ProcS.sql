/* Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved. 

LastModified: 30 Mar 2020

*/
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.SCHEMATA where SCHEMA_NAME=N'A2v10_ProcS')
begin
	exec sp_executesql N'create schema A2v10_ProcS';
end
go
------------------------------------------------
begin
	set nocount on;
	grant execute on schema ::A2v10_ProcS to public;
end
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'A2v10_ProcS' and TABLE_NAME=N'SagaMap')
begin
	create table A2v10_ProcS.SagaMap
	(
		Host uniqueidentifier not null,
		[MessageKind] nvarchar(128) not null,
		[SagaKind] nvarchar(128) not null,
		constraint PK_SagaMap primary key(Host, [MessageKind], [SagaKind])
	);
end
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'A2v10_ProcS' and TABLE_NAME=N'Instances')
begin
	create table A2v10_ProcS.Instances
	(
		Id	uniqueidentifier not null constraint PK_Instances primary key,
		Parent uniqueidentifier null
			constraint FK_Instances_Parent_Instances references A2v10_ProcS.Instances(Id),
		Workflow nvarchar(255) not null,
		[Version] int not null,
		IsComplete bit not null constraint DF_Instances_IsCompelte default(0),
		CurrentState nvarchar(255) null,
		WorkflowState nvarchar(max) null,
		InstanceState nvarchar(max) null,
		DateCreated datetime2 not null constraint DF_Instances_DateCreated default(sysutcdatetime())
	);
end
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.COLUMNS where TABLE_SCHEMA=N'A2v10_ProcS' and TABLE_NAME=N'Instances' and COLUMN_NAME=N'CurrentState')
begin
	alter table A2v10_ProcS.Instances add CurrentState nvarchar(255) null;
end
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.SEQUENCES where SEQUENCE_SCHEMA=N'A2v10_ProcS' and SEQUENCE_NAME=N'SQ_MessageQueue')
	create sequence A2v10_ProcS.SQ_MessageQueue as bigint start with 1000 increment by 1;
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'A2v10_ProcS' and TABLE_NAME=N'MessageQueue')
begin
	create table A2v10_ProcS.MessageQueue
	(
		Id	bigint	not null constraint PK_MessageQueue primary key
			constraint DF_MessageQueue_PK default(next value for A2v10_ProcS.SQ_MessageQueue),
		CorrelationId nvarchar(255) null,
		Parent bigint null
			constraint FK_MessageQueue_Parent_MessageQueue references A2v10_ProcS.MessageQueue(Id),
		[Kind] nvarchar(255),
		[Body] nvarchar(max),
		[After] datetime2 null,
		[State] nvarchar(32) not null
			constraint DF_MessageQueue_State default(N'Init'),
		DateCreated datetime2 not null constraint DF_MessageQueue_DateCreated default(sysutcdatetime())
	)
end
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'A2v10_ProcS' and TABLE_NAME=N'Sagas')
begin
	create table A2v10_ProcS.Sagas
	(
		[Id] uniqueidentifier not null constraint PK_Sagas primary key,
		CorrelationId nvarchar(255) null,
		[Kind] nvarchar(255) null,
		[Body] nvarchar(max) null,
		[Hold] bit not null constraint DF_Sagas_Hold default(0),
		[Fault] bit not null constraint DF_Sagas_Fault default(0),
		MessageId bigint null,
		DateCreated datetime2 not null constraint DF_Sagas_DateCreated default(sysutcdatetime())
	);
end
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'A2v10_ProcS' and TABLE_NAME=N'Log')
begin
	create table A2v10_ProcS.[Log]
	(
		Id bigint not null identity(100, 1) constraint PK_Log primary key,
		EventTime datetime2 not null constraint DF_Log_EventTime default(sysutcdatetime()),
		Severity nchar(1) not null constraint CK_Log_Severity check (Severity in (N'I', N'W', N'E', N'C')),
		[SagaId] uniqueidentifier sparse null,
		[MessageId] bigint sparse null,
		[InstanceId] bigint sparse null,
		[CorrelationId] nvarchar(255) sparse null,
		[SagaKind] nvarchar(255) null,
		[Message] nvarchar(max) null,
		[StackTrace] nvarchar(max) null
	);
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'A2v10_ProcS' and ROUTINE_NAME=N'Instance.Save')
	drop procedure A2v10_ProcS.[Instance.Save]
go
------------------------------------------------
create procedure A2v10_ProcS.[Instance.Save]
@Id uniqueidentifier,
@Parent uniqueidentifier = null,
@Workflow nvarchar(255),
@Version int,
@IsComplete bit,
@CurrentState nvarchar(255),
@WorkflowState nvarchar(max),
@InstanceState nvarchar(max)
as
begin
	set nocount on;
	set transaction isolation level serializable;
	set xact_abort on;

	merge A2v10_ProcS.Instances as target
	using (select Id=@Id) as source
	on target.Id = source.Id
	when matched then update set
		target.IsComplete = @IsComplete,
		target.CurrentState = @CurrentState,
		target.WorkflowState = @WorkflowState,
		target.InstanceState = @InstanceState
	when not matched by target then 
		insert(Id, Parent, Workflow, [Version], IsComplete, CurrentState, WorkflowState, InstanceState)
		values (@Id, @Parent, @Workflow, @Version, @IsComplete, @CurrentState, @WorkflowState, @InstanceState);
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'A2v10_ProcS' and ROUTINE_NAME=N'Instance.Load')
	drop procedure A2v10_ProcS.[Instance.Load]
go
------------------------------------------------
create procedure A2v10_ProcS.[Instance.Load]
@Id uniqueidentifier
as
begin
	set nocount on;
	set transaction isolation level read committed;

	select Id, Parent, Workflow, [Version], IsComplete, WorkflowState, InstanceState
	from A2v10_ProcS.Instances where Id=@Id;
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'A2v10_ProcS' and ROUTINE_NAME=N'Message.Send')
	drop procedure A2v10_ProcS.[Message.Send]
go
------------------------------------------------
create procedure A2v10_ProcS.[Message.Send]
@Kind nvarchar(255) = null,
@CorrelationId nvarchar(255) = null,
@Body nvarchar(max),
@Parent bigint = null,
@After datetime2 = null,
@RetId bigint output
as
begin
	set nocount on;
	set transaction isolation level serializable;
	set xact_abort on;
	declare @rtable table(Id bigint);

	insert into A2v10_ProcS.MessageQueue ([Kind], CorrelationId, [Body], Parent, [After], [State]) 
	output inserted.Id into @rtable(id)
	values (@Kind, @CorrelationId, @Body, @Parent, @After,
		case when @Parent is not null then N'Wait' else N'Init' end);

	select top(1) @RetId = Id from @rtable;
end
go

------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'A2v10_ProcS' and ROUTINE_NAME=N'SagaMap.Save')
	drop procedure A2v10_ProcS.[SagaMap.Save]
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.DOMAINS where DATA_TYPE = N'table type'and DOMAIN_SCHEMA=N'A2v10_ProcS' and DOMAIN_NAME=N'SagaMap.TableType')
	drop type A2v10_ProcS.[SagaMap.TableType];
go
------------------------------------------------
create type A2v10_ProcS.[SagaMap.TableType]
as table(
	[MessageKind] nvarchar(255),
	SagaKind nvarchar(255)
)
go
------------------------------------------------
create procedure A2v10_ProcS.[SagaMap.Save]
@Host uniqueidentifier,
@Sagas A2v10_ProcS.[SagaMap.TableType] readonly
as
begin
	set nocount on;
	set transaction isolation level serializable;
	set xact_abort on;

	merge A2v10_ProcS.[SagaMap] as target
	using 
		(select Host = @Host, MessageKind, SagaKind from @Sagas) as source
	on target.Host = source.Host and target.MessageKind = source.MessageKind and target.SagaKind=source.SagaKind
	when not matched by target then 
		insert (Host, MessageKind, SagaKind)
		values (source.Host, source.MessageKind, source.SagaKind)
	when not matched by source then 
		delete;
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'A2v10_ProcS' and ROUTINE_NAME=N'Message.Peek')
	drop procedure A2v10_ProcS.[Message.Peek]
go
------------------------------------------------
create procedure A2v10_ProcS.[Message.Peek]
@Host uniqueidentifier
as
begin
	set nocount on;
	set transaction isolation level serializable;
	set xact_abort on;

	declare @queueTable table(QueueId bigint, MessageKind nvarchar(255), MessageBody nvarchar(max), 
		MessageCorrelationId nvarchar(255),
		SagaId uniqueidentifier, SagaKind nvarchar(255), SagaBody nvarchar(max), SagaHold bit, SagaCorrelationId nvarchar(255));

	declare @sagaTable table(Id uniqueidentifier);
	declare @sagaId uniqueidentifier;
	declare @queueId bigint;
	declare @currentTime datetime2;
	set @currentTime = sysutcdatetime();

	begin tran;
	with T
	as(
		select top(1) QueueId = q.Id, MessageKind = q.Kind, MessageBody = q.Body, MessageCorrelationId = q.CorrelationId,
			SagaId = s.Id,  SagaKind = m.SagaKind, SagaBody = s.Body, SagaHold = s.Hold, SagaCorrelationId = s.CorrelationId
		from A2v10_ProcS.MessageQueue q
			inner join A2v10_ProcS.SagaMap m on q.Kind = m.MessageKind
			left join A2v10_ProcS.Sagas s on s.[Kind] = m.SagaKind and q.CorrelationId = s.CorrelationId
		where m.Host = @Host and q.[State] = N'Init' and isnull(s.Hold, 0) = 0
		and (q.[After] is null or q.[After] < @currentTime)
		order by QueueId
	)
	update A2v10_ProcS.MessageQueue set [State] = N'Hold'
	output inserted.Id, inserted.Kind, inserted.Body, T.SagaId, T.SagaKind, T.SagaBody, T.MessageCorrelationId, T.SagaCorrelationId
	into @queueTable(QueueId, MessageKind, MessageBody, SagaId, SagaKind, SagaBody, MessageCorrelationId, SagaCorrelationId)
	from T inner join A2v10_ProcS.MessageQueue q on T.QueueId = q.Id;

	select top(1) @queueId = QueueId, @sagaId = SagaId from @queueTable;

	if exists(select * from @queueTable where SagaId is null and MessageCorrelationId <> N'null')
	begin
		-- create new saga
		insert into A2v10_ProcS.Sagas (Id, Kind, Hold, CorrelationId)
		output inserted.Id into @sagaTable(Id)
		select newid(), SagaKind, 1, MessageCorrelationId
			from @queueTable;
		select @sagaId = Id from @sagaTable;
		update @queueTable set SagaId = @sagaId, SagaHold = 1 where QueueId = @queueId;
	end
	else
	begin
		-- hold exisiting saga
		update A2v10_ProcS.Sagas set Hold = 1, MessageId = @queueId where Id = @sagaId;
	end
	commit tran;

	-- queue dependent messages
	update A2v10_ProcS.MessageQueue set [State] = N'Init' where Parent = @queueId and [State] = N'Wait';

	select QueueId, MessageKind, MessageBody, SagaId, SagaKind, SagaBody, SagaCorrelationId from @queueTable;
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'A2v10_ProcS' and ROUTINE_NAME=N'Saga.Update')
	drop procedure A2v10_ProcS.[Saga.Update]
go
------------------------------------------------
create procedure A2v10_ProcS.[Saga.Update]
@Id uniqueidentifier,
@CorrelationId nvarchar(255),
@Body nvarchar(max) = null,
@IsComplete bit
as
begin
	set nocount on;
	set transaction isolation level serializable;
	set xact_abort on;
	if not exists (select * from A2v10_ProcS.Sagas where Id = @Id)
	begin
		declare @msg nvarchar(255);
		set @msg = N'Saga with id {' + cast(@Id as nvarchar(255)) + N'} not found';
		throw 60000, @msg, 0
	end

	if @IsComplete = 1 or @CorrelationId is null
	begin
		begin tran;
		delete from A2v10_ProcS.MessageQueue 
			from A2v10_ProcS.MessageQueue q inner join A2v10_ProcS.Sagas s on q.Id = s.MessageId
			where s.Id = @Id;
		delete from A2v10_ProcS.Sagas where Id=@Id;
		commit tran;
	end
	else
	begin
		update A2v10_ProcS.Sagas set Hold = 0, CorrelationId = @CorrelationId, [Body] = @Body where [Id] = @Id;
	end
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'A2v10_ProcS' and ROUTINE_NAME=N'Saga.Fail')
	drop procedure A2v10_ProcS.[Saga.Fail]
go
------------------------------------------------
create procedure A2v10_ProcS.[Saga.Fail]
@Id uniqueidentifier,
@Exception nvarchar(255) = null,
@SagaKind nvarchar(255) = null,
@StackTrace nvarchar(max) = null,
@CorrelationId nvarchar(255) = null
as
begin
	set nocount on;
	set transaction isolation level serializable;
	set xact_abort on;
	begin tran;
	update A2v10_ProcS.[Sagas] set Fault = 1, Hold=1 where Id=@Id;
	-- write to log
	insert into A2v10_ProcS.[Log] (Severity, [Message], SagaKind, SagaId, StackTrace, CorrelationId)
		values (N'E', @Exception, @SagaKind, @Id, @StackTrace, @CorrelationId);
	commit tran;
end
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'A2v10_ProcS' and TABLE_NAME=N'Workflows')
begin
	create table A2v10_ProcS.Workflows
	(
		[Id] nvarchar(255) not null,
		[Version] int not null,
			constraint PK_Workflows primary key ([Id], [Version]),
		[Hash] uniqueidentifier null,
		[Body] nvarchar(max) null
	);
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'A2v10_ProcS' and ROUTINE_NAME=N'Workflows.Load')
	drop procedure A2v10_ProcS.[Workflows.Load]
go
------------------------------------------------
create procedure A2v10_ProcS.[Workflows.Load]
@Id nvarchar(255),
@Version int
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	select Id, [Version], [Hash], [Body]
	from A2v10_ProcS.Workflows
	where Id=@Id and [Version]=@Version;
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'A2v10_ProcS' and ROUTINE_NAME=N'Workflows.Update')
	drop procedure A2v10_ProcS.[Workflows.Update]
go
------------------------------------------------
create procedure A2v10_ProcS.[Workflows.Update]
@Id nvarchar(255),
@Body nvarchar(max),
@Hash uniqueidentifier
as
begin
	set nocount on;
	set transaction isolation level serializable;
	set xact_abort on;

	begin tran;

	declare @t table (
		n int not null primary key,
		h uniqueidentifier,
		v int
	)

	insert into @t (n, h, v) values (0, null, 0);

	insert into @t (n, h, v)
	select top (1) 1, [Hash], [Version]
	from A2v10_ProcS.Workflows
	where Id=@Id
	order by [Version] desc;
	
	insert into A2v10_ProcS.Workflows (Id, [Version], [Hash], [Body])
	output 2, INSERTED.[Hash], INSERTED.[Version] into @t (n, h, v)
	select @Id, v+1, @Hash, @Body
	from (
		select top (1) * from @t order by n desc
	) s
	where n=0 or h<>@Hash;

	select top (1) [Id]=@Id, [Version]=v, [Hash]=h, [Body]=@Body
	from @t
	order by n desc;

	commit tran;
end
go

/*
select * from A2v10_ProcS.[MessageQueue] order by Id desc;
select * from A2v10_ProcS.Instances order by DateCreated desc
select * from A2v10_ProcS.[Sagas] order by DateCreated desc;
select * from A2v10_ProcS.[Log] order by Id desc;
--select * from A2v10_ProcS.[SagaMap]
*/



/* 
	delete from A2v10_ProcS.[MessageQueue];
	delete from A2v10_ProcS.Instances;
	delete from A2v10_ProcS.[Sagas];
	delete from A2v10_ProcS.[SagaMap];
*/

/*
drop table A2v10_ProcS.[MessageQueue];
drop table A2v10_ProcS.Instances;
drop table A2v10_ProcS.[Sagas];
--drop table A2v10_ProcS.[SagaMap];

delete from A2v10_ProcS.Workflows;
*/
