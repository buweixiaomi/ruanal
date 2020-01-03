CREATE TABLE taskWorkLog{parttablename}
(
	[logId] [int] primary key IDENTITY(1,1) NOT NULL,
	[taskId] [int] NOT NULL,
	[nodeId] [int] NOT NULL,
	[dispatchId] [varchar](100) NULL,
	[logType] [int] NOT NULL default(0),
	[logText] [varchar](max) NULL,
	[serverTime] [datetime] NOT NULL,
	[createTime] [datetime] NOT NULL default(getdate())
);
	
create nonclustered index IX_taskWorkLog{parttablename}_ct on taskWorkLog{parttablename}(createtime desc);
create nonclustered index IX_taskWorkLog{parttablename}_dpid on taskWorkLog{parttablename}(dispatchid);
create nonclustered index IX_taskWorkLog{parttablename}_logtype on taskWorkLog{parttablename}(logtype);
create nonclustered index IX_taskWorkLog{parttablename}_nodeid on taskWorkLog{parttablename}(nodeid);
create nonclustered index IX_taskWorkLog{parttablename}_st on taskWorkLog{parttablename}(servertime desc);
create nonclustered index IX_taskWorkLog{parttablename}_taskid on taskWorkLog{parttablename}(taskid);