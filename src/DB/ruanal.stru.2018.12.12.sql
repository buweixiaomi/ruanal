-- BEGIN #########cmd########### 
create table cmd
(
	[cmdId]            int             not null    primary key   identity(1,1) ,
	[nodeId]           int             not null     ,
	[cmdType]          varchar(100)    not null     ,
	[cmdArgs]          varchar(max)    not null     default(''),
	[createTime]       datetime        not null     default(getdate()),
	[cmdState]         int             not null     default(0),
	[callTime]         datetime        null         ,
	[endTime]          datetime        null         ,
	[resultText]       varchar(max)    null         
)
-- primary key

-- foreign keys

-- unique keys

-- other indexes(except pk fk uq)
create nonclustered index IX_cmd_nodeid on cmd(nodeId asc) ;
create nonclustered index IX_cmd_createTime on cmd(createTime desc) ;
create nonclustered index IX_CLOUDDBA_nodeId_cmdState_ on cmd(nodeId asc,cmdState asc) ;
-- desc
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'cmd', @level2type=N'COLUMN',@level2name=N'cmdId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'cmd', @level2type=N'COLUMN',@level2name=N'nodeId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'cmd', @level2type=N'COLUMN',@level2name=N'cmdType'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'cmd', @level2type=N'COLUMN',@level2name=N'cmdArgs'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'cmd', @level2type=N'COLUMN',@level2name=N'createTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'cmd', @level2type=N'COLUMN',@level2name=N'cmdState'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'cmd', @level2type=N'COLUMN',@level2name=N'callTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'cmd', @level2type=N'COLUMN',@level2name=N'endTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'cmd', @level2type=N'COLUMN',@level2name=N'resultText'

-- END #########cmd########### 


-- BEGIN #########dispatch########### 
create table dispatch
(
	[dispatchId]       int             not null    primary key   identity(1,1) ,
	[groupId]          varchar(100)    not null     ,
	[invokeId]         varchar(100)    not null     ,
	[taskId]           int             not null     ,
	[dispatchState]    int             not null     default(0),
	[runArgs]          varchar(max)    not null     default(''),
	[createTime]       datetime        not null     default(getdate()),
	[expireTime]       datetime        not null     ,
	[dispatchTime]     datetime        null         ,
	[executeTime]      datetime        null         ,
	[endTime]          datetime        null         ,
	[resultText]       varchar(max)    null         ,
	[nodeId]           int             not null     default(0),
	[runKey]           varchar(100)    null         ,
	[nickName]         varchar(255)    null         
)
-- primary key

-- foreign keys

-- unique keys

-- other indexes(except pk fk uq)
create nonclustered index IX_disp_groupid on dispatch(groupId asc) ;
create nonclustered index IX_disp_invokeId on dispatch(invokeId asc) ;
create nonclustered index IX_disp_taskid on dispatch(taskId asc) ;
create nonclustered index IX_disp_nodeid on dispatch(nodeId asc) ;
create nonclustered index IX_disp_runkey on dispatch(runKey asc) ;
create nonclustered index IX_disp_createtime on dispatch(createTime desc) ;
create nonclustered index IX_disp_expiretime on dispatch(expireTime desc) ;
create nonclustered index IX_dispatch_state on dispatch(dispatchState asc) ;
create nonclustered index IX_disp_exectime on dispatch(executeTime desc) ;
create nonclustered index IX_dispatch_taskid_dstate_etime on dispatch(taskId asc,dispatchState asc,expireTime desc)  include (dispatchId) ;
create nonclustered index IX_CLOUDDBA_dispatchState_groupId_invokeId_runKey_nickName on dispatch(dispatchState asc)  include (groupId,invokeId,runKey,nickName) ;
create nonclustered index IX_dispatch_id_state on dispatch(dispatchId asc,dispatchState asc) ;
create nonclustered index IX_dispatch_nodeid_taskid_state on dispatch(nodeId asc,taskId asc,dispatchState asc) ;
create nonclustered index IX_dispatch_taskid_state_expiretime on dispatch(taskId asc,dispatchState asc,expireTime desc) ;
-- desc
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dispatch', @level2type=N'COLUMN',@level2name=N'dispatchId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dispatch', @level2type=N'COLUMN',@level2name=N'groupId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dispatch', @level2type=N'COLUMN',@level2name=N'invokeId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dispatch', @level2type=N'COLUMN',@level2name=N'taskId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dispatch', @level2type=N'COLUMN',@level2name=N'dispatchState'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dispatch', @level2type=N'COLUMN',@level2name=N'runArgs'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dispatch', @level2type=N'COLUMN',@level2name=N'createTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dispatch', @level2type=N'COLUMN',@level2name=N'expireTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dispatch', @level2type=N'COLUMN',@level2name=N'dispatchTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dispatch', @level2type=N'COLUMN',@level2name=N'executeTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dispatch', @level2type=N'COLUMN',@level2name=N'endTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dispatch', @level2type=N'COLUMN',@level2name=N'resultText'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dispatch', @level2type=N'COLUMN',@level2name=N'nodeId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dispatch', @level2type=N'COLUMN',@level2name=N'runKey'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dispatch', @level2type=N'COLUMN',@level2name=N'nickName'

-- END #########dispatch########### 


-- BEGIN #########dispatchKeyState########### 
create table dispatchKeyState
(
	[disKey]           varchar(256)    not null     ,
	[keyState]         int             not null     ,
	[dispatchId]       int             not null     
)
-- primary key
alter table dispatchKeyState add constraint PK_dispatchKeyState primary key nonclustered (disKey);
-- foreign keys

-- unique keys

-- other indexes(except pk fk uq)

-- desc
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dispatchKeyState', @level2type=N'COLUMN',@level2name=N'disKey'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dispatchKeyState', @level2type=N'COLUMN',@level2name=N'keyState'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dispatchKeyState', @level2type=N'COLUMN',@level2name=N'dispatchId'

-- END #########dispatchKeyState########### 


-- BEGIN #########manager########### 
create table manager
(
	[managerId]        int             not null    primary key   identity(1,1) ,
	[name]             varchar(60)     not null     ,
	[subName]          varchar(100)    not null     default(''),
	[loginName]        varchar(100)    not null     default(''),
	[loginPwd]         varchar(100)    null         default(NULL),
	[allowLogin]       int             not null     default('0'),
	[state]            int             not null     default('0'),
	[createTime]       datetime        not null     ,
	[lastLoginTime]    datetime        null         default(NULL),
	[updateTime]       datetime        null         default(NULL),
	[remark]           varchar(400)    not null     default('')
)
-- primary key

-- foreign keys

-- unique keys

-- other indexes(except pk fk uq)

-- desc
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'manager', @level2type=N'COLUMN',@level2name=N'managerId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'manager', @level2type=N'COLUMN',@level2name=N'name'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'manager', @level2type=N'COLUMN',@level2name=N'subName'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'manager', @level2type=N'COLUMN',@level2name=N'loginName'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'manager', @level2type=N'COLUMN',@level2name=N'loginPwd'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'manager', @level2type=N'COLUMN',@level2name=N'allowLogin'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'manager', @level2type=N'COLUMN',@level2name=N'state'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'manager', @level2type=N'COLUMN',@level2name=N'createTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'manager', @level2type=N'COLUMN',@level2name=N'lastLoginTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'manager', @level2type=N'COLUMN',@level2name=N'updateTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'manager', @level2type=N'COLUMN',@level2name=N'remark'

-- END #########manager########### 


-- BEGIN #########node########### 
create table node
(
	[nodeId]           int             not null    primary key   identity(1,1) ,
	[title]            varchar(100)    not null     ,
	[clientId]         varchar(100)    not null     ,
	[nodeConfig]       varchar(max)    not null     ,
	[nodeType]         int             not null     default(0),
	[lastHeartTime]    datetime        null         ,
	[macs]             varchar(500)    not null     ,
	[ips]              varchar(500)    not null     ,
	[state]            int             not null     default(0),
	[remark]           varchar(500)    not null     default(''),
	[createTime]       datetime        not null     default(getdate()),
	[stopDispatch]     int             not null     default(0)
)
-- primary key

-- foreign keys

-- unique keys

-- other indexes(except pk fk uq)

-- desc
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'node', @level2type=N'COLUMN',@level2name=N'nodeId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'node', @level2type=N'COLUMN',@level2name=N'title'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'node', @level2type=N'COLUMN',@level2name=N'clientId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'node', @level2type=N'COLUMN',@level2name=N'nodeConfig'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'node', @level2type=N'COLUMN',@level2name=N'nodeType'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'node', @level2type=N'COLUMN',@level2name=N'lastHeartTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'node', @level2type=N'COLUMN',@level2name=N'macs'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'node', @level2type=N'COLUMN',@level2name=N'ips'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'node', @level2type=N'COLUMN',@level2name=N'state'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'node', @level2type=N'COLUMN',@level2name=N'remark'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'node', @level2type=N'COLUMN',@level2name=N'createTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'node', @level2type=N'COLUMN',@level2name=N'stopDispatch'

-- END #########node########### 


-- BEGIN #########operationlog########### 
create table operationlog
(
	[Id]               int             not null    primary key   identity(1,1) ,
	[OperationContent] text            null         ,
	[OperationName]    varchar(100)    null         default(NULL),
	[Createtime]       datetime        null         default(NULL),
	[OperationTitle]   varchar(200)    null         default(NULL),
	[Module]           varchar(200)    null         default(NULL)
)
-- primary key

-- foreign keys

-- unique keys

-- other indexes(except pk fk uq)

-- desc
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'operationlog', @level2type=N'COLUMN',@level2name=N'Id'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'operationlog', @level2type=N'COLUMN',@level2name=N'OperationContent'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'operationlog', @level2type=N'COLUMN',@level2name=N'OperationName'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'operationlog', @level2type=N'COLUMN',@level2name=N'Createtime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'operationlog', @level2type=N'COLUMN',@level2name=N'OperationTitle'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'operationlog', @level2type=N'COLUMN',@level2name=N'Module'

-- END #########operationlog########### 


-- BEGIN #########RuanalCfg########### 
create table RuanalCfg
(
	[Key]              varchar(256)    not null     ,
	[Value]            varchar(max)    null         
)
-- primary key

-- foreign keys

-- unique keys

-- other indexes(except pk fk uq)

-- desc
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RuanalCfg', @level2type=N'COLUMN',@level2name=N'Key'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RuanalCfg', @level2type=N'COLUMN',@level2name=N'Value'

-- END #########RuanalCfg########### 


-- BEGIN #########task########### 
create table task
(
	[taskId]           int             not null    primary key   identity(1,1) ,
	[title]            varchar(200)    not null     default(''),
	[taskType]         int             not null     default(0),
	[taskTags]         int             not null     default(0),
	[runCron]          varchar(200)    not null     default(''),
	[taskConfig]       varchar(max)    not null     default(''),
	[state]            int             not null     default(0),
	[currVersionId]    int             not null     default(0),
	[enterDll]         varchar(200)    not null     default(''),
	[enterClass]       varchar(200)    not null     default(''),
	[dispatchClass]    varchar(200)    not null     default(''),
	[expireMins]       decimal(18,2)   not null     ,
	[createTime]       datetime        not null     default(getdate()),
	[updateTime]       datetime        null         ,
	[remark]           nvarchar(500)   not null     default('')
)
-- primary key

-- foreign keys

-- unique keys

-- other indexes(except pk fk uq)

-- desc
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'task', @level2type=N'COLUMN',@level2name=N'taskId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'task', @level2type=N'COLUMN',@level2name=N'title'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'task', @level2type=N'COLUMN',@level2name=N'taskType'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'task', @level2type=N'COLUMN',@level2name=N'taskTags'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'task', @level2type=N'COLUMN',@level2name=N'runCron'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'task', @level2type=N'COLUMN',@level2name=N'taskConfig'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'task', @level2type=N'COLUMN',@level2name=N'state'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'task', @level2type=N'COLUMN',@level2name=N'currVersionId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'task', @level2type=N'COLUMN',@level2name=N'enterDll'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'task', @level2type=N'COLUMN',@level2name=N'enterClass'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'task', @level2type=N'COLUMN',@level2name=N'dispatchClass'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'task', @level2type=N'COLUMN',@level2name=N'expireMins'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'task', @level2type=N'COLUMN',@level2name=N'createTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'task', @level2type=N'COLUMN',@level2name=N'updateTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'task', @level2type=N'COLUMN',@level2name=N'remark'

-- END #########task########### 


-- BEGIN #########taskBinding########### 
create table taskBinding
(
	[bindId]           int             not null    primary key   identity(1,1) ,
	[taskId]           int             not null     ,
	[nodeId]           int             not null     ,
	[runVersion]       int             not null     default(0),
	[localState]       int             not null     default(0),
	[serverState]      int             not null     default(0),
	[lastRunTime]      datetime        null         ,
	[memory]           decimal(18,2)   null         ,
	[stopDispatch]     int             not null     default(0)
)
-- primary key

-- foreign keys

-- unique keys

-- other indexes(except pk fk uq)
create nonclustered index IX_taskbinding_taskid_nodeid on taskBinding(taskId asc,nodeId asc) ;
-- desc
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskBinding', @level2type=N'COLUMN',@level2name=N'bindId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskBinding', @level2type=N'COLUMN',@level2name=N'taskId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskBinding', @level2type=N'COLUMN',@level2name=N'nodeId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskBinding', @level2type=N'COLUMN',@level2name=N'runVersion'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskBinding', @level2type=N'COLUMN',@level2name=N'localState'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskBinding', @level2type=N'COLUMN',@level2name=N'serverState'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskBinding', @level2type=N'COLUMN',@level2name=N'lastRunTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskBinding', @level2type=N'COLUMN',@level2name=N'memory'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskBinding', @level2type=N'COLUMN',@level2name=N'stopDispatch'

-- END #########taskBinding########### 


-- BEGIN #########taskRunLog########### 
create table taskRunLog
(
	[logId]            int             not null    primary key   identity(1,1) ,
	[runGuid]          varchar(100)    not null     ,
	[taskId]           int             not null     ,
	[nodeId]           int             not null     ,
	[runType]          int             not null     ,
	[runServerTime]    datetime        not null     ,
	[runDbTime]        datetime        not null     default(getdate()),
	[endServerTime]    datetime        null         ,
	[endDbTime]        datetime        null         ,
	[resultType]       int             not null     default(0),
	[logText]          varchar(max)    null         
)
-- primary key

-- foreign keys

-- unique keys

-- other indexes(except pk fk uq)
create nonclustered index IX_runlog_runguid on taskRunLog(runGuid asc) ;
create nonclustered index IX_runlog_taskid on taskRunLog(taskId asc) ;
create nonclustered index IX_runlog_nodeid on taskRunLog(nodeId asc) ;
create nonclustered index IX_runlog_resulttype on taskRunLog(resultType asc) ;
create nonclustered index IX_runlog_runservertime on taskRunLog(runServerTime desc) ;
create nonclustered index IX_runlog_rundbtime on taskRunLog(runDbTime desc) ;
create nonclustered index IX_taskrunlog_taskid_nodeid_runguid on taskRunLog(taskId asc,nodeId asc,runGuid asc) ;
-- desc
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskRunLog', @level2type=N'COLUMN',@level2name=N'logId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskRunLog', @level2type=N'COLUMN',@level2name=N'runGuid'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskRunLog', @level2type=N'COLUMN',@level2name=N'taskId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskRunLog', @level2type=N'COLUMN',@level2name=N'nodeId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskRunLog', @level2type=N'COLUMN',@level2name=N'runType'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskRunLog', @level2type=N'COLUMN',@level2name=N'runServerTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskRunLog', @level2type=N'COLUMN',@level2name=N'runDbTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskRunLog', @level2type=N'COLUMN',@level2name=N'endServerTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskRunLog', @level2type=N'COLUMN',@level2name=N'endDbTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskRunLog', @level2type=N'COLUMN',@level2name=N'resultType'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskRunLog', @level2type=N'COLUMN',@level2name=N'logText'

-- END #########taskRunLog########### 


-- BEGIN #########taskTag########### 
create table taskTag
(
	[tagIndex]         int             not null     ,
	[tagName]          varchar(200)    not null     
)
-- primary key

-- foreign keys

-- unique keys

-- other indexes(except pk fk uq)

-- desc
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskTag', @level2type=N'COLUMN',@level2name=N'tagIndex'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskTag', @level2type=N'COLUMN',@level2name=N'tagName'

-- END #########taskTag########### 


-- BEGIN #########taskVersion########### 
create table taskVersion
(
	[versionId]        int             not null    primary key   identity(1,1) ,
	[taskId]           int             not null     ,
	[versionNO]        varchar(60)     not null     default(''),
	[filePath]         varchar(200)    not null     ,
	[fileSize]         decimal(18,2)   not null     ,
	[vstate]           int             not null     default(0),
	[createTime]       datetime        not null     default(getdate()),
	[remark]           varchar(max)    not null     default('')
)
-- primary key

-- foreign keys

-- unique keys

-- other indexes(except pk fk uq)
create nonclustered index IX_taskversion_taskid on taskVersion(taskId asc) ;
-- desc
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskVersion', @level2type=N'COLUMN',@level2name=N'versionId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskVersion', @level2type=N'COLUMN',@level2name=N'taskId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskVersion', @level2type=N'COLUMN',@level2name=N'versionNO'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskVersion', @level2type=N'COLUMN',@level2name=N'filePath'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskVersion', @level2type=N'COLUMN',@level2name=N'fileSize'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskVersion', @level2type=N'COLUMN',@level2name=N'vstate'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskVersion', @level2type=N'COLUMN',@level2name=N'createTime'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'taskVersion', @level2type=N'COLUMN',@level2name=N'remark'

-- END #########taskVersion########### 


