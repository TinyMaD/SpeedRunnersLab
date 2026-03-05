# 评论系统 API

<cite>
**本文档引用的文件**
- [CommentController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/CommentController.cs)
- [CommentBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/CommentBLL.cs)
- [CommentDAL.cs](file://SpeedRunners.API/SpeedRunners.DAL/CommentDAL.cs)
- [MComment.cs](file://SpeedRunners.API/SpeedRunners.Model/Comment/MComment.cs)
- [MCommentParam.cs](file://SpeedRunners.API/SpeedRunners.Model/Comment/MCommentParam.cs)
- [MCommentOut.cs](file://SpeedRunners.API/SpeedRunners.Model/Comment/MCommentOut.cs)
- [MCommentLike.cs](file://SpeedRunners.API/SpeedRunners.Model/Comment/MCommentLike.cs)
- [BaseController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/BaseController.cs)
- [NotificationBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/NotificationBLL.cs)
- [MNotification.cs](file://SpeedRunners.API/SpeedRunners.Model/User/MNotification.cs)
- [NotificationController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/NotificationController.cs)
- [NotificationDAL.cs](file://SpeedRunners.API/SpeedRunners.DAL/NotificationDAL.cs)
- [RankBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/RankBLL.cs)
- [request.js](file://SpeedRunners.UI/src/utils/request.js)
</cite>

## 更新摘要
**所做更改**
- 完善了通知系统的架构说明，详细描述了通知类型、触发条件、去重机制
- 更新了通知存储查询和清理机制的实现细节
- 补充了通知系统与评论系统的集成关系
- 增强了通知系统的安全性和性能考虑

## 目录
1. [简介](#简介)
2. [项目结构](#项目结构)
3. [核心组件](#核心组件)
4. [架构概览](#架构概览)
5. [详细组件分析](#详细组件分析)
6. [API 接口定义](#api-接口定义)
7. [数据模型说明](#数据模型说明)
8. [通知机制详解](#通知机制详解)
9. [依赖关系分析](#依赖关系分析)
10. [性能考虑](#性能考虑)
11. [故障排除指南](#故障排除指南)
12. [总结](#总结)

## 简介

SpeedRunnersLab 评论系统是一个基于 ASP.NET Core 构建的完整评论功能模块，支持多级评论、点赞、回复通知等核心功能。该系统采用经典的三层架构设计（控制器-业务逻辑-数据访问），为游戏 SpeedRunners 提供了完整的社区互动平台。

**更新** 新增了完善的通知机制，支持评论回复和点赞的实时通知功能，包括去重机制和消息清理功能。通知系统作为评论系统的重要组成部分，实现了完整的消息生命周期管理。

## 项目结构

评论系统在整体项目中的位置和组织方式如下：

```mermaid
graph TB
subgraph "前端层 (SpeedRunners.UI)"
FE[Vue.js 前端应用]
Request[请求拦截器]
CommentSection[评论组件]
NotificationUI[通知界面]
end
subgraph "API 层 (SpeedRunners.API)"
CommentController[CommentController]
NotificationController[NotificationController]
BaseController[BaseController]
end
subgraph "业务逻辑层 (SpeedRunners.BLL)"
CommentBLL[CommentBLL]
NotificationBLL[NotificationBLL]
RankBLL[RankBLL]
end
subgraph "数据访问层 (SpeedRunners.DAL)"
CommentDAL[CommentDAL]
NotificationDAL[NotificationDAL]
DbHelper[DbHelper]
end
subgraph "数据模型层 (SpeedRunners.Model)"
MComment[MComment]
MCommentParam[MCommentParam]
MCommentOut[MCommentOut]
MCommentLike[MCommentLike]
MNotification[MNotification]
end
FE --> Request
Request --> CommentController
Request --> NotificationController
CommentController --> BaseController
NotificationController --> BaseController
BaseController --> CommentBLL
BaseController --> NotificationBLL
CommentBLL --> CommentDAL
CommentBLL --> NotificationBLL
CommentBLL --> RankBLL
NotificationBLL --> NotificationDAL
CommentDAL --> DbHelper
NotificationDAL --> DbHelper
CommentBLL --> MComment
CommentBLL --> MCommentParam
CommentBLL --> MCommentOut
CommentBLL --> MCommentLike
NotificationBLL --> MNotification
```

**图表来源**
- [CommentController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/CommentController.cs#L1-L33)
- [NotificationController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/NotificationController.cs#L1-L48)
- [CommentBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/CommentBLL.cs#L1-L181)
- [NotificationBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/NotificationBLL.cs#L1-L107)

**章节来源**
- [CommentController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/CommentController.cs#L1-L33)
- [BaseController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/BaseController.cs#L1-L25)

## 核心组件

### 控制器层
- **CommentController**: 处理所有评论相关的 HTTP 请求，提供 RESTful API 接口
- **NotificationController**: 处理通知相关的 HTTP 请求，提供通知查询和管理接口
- **BaseController**: 通用控制器基类，提供依赖注入和用户上下文管理

### 业务逻辑层
- **CommentBLL**: 核心业务逻辑处理，包含评论管理、点赞、通知等功能
- **NotificationBLL**: 通知业务逻辑处理，包含消息存储、查询、清理等功能
- **RankBLL**: 排行榜业务逻辑，为通知系统提供用户头像和昵称信息

### 数据访问层
- **CommentDAL**: 数据库操作封装，提供评论 CRUD 操作和查询功能
- **NotificationDAL**: 通知数据访问层，提供通知消息的存储和查询功能

### 数据模型层
- **MComment**: 评论实体模型
- **MCommentParam**: 评论参数模型（分页、添加评论）
- **MCommentOut**: 输出模型，包含评论详情和统计信息
- **MCommentLike**: 评论点赞关联模型
- **MNotification**: 通知消息实体模型

**章节来源**
- [CommentController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/CommentController.cs#L10-L31)
- [NotificationController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/NotificationController.cs#L10-L48)
- [CommentBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/CommentBLL.cs#L9-L19)
- [NotificationBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/NotificationBLL.cs#L9-L107)

## 架构概览

评论系统采用经典的三层架构模式，确保了关注点分离和代码的可维护性：

```mermaid
sequenceDiagram
participant Client as 客户端
participant CommentController as CommentController
participant NotificationController as NotificationController
participant CommentBLL as CommentBLL
participant NotificationBLL as NotificationBLL
participant CommentDAL as CommentDAL
participant NotificationDAL as NotificationDAL
participant DB as MySQL数据库
Client->>CommentController : POST /api/Comment/AddComment
CommentController->>CommentBLL : AddComment(param)
CommentBLL->>CommentDAL : AddComment(comment)
CommentDAL->>DB : INSERT 评论记录
DB-->>CommentDAL : 返回新评论ID
CommentDAL-->>CommentBLL : 新评论ID
CommentBLL->>CommentBLL : 发送回复通知
CommentBLL->>NotificationBLL : AddReplyNotification(...)
NotificationBLL->>NotificationDAL : Add(notification)
NotificationDAL->>DB : INSERT 通知记录
DB-->>NotificationDAL : 通知ID
NotificationDAL-->>NotificationBLL : 通知ID
NotificationBLL-->>CommentBLL : 操作完成
CommentBLL-->>CommentController : 操作完成
CommentController-->>Client : 成功响应
```

**图表来源**
- [CommentController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/CommentController.cs#L17-L20)
- [CommentBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/CommentBLL.cs#L45-L81)
- [NotificationBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/NotificationBLL.cs#L39-L65)

## 详细组件分析

### CommentController 分析

CommentController 是评论系统的主要入口点，提供了四个核心 API：

```mermaid
classDiagram
class CommentController {
+GetCommentList(param) MPageResult~MCommentOut~
+AddComment(param) void
+DeleteComment(commentID) void
+ToggleLike(commentID) int
}
class BaseController {
#TBLL BLL
#GetBLL() TBLL
}
class CommentBLL {
+GetCommentList(param) MPageResult~MCommentOut~
+AddComment(param) void
+DeleteComment(commentID) void
+ToggleLike(commentID) int
}
CommentController --|> BaseController
BaseController --> CommentBLL : 依赖注入
CommentController --> CommentBLL : 调用方法
```

**图表来源**
- [CommentController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/CommentController.cs#L10-L31)
- [BaseController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/BaseController.cs#L10-L23)

**章节来源**
- [CommentController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/CommentController.cs#L12-L30)

### CommentBLL 业务逻辑分析

CommentBLL 实现了评论系统的核心业务逻辑，包括：

#### 评论管理功能
- **GetCommentList**: 支持顶级评论和回复列表的获取
- **AddComment**: 评论创建和验证，包含通知发送逻辑
- **DeleteComment**: 评论删除（软删除）
- **ToggleLike**: 点赞/取消点赞功能，包含点赞通知逻辑

#### 通知机制
```mermaid
flowchart TD
Start([开始点赞]) --> CheckComment["检查评论是否存在"]
CheckComment --> CommentExists{"评论存在?"}
CommentExists --> |否| End([结束])
CommentExists --> |是| ToggleLike["执行点赞切换"]
ToggleLike --> CheckSender{"是否自己点赞?"}
CheckSender --> |是| UpdateCount["更新点赞计数"]
CheckSender --> |否| CheckLikeStatus["检查点赞状态"]
CheckLikeStatus --> IsLiked{"是否是点赞操作?"}
IsLiked --> |否| UpdateCount
IsLiked --> |是| SendLikeNotification["发送点赞通知"]
SendLikeNotification --> GetUserAvatar["获取用户头像信息"]
GetUserAvatar --> AddLikeNotification["调用通知BLL添加点赞通知"]
AddLikeNotification --> UpdateCount
UpdateCount --> End
```

**图表来源**
- [CommentBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/CommentBLL.cs#L136-L178)

**章节来源**
- [CommentBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/CommentBLL.cs#L23-L178)

### CommentDAL 数据访问分析

CommentDAL 提供了完整的数据库操作功能：

#### 数据库查询优化
- 使用 Dapper 的 QueryMultiple 实现单次连接的多结果集查询
- 支持分页查询和条件过滤
- 包含用户点赞状态的判断逻辑

#### 关键查询功能
- **GetCommentList**: 获取顶级评论列表，包含回复数量和点赞统计
- **GetReplyList**: 获取特定评论的回复列表
- **ToggleLike**: 点赞状态切换和计数更新

**章节来源**
- [CommentDAL.cs](file://SpeedRunners.API/SpeedRunners.DAL/CommentDAL.cs#L16-L147)

## API 接口定义

### 评论列表获取
- **URL**: `/api/Comment/GetCommentList`
- **方法**: POST
- **权限**: Persona
- **请求体**: `MCommentPageParam`
- **响应**: `MPageResult<MCommentOut>`

### 添加评论
- **URL**: `/api/Comment/AddComment`
- **方法**: POST
- **权限**: User
- **请求体**: `MAddComment`
- **响应**: void

### 删除评论
- **URL**: `/api/Comment/DeleteComment/{commentID}`
- **方法**: GET
- **权限**: User
- **参数**: commentID (路径参数)
- **响应**: void

### 切换点赞
- **URL**: `/api/Comment/ToggleLike/{commentID}`
- **方法**: GET
- **权限**: User
- **参数**: commentID (路径参数)
- **响应**: 点赞数量 (int)

### 通知相关接口

#### 获取通知列表
- **URL**: `/api/Notification/GetList`
- **方法**: POST
- **权限**: User
- **请求体**: `MNotificationQueryParam`
- **响应**: `MPageResult<MNotification>`

#### 获取未读通知数量
- **URL**: `/api/Notification/GetUnreadCount`
- **方法**: GET
- **权限**: User
- **响应**: `MUnreadCount`

#### 标记通知为已读
- **URL**: `/api/Notification/MarkAsRead`
- **方法**: POST
- **权限**: User
- **请求体**: `MMarkReadParam`
- **响应**: void

**章节来源**
- [CommentController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/CommentController.cs#L12-L30)
- [NotificationController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/NotificationController.cs#L15-L45)

## 数据模型说明

### MComment 实体模型
```mermaid
erDiagram
COMMENT {
int ID PK
string PagePath
string PlatformID
int ParentID
string ReplyToPlatformID
string Content
datetime CreateTime
int IsDeleted
}
COMMENTLIKE {
int ID PK
int CommentID FK
string PlatformID
datetime CreateTime
}
COMMENT ||--o{ COMMENT : "父子关系"
COMMENT ||--o{ COMMENTLIKE : "点赞关系"
```

**图表来源**
- [MComment.cs](file://SpeedRunners.API/SpeedRunners.Model/Comment/MComment.cs#L5-L15)
- [MCommentLike.cs](file://SpeedRunners.API/SpeedRunners.Model/Comment/MCommentLike.cs#L5-L12)

### MCommentParam 参数模型
- **MCommentPageParam**: 继承自 MPageParam，包含分页参数和页面路径
- **MAddComment**: 评论添加参数，包含内容和回复目标

### MCommentOut 输出模型
扩展了基础 MComment，增加了用户信息和统计字段：
- PersonaName: 用户昵称
- AvatarS: 用户头像
- ReplyToPersonaName: 回复对象昵称
- ReplyCount: 回复数量
- LikeCount: 点赞数量
- IsLiked: 当前用户是否已点赞

### MNotification 通知模型
- **NotificationType**: 通知类型枚举（Reply=回复我, Like=收到的点赞）
- **MNotification**: 通知实体，包含接收者、发送者、消息内容等信息
- **MNotificationQueryParam**: 通知查询参数，支持按类型和已读状态筛选
- **MUnreadCount**: 未读消息统计，包含回复和点赞的未读数量
- **MMarkReadParam**: 标记已读参数，支持批量标记

**章节来源**
- [MCommentParam.cs](file://SpeedRunners.API/SpeedRunners.Model/Comment/MCommentParam.cs#L3-L17)
- [MCommentOut.cs](file://SpeedRunners.API/SpeedRunners.Model/Comment/MCommentOut.cs#L3-L11)
- [MNotification.cs](file://SpeedRunners.API/SpeedRunners.Model/User/MNotification.cs#L5-L144)

## 通知机制详解

### 通知类型和触发条件

**回复通知 (Reply)**
- **触发条件**: 当用户回复他人评论或回复顶级评论时触发
- **发送逻辑**: 
  - 检查 ReplyToPlatformID（回复他人评论）
  - 检查 ParentID（回复顶级评论）
  - 排除自己回复自己的情况
- **通知内容**: 包含回复者的头像、昵称、评论内容摘要和页面路径

**点赞通知 (Like)**
- **触发条件**: 当用户对他人评论进行点赞时触发（取消点赞时不触发）
- **发送逻辑**: 
  - 检查 isLiked 返回值确认是点赞操作
  - 排除自己点赞自己的情况
- **通知内容**: 包含点赞者的头像、昵称、被点赞评论的内容摘要和页面路径

### 通知去重和频率控制

```mermaid
flowchart TD
Start([添加通知]) --> CheckSameUser["检查是否同一用户"]
CheckSameUser --> SameUser{"同一用户?"}
SameUser --> |是| CheckTimeRange["检查24小时时间范围"]
CheckTimeRange --> TimeRange{"24小时内?"}
TimeRange --> |是| CheckSameType["检查相同通知类型"]
CheckSameType --> SameType{"相同类型?"}
SameType --> |是| CheckSameComment["检查相同评论ID"]
CheckSameComment --> SameComment{"相同评论?"}
SameComment --> |是| Skip([跳过通知])
SameComment --> |否| Create([创建新通知])
SameType --> |否| Create
TimeRange --> |否| Create
SameUser --> |否| Create
Create --> End([通知创建成功])
Skip --> End
```

**图表来源**
- [NotificationBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/NotificationBLL.cs#L47-L48)

### 通知存储和查询

**存储机制**
- 使用 NotificationDAL 进行通知消息的持久化存储
- 支持通知内容的截断处理（标题和消息长度限制）
- 自动设置创建时间和默认已读状态

**查询机制**
- 支持按类型筛选（回复通知、点赞通知）
- 支持按已读状态筛选
- 支持分页查询和总数统计
- 提供未读消息数量统计功能

**清理机制**
- 提供清理过期通知的功能
- 支持定时任务自动清理历史通知
- 保持数据库表的整洁和性能

**章节来源**
- [NotificationBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/NotificationBLL.cs#L39-L96)
- [NotificationController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/NotificationController.cs#L15-L45)

## 依赖关系分析

```mermaid
graph LR
subgraph "外部依赖"
Dapper[Dapper ORM]
MySQL[MySQL Connector]
ASPNET[ASP.NET Core]
end
subgraph "内部模块"
CommentController
NotificationController
CommentBLL
NotificationBLL
RankBLL
CommentDAL
NotificationDAL
MComment
MCommentParam
MCommentOut
MCommentLike
MNotification
end
CommentController --> CommentBLL
NotificationController --> NotificationBLL
CommentBLL --> CommentDAL
CommentBLL --> NotificationBLL
CommentBLL --> RankBLL
NotificationBLL --> NotificationDAL
CommentDAL --> MySQL
NotificationDAL --> MySQL
CommentBLL --> Dapper
NotificationBLL --> Dapper
CommentController --> ASPNET
NotificationController --> ASPNET
```

**图表来源**
- [CommentController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/CommentController.cs#L1-L8)
- [NotificationController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/NotificationController.cs#L1-L8)
- [CommentBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/CommentBLL.cs#L1-L5)
- [NotificationBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/NotificationBLL.cs#L1-L5)

**章节来源**
- [CommentBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/CommentBLL.cs#L1-L18)
- [NotificationBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/NotificationBLL.cs#L1-L107)

## 性能考虑

### 查询优化策略
1. **批量查询**: 使用 QueryMultiple 减少数据库连接次数
2. **索引优化**: PagePath 和 ParentID 字段应建立适当索引
3. **分页处理**: 合理设置 PageSize，避免大数据量查询
4. **缓存策略**: 用户头像和昵称信息可考虑缓存

### 并发控制
- 使用数据库事务确保数据一致性
- 点赞操作采用原子性更新
- 防止重复提交的机制

### 通知性能优化
- **去重机制**: 24小时内相同用户相同类型的重复通知会被跳过
- **批量清理**: 定期清理过期通知，避免表膨胀
- **分页查询**: 通知列表支持分页，避免一次性加载大量数据

## 故障排除指南

### 常见问题及解决方案

#### 权限相关错误
- **问题**: 无权限删除评论
- **原因**: 非评论作者或非管理员用户
- **解决方案**: 检查 CurrentUser.PlatformID 和管理员标识

#### 评论内容验证失败
- **问题**: 评论长度超出限制或为空
- **原因**: 内容验证规则触发
- **解决方案**: 确保评论内容在 1-2000 字符范围内

#### 数据库连接问题
- **问题**: SQL 查询执行失败
- **原因**: 数据库连接字符串配置错误
- **解决方案**: 检查连接字符串和数据库服务状态

#### 通知发送失败
- **问题**: 通知无法正常发送
- **原因**: 通知BLL初始化失败或数据库连接异常
- **解决方案**: 检查 NotificationBLL 的依赖注入和数据库连接状态

#### 通知去重失效
- **问题**: 重复通知仍然发送
- **原因**: 时间范围检查或用户ID检查逻辑异常
- **解决方案**: 验证 24 小时时间窗口和用户ID匹配逻辑

**章节来源**
- [CommentBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/CommentBLL.cs#L116-L120)
- [CommentBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/CommentBLL.cs#L46-L49)
- [NotificationBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/NotificationBLL.cs#L47-L48)

## 总结

SpeedRunnersLab 评论系统是一个设计良好的完整解决方案，具有以下特点：

### 技术优势
- **清晰的架构层次**: 三层架构确保了代码的可维护性和可测试性
- **完善的业务逻辑**: 支持多级评论、点赞、通知等核心功能
- **智能通知机制**: 支持回复和点赞的实时通知，包含去重和频率控制
- **合理的数据模型**: 清晰的实体关系和参数设计
- **性能优化**: 使用 Dapper 和批量查询提升性能

### 功能特性
- 支持页面级评论系统
- 多级回复机制
- 实时点赞功能
- 智能通知系统（去重和频率控制）
- 管理员权限控制
- 通知清理和管理功能

### 扩展建议
1. 添加评论搜索功能
2. 实现评论举报机制
3. 增加评论审核流程
4. 优化图片和多媒体内容支持
5. 扩展通知渠道（邮件、站内信等）

该评论系统为 SpeedRunners 社区提供了坚实的技术基础，能够满足游戏社区的各种互动需求。新增的通知机制进一步增强了用户体验，使社区互动更加及时和有效。