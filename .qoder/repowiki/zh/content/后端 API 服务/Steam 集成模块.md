# Steam 集成模块

<cite>
**本文引用的文件**
- [SteamBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/SteamBLL.cs)
- [ProfileBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/ProfileBLL.cs)
- [AchievementSchemaService.cs](file://SpeedRunners.API/SpeedRunners.BLL/AchievementSchemaService.cs)
- [SteamController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/SteamController.cs)
- [ProfileController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/ProfileController.cs)
- [ProfileController.cs](file://SpeedRunners.API/SpeedRunners/Controllers/ProfileController.cs)
- [ProfileBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/ProfileBLL.cs)
- [MSteamAchievement.cs](file://SpeedRunners.API/SpeedRunners.Model/Steam/MSteamAchievement.cs)
- [MAchievementSchema.cs](file://SpeedRunners.API/SpeedRunners.Model/Steam/MAchievementSchema.cs)
- [MAchievement.cs](file://SpeedRunners.API/SpeedRunners.Model/Profile/MAchievement.cs)
- [BaseBLL.cs](file://SpeedRunners.API/SpeedRunners.Utils/BaseBLL.cs)
- [Startup.cs](file://SpeedRunners.API/SpeedRunners/Startup.cs)
- [appsettings.json](file://SpeedRunners.API/SpeedRunners/appsettings.json)
- [HttpHelper.cs](file://SpeedRunners.API/SpeedRunners.Utils/HttpHelper.cs)
- [SRLabTokenAuthMidd.cs](file://SpeedRunners.API/SpeedRunners/Middleware/SRLabTokenAuthMidd.cs)
- [UserBLL.cs](file://SpeedRunners.API/SpeedRunners.BLL/UserBLL.cs)
- [MUser.cs](file://SpeedRunners.API/SpeedRunners.Model/MUser.cs)
- [steam.js](file://SpeedRunners.UI/src/api/steam.js)
- [profile.js](file://SpeedRunners.UI/src/api/profile.js)
- [index.vue](file://SpeedRunners.UI/src/views/searchPlayer/index.vue)
- [index.vue](file://SpeedRunners.UI/src/views/profile/index.vue)
- [Task.cs](file://SpeedRunners.Scheduler/Task.cs)
- [HttpRequestBase.cs](file://SpeedRunners.Scheduler/HttpRequestBase.cs)
</cite>

## 更新摘要
**变更内容**
- 新增AchievementSchemaService缓存服务，提供Steam成就定义的内存缓存
- 改进ProfileBLL的成就数据获取流程，增强错误处理和回退机制
- 优化Steam API调用性能，提供更稳定的成就数据获取
- 在Startup.cs中注册内存缓存和AchievementSchemaService服务
- ProfileController新增GetAchievements接口，支持成就数据获取

## 目录
1. [简介](#简介)
2. [项目结构](#项目结构)
3. [核心组件](#核心组件)
4. [架构总览](#架构总览)
5. [组件详解](#组件详解)
6. [依赖关系分析](#依赖关系分析)
7. [性能与限流](#性能与限流)
8. [故障排查指南](#故障排查指南)
9. [结论](#结论)
10. [附录：API 接口规范](#附录api-接口规范)

## 简介
本技术文档聚焦于 SpeedRunners 项目中的 Steam 集成模块，系统性阐述以下内容：
- Steam Web API 的集成方案与数据同步机制
- 玩家信息查询、游戏数据获取、好友/社区玩家列表处理的实现原理
- **新增**：Steam成就数据获取与解析功能，包括成就定义缓存服务
- SteamBLL 业务逻辑层如何封装第三方 API 调用、处理 API 限制与错误重试
- **新增**：AchievementSchemaService缓存服务的设计与实现
- Steam 认证流程、API 密钥管理与安全防护
- 完整的 API 接口规范（含路径、参数、响应结构与异常处理）
- 前后端调用示例与最佳实践

## 项目结构
围绕 Steam 集成的关键文件分布如下：
- 控制器层：SteamController 和 ProfileController 提供对外 REST 接口
- 业务逻辑层：SteamBLL、ProfileBLL 和 AchievementSchemaService 封装 Steam API 调用与数据转换
- 模型层：MSearchPlayerResult、MSteamAchievement、MAchievementSchema、MAchievement 等用于前后端数据契约
- 工具与配置：AppSettings、HttpHelper、Startup 中的中间件与本地化
- 前端：UI 层通过 steam.js 和 profile.js 发起请求，index.vue 展示结果
- 后台任务：Scheduler 中定时批量拉取 Steam 数据并写入数据库

```mermaid
graph TB
subgraph "API 层"
C["SteamController<br/>控制器"]
PC["ProfileController<br/>个人主页控制器"]
end
subgraph "业务层"
B["SteamBLL<br/>业务逻辑"]
PB["ProfileBLL<br/>个人主页业务"]
ASC["AchievementSchemaService<br/>成就定义缓存服务"]
end
subgraph "模型与工具"
M["MSearchPlayerResult<br/>玩家搜索模型"]
SA["MSteamAchievement<br/>Steam成就模型"]
AS["MAchievementSchema<br/>成就定义模型"]
PA["MAchievement<br/>本地成就模型"]
U["BaseBLL<br/>基类"]
H["HttpHelper<br/>HTTP 工具"]
A["AppSettings<br/>配置读取"]
MC["IMemoryCache<br/>内存缓存"]
end
subgraph "外部服务"
S["Steam Web API"]
SC["Steam Community"]
end
subgraph "前端"
F["steam.js<br/>Steam请求"]
PF["profile.js<br/>个人主页请求"]
V["searchPlayer/index.vue<br/>玩家搜索视图"]
PV["profile/index.vue<br/>个人主页视图"]
end
subgraph "后台任务"
T["Task.cs<br/>定时任务"]
HB["HttpRequestBase<br/>HTTP 客户端"]
end
F --> C
PF --> PC
C --> B
PC --> PB
PB --> ASC
PB --> B
ASC --> MC
ASC --> H
B --> H
B --> A
B --> M
B --> SA
ASC --> AS
PB --> PA
B --> S
B --> SC
V --> F
PV --> PF
T --> HB
T --> S
```

**图表来源**
- [SteamController.cs:1-28](file://SpeedRunners.API/SpeedRunners/Controllers/SteamController.cs#L1-L28)
- [ProfileController.cs:1-41](file://SpeedRunners.API/SpeedRunners/Controllers/ProfileController.cs#L1-L41)
- [SteamBLL.cs:1-505](file://SpeedRunners.API/SpeedRunners.BLL/SteamBLL.cs#L1-L505)
- [ProfileBLL.cs:108-168](file://SpeedRunners.API/SpeedRunners.BLL/ProfileBLL.cs#L108-L168)
- [AchievementSchemaService.cs:1-110](file://SpeedRunners.API/SpeedRunners.BLL/AchievementSchemaService.cs#L1-L110)
- [MSteamAchievement.cs:1-15](file://SpeedRunners.API/SpeedRunners.Model/Steam/MSteamAchievement.cs#L1-L15)
- [MAchievementSchema.cs:1-41](file://SpeedRunners.API/SpeedRunners.Model/Steam/MAchievementSchema.cs#L1-L41)
- [MAchievement.cs:1-51](file://SpeedRunners.API/SpeedRunners.Model/Profile/MAchievement.cs#L1-L51)

**章节来源**
- [SteamController.cs:1-28](file://SpeedRunners.API/SpeedRunners/Controllers/SteamController.cs#L1-L28)
- [ProfileController.cs:1-41](file://SpeedRunners.API/SpeedRunners/Controllers/ProfileController.cs#L1-L41)
- [SteamBLL.cs:1-505](file://SpeedRunners.API/SpeedRunners.BLL/SteamBLL.cs#L1-L505)
- [ProfileBLL.cs:108-168](file://SpeedRunners.API/SpeedRunners.BLL/ProfileBLL.cs#L108-L168)
- [AchievementSchemaService.cs:1-110](file://SpeedRunners.API/SpeedRunners.BLL/AchievementSchemaService.cs#L1-L110)
- [MSteamAchievement.cs:1-15](file://SpeedRunners.API/SpeedRunners.Model/Steam/MSteamAchievement.cs#L1-L15)
- [MAchievementSchema.cs:1-41](file://SpeedRunners.API/SpeedRunners.Model/Steam/MAchievementSchema.cs#L1-L41)
- [MAchievement.cs:1-51](file://SpeedRunners.API/SpeedRunners.Model/Profile/MAchievement.cs#L1-L51)

## 核心组件
- SteamController：暴露 REST 接口，路由形如 api/steam/{action}
- **新增**：ProfileController：提供个人主页相关接口，包括成就获取
- SteamBLL：封装 Steam API 调用，包含玩家信息、游戏数据、在线人数、玩家搜索、社区列表和**新增的成就数据获取**等方法
- **新增**：ProfileBLL：整合Steam成就与本地成就定义，提供成就状态关联，依赖AchievementSchemaService
- **新增**：AchievementSchemaService：成就定义缓存服务，使用内存缓存存储Steam成就定义，避免频繁调用Steam API
- MSearchPlayerResult：统一返回结构，支持"游戏统计"和"玩家列表"两种模式
- MSteamAchievement：Steam成就数据模型，包含成就名称、解锁状态和解锁时间
- **新增**：MAchievementSchema：Steam成就定义模型，包含成就显示名称、描述、图标等
- MAchievement：本地成就模型，扩展Steam成就数据，包含显示名称、描述、图标等
- BaseBLL：提供当前用户上下文、HttpContext、本地化等基础能力
- HttpHelper/AppSettings：统一 HTTP 请求与配置读取（含 API Key）
- **新增**：IMemoryCache：内存缓存服务，用于AchievementSchemaService缓存成就定义
- 前端 steam.js、profile.js 与 index.vue：发起请求并渲染结果
- Scheduler Task：定时批量抓取 Steam 用户信息并写库

**章节来源**
- [SteamController.cs:1-28](file://SpeedRunners.API/SpeedRunners/Controllers/SteamController.cs#L1-L28)
- [ProfileController.cs:1-41](file://SpeedRunners.API/SpeedRunners/Controllers/ProfileController.cs#L1-L41)
- [SteamBLL.cs:1-505](file://SpeedRunners.API/SpeedRunners.BLL/SteamBLL.cs#L1-L505)
- [ProfileBLL.cs:108-168](file://SpeedRunners.API/SpeedRunners.BLL/ProfileBLL.cs#L108-L168)
- [AchievementSchemaService.cs:1-110](file://SpeedRunners.API/SpeedRunners.BLL/AchievementSchemaService.cs#L1-L110)
- [MSearchPlayerResult.cs:1-38](file://SpeedRunners.API/SpeedRunners.Model/Steam/MSearchPlayerResult.cs#L1-L38)
- [MSteamAchievement.cs:1-15](file://SpeedRunners.API/SpeedRunners.Model/Steam/MSteamAchievement.cs#L1-L15)
- [MAchievementSchema.cs:1-41](file://SpeedRunners.API/SpeedRunners.Model/Steam/MAchievementSchema.cs#L1-L41)
- [MAchievement.cs:1-51](file://SpeedRunners.API/SpeedRunners.Model/Profile/MAchievement.cs#L1-L51)

## 架构总览
Steam 集成采用"控制器 → 业务层 → 第三方 API"的分层设计。业务层通过 SteamWebAPI2 库与 Steam Web API 交互，并在必要时回退到自建 HTTP 请求（如社区搜索）。**新增的成就数据获取通过AchievementSchemaService缓存服务和SteamBLL的直接HTTP请求实现**。控制器负责参数绑定与返回包装，前端通过 axios 风格封装进行调用。

```mermaid
sequenceDiagram
participant FE as "前端(profile/index.vue)"
participant API as "ProfileController"
participant PB as "ProfileBLL"
participant ASC as "AchievementSchemaService"
participant SBLL as "SteamBLL"
participant MC as "IMemoryCache"
participant HTTP as "HttpHelper"
participant STEAM as "Steam API"
FE->>API : GET /profile/getAchievements/{steamId}
API->>PB : GetAchievements(steamId)
PB->>ASC : GetAchievementSchemaAsync()
ALT 缓存命中
ASC->>MC : 从缓存获取成就定义
MC-->>ASC : 成就定义列表
ELSE 缓存未命中
ASC->>HTTP : GET ISteamUserStats/GetSchemaForGame
HTTP-->>ASC : JSON 成就定义
ASC->>MC : 写入缓存24小时
END
ASC-->>PB : List<MAchievementSchema>
PB->>SBLL : GetPlayerAchievements(steamId)
SBLL->>HTTP : GET ISteamUserStats/GetPlayerAchievements
HTTP-->>SBLL : JSON 成就数据
SBLL-->>PB : List<MSteamAchievement>
PB->>PB : 关联本地成就定义
PB-->>API : List<MAchievement>
API-->>FE : 返回成就列表
```

**图表来源**
- [ProfileController.cs:32-39](file://SpeedRunners.API/SpeedRunners/Controllers/ProfileController.cs#L32-L39)
- [ProfileBLL.cs:115-168](file://SpeedRunners.API/SpeedRunners.BLL/ProfileBLL.cs#L115-L168)
- [AchievementSchemaService.cs:34-49](file://SpeedRunners.API/SpeedRunners.BLL/AchievementSchemaService.cs#L34-L49)
- [SteamBLL.cs:113-163](file://SpeedRunners.API/SpeedRunners.BLL/SteamBLL.cs#L113-L163)
- [MSteamAchievement.cs:8-13](file://SpeedRunners.API/SpeedRunners.Model/Steam/MSteamAchievement.cs#L8-L13)
- [MAchievement.cs:8-51](file://SpeedRunners.API/SpeedRunners.Model/Profile/MAchievement.cs#L8-L51)

## 组件详解

### AchievementSchemaService 缓存服务
- **新增**：核心职责
  - 从Steam API获取SpeedRunners游戏的成就定义并缓存
  - 使用IMemoryCache提供24小时缓存，避免频繁调用Steam API
  - 提供RefreshCacheAsync方法手动刷新缓存
- **缓存策略**
  - 缓存键：SpeedRunners_AchievementSchema
  - 缓存时长：24小时
  - 缓存失效：自动过期
- **数据获取流程**
  - 首先检查内存缓存，命中则直接返回
  - 未命中则调用Steam API的GetSchemaForGame接口
  - 解析JSON响应，提取成就定义列表
  - 将成就定义写入缓存并返回
- **错误处理**
  - Steam API调用异常时返回空列表
  - JSON解析失败时返回空列表
  - 网络请求异常时返回空列表

```mermaid
flowchart TD
Start(["进入GetAchievementSchemaAsync"]) --> CheckCache["检查内存缓存"]
CheckCache --> CacheHit{"缓存命中？"}
CacheHit --> |是| ReturnCache["返回缓存数据"]
CacheHit --> |否| CallSteam["调用Steam API GetSchemaForGame"]
CallSteam --> ParseJSON["解析JSON响应"]
ParseJSON --> CheckSuccess{"响应有效？"}
CheckSuccess --> |否| ReturnEmpty["返回空列表"]
CheckSuccess --> |是| ExtractSchema["提取成就定义"]
ExtractSchema --> CacheData["写入内存缓存24小时"]
CacheData --> ReturnSchema["返回成就定义列表"]
ReturnCache --> End(["结束"])
ReturnEmpty --> End
ReturnSchema --> End
```

**图表来源**
- [AchievementSchemaService.cs:34-49](file://SpeedRunners.API/SpeedRunners.BLL/AchievementSchemaService.cs#L34-L49)
- [AchievementSchemaService.cs:54-98](file://SpeedRunners.API/SpeedRunners.BLL/AchievementSchemaService.cs#L54-L98)

**章节来源**
- [AchievementSchemaService.cs:1-110](file://SpeedRunners.API/SpeedRunners.BLL/AchievementSchemaService.cs#L1-L110)

### SteamBLL 业务逻辑层
- 关键职责
  - 封装 Steam Web API 调用（用户摘要、拥有游戏、最近游戏、在线人数）
  - **新增**：GetPlayerAchievements 方法，通过 Steam Web API 获取玩家成就数据
  - 封装社区搜索（解析 HTML 提取玩家列表），并注入 Cookie 与国家信息
  - 玩家搜索多路并行策略：优先 SteamID64 或 CustomURL，否则回退社区用户列表
  - 游戏统计中文名映射（按游戏术语表替换）
  - 天梯分查询（对接自有服务，分批请求）
- **新增**：GetPlayerAchievements 实现细节
  - 构造 Steam Web API GetPlayerAchievements 请求URL
  - 解析返回的JSON数据，提取成就列表
  - 将 Unix 时间戳转换为 DateTime 格式
  - 返回 MSteamAchievement 对象列表
  - **增强**：添加try-catch异常处理，失败时返回null而非抛出异常
- 错误处理
  - 对 HTTP 请求进行 try/catch，失败返回 null 以避免中断整体流程
  - 对社区搜索返回 success 字段校验，确保数据有效性
  - 对成就数据解析进行健壮性检查
  - **增强**：Steam API调用失败时返回null，让上层逻辑进行回退处理
- 性能优化
  - 使用分组批量请求（每批最多 99 个 ID）减少第三方调用次数
  - 并行执行多个搜索分支，缩短首字节时间
  - **新增**：通过AchievementSchemaService缓存成就定义，减少Steam API调用频率

```mermaid
flowchart TD
Start(["进入 GetPlayerAchievements"]) --> BuildUrl["构造Steam API URL"]
BuildUrl --> HttpGet["HttpHelper.HttpGet()"]
HttpGet --> TryCatch{"try-catch包裹"}
TryCatch --> HttpError{"HTTP请求异常？"}
HttpError --> |是| ReturnNull["返回null"]
HttpError --> |否| CheckResponse{"响应为空？"}
CheckResponse --> |是| ReturnNull
CheckResponse --> |否| ParseJson["解析JSON响应"]
ParseJson --> CheckSuccess{"success字段验证"}
CheckSuccess --> |失败| ReturnNull
CheckSuccess --> |成功| ExtractAch["提取成就列表"]
ExtractAch --> ParseUnlockTime{"解锁时间检查"}
ParseUnlockTime --> |有时间| ConvertTime["转换Unix时间戳"]
ParseUnlockTime --> |无时间| SkipTime["跳过时间处理"]
ConvertTime --> CreateModel["创建MSteamAchievement对象"]
SkipTime --> CreateModel
CreateModel --> AddToList["添加到结果列表"]
AddToList --> ReturnList["返回成就列表"]
ReturnNull --> End(["结束"])
ReturnList --> End
```

**图表来源**
- [SteamBLL.cs:113-163](file://SpeedRunners.API/SpeedRunners.BLL/SteamBLL.cs#L113-L163)
- [MSteamAchievement.cs:8-13](file://SpeedRunners.API/SpeedRunners.Model/Steam/MSteamAchievement.cs#L8-L13)

**章节来源**
- [SteamBLL.cs:1-505](file://SpeedRunners.API/SpeedRunners.BLL/SteamBLL.cs#L1-L505)

### ProfileBLL 业务逻辑层
- **新增**：GetAchievements 方法实现
  - **增强**：首先通过AchievementSchemaService获取缓存的成就定义
  - **增强**：如果缓存为空或获取失败，返回空成就列表而非抛出异常
  - **增强**：转换成就定义为MAchievement模型，包含图标URL和隐藏状态
  - **增强**：Steam API调用失败时捕获异常，但不中断整个流程
  - **增强**：按解锁状态、解锁时间和名称进行排序
  - **增强**：隐藏成就的描述处理，如果为空则显示本地化文本
- **依赖关系**
  - 依赖AchievementSchemaService进行成就定义缓存
  - 依赖SteamBLL进行玩家成就状态获取

**章节来源**
- [ProfileBLL.cs:115-168](file://SpeedRunners.API/SpeedRunners.BLL/ProfileBLL.cs#L115-L168)

### 控制器与路由
- 路由约定：api/steam/{action} 和 api/profile/{action}
- **新增**：ProfileController 支持的动作
  - GetData(steamId)：获取个人主页数据
  - GetDailyScoreHistory(steamId)：获取每日天梯分历史记录
  - **新增**：GetAchievements(steamId)：获取玩家成就列表
- 支持动作
  - SearchPlayer(keyWords)
  - GetPlayerList(userName, sessionID, pageNo)
  - SearchPlayerByUrl(url)
  - SearchPlayerBySteamID64(steamID64)
  - GetOnlineCount()

**章节来源**
- [SteamController.cs:1-28](file://SpeedRunners.API/SpeedRunners/Controllers/SteamController.cs#L1-L28)
- [ProfileController.cs:1-41](file://SpeedRunners.API/SpeedRunners/Controllers/ProfileController.cs#L1-L41)

### 模型与数据契约
- MSearchPlayerResult
  - IsGameInfo：是否返回游戏统计
  - GameInfo：JSON 游戏统计（含 stats 数组）
  - PlayerList：玩家列表（头像、用户名、ProfilesOrID、ContentOfID）
  - Total/PageNo/SessionID：社区列表分页与会话信息
- MSteamAchievement：Steam成就数据模型，包含成就名称、解锁状态和解锁时间
- **新增**：MAchievementSchema：Steam成就定义模型，包含成就显示名称、描述、图标等
- MAchievement：本地成就模型，扩展Steam成就数据，包含显示名称、描述、图标等
- **增强**：MAchievement模型新增Hidden属性，支持隐藏成就的显示控制

**章节来源**
- [MSearchPlayerResult.cs:1-38](file://SpeedRunners.API/SpeedRunners.Model/Steam/MSearchPlayerResult.cs#L1-L38)
- [MSteamAchievement.cs:1-15](file://SpeedRunners.API/SpeedRunners.Model/Steam/MSteamAchievement.cs#L1-L15)
- [MAchievementSchema.cs:1-41](file://SpeedRunners.API/SpeedRunners.Model/Steam/MAchievementSchema.cs#L1-L41)
- [MAchievement.cs:1-51](file://SpeedRunners.API/SpeedRunners.Model/Profile/MAchievement.cs#L1-L51)

### 前端集成
- steam.js：封装 GET 请求到各 SteamController 动作
- **新增**：profile.js：封装 GET 请求到 ProfileController 的个人主页相关接口
- index.vue（搜索页面）：输入关键词触发搜索，支持点击列表项进一步按 SteamID64 或 CustomURL 查询
- **新增**：index.vue（个人主页）：显示玩家成就，支持解锁状态和解锁时间展示，包含隐藏成就的处理
- 本地化：通过请求头 locale 决定游戏统计字段中文名映射

**章节来源**
- [steam.js:1-36](file://SpeedRunners.UI/src/api/steam.js#L1-L36)
- [profile.js:1-26](file://SpeedRunners.UI/src/api/profile.js#L1-L26)
- [index.vue:1-169](file://SpeedRunners.UI/src/views/searchPlayer/index.vue#L1-L169)
- [index.vue:1-760](file://SpeedRunners.UI/src/views/profile/index.vue#L1-L760)

### 后台任务与数据同步
- 定时任务 Task：周期性批量抓取 Steam 用户信息（头像、状态、当前游戏等）
- 批处理策略：按 100 人一组分批请求，解析 JSON 并写入数据库
- HTTP 客户端：HttpRequestBase 创建带超时与代理的 HttpClient

```mermaid
sequenceDiagram
participant SCH as "Scheduler Task"
participant HB as "HttpRequestBase"
participant API as "Steam API"
participant DB as "数据库"
SCH->>HB : CreateHttpClient()
loop 分批
SCH->>API : GetPlayerSummaries(批量 SteamID)
API-->>SCH : JSON response.players
SCH->>DB : 更新头像/状态/当前游戏等字段
end
```

**图表来源**
- [Task.cs:295-328](file://SpeedRunners.Scheduler/Task.cs#L295-L328)
- [HttpRequestBase.cs:11-18](file://SpeedRunners.Scheduler/HttpRequestBase.cs#L11-L18)

**章节来源**
- [Task.cs:1-330](file://SpeedRunners.Scheduler/Task.cs#L1-L330)
- [HttpRequestBase.cs:1-42](file://SpeedRunners.Scheduler/HttpRequestBase.cs#L1-L42)

## 依赖关系分析
- 控制器依赖业务层，业务层依赖工具类与配置
- **新增**：ProfileController 依赖 ProfileBLL，ProfileBLL 依赖 AchievementSchemaService 和 SteamBLL
- AchievementSchemaService 依赖 IMemoryCache 和 SteamBLL
- 业务层同时依赖 SteamWebAPI2 与自建 HttpHelper
- 前端依赖控制器提供的 REST 接口
- 后台任务独立于 API 层，直接访问 Steam API
- **新增**：Startup.cs 注册内存缓存和AchievementSchemaService服务

```mermaid
classDiagram
class SteamController {
+SearchPlayer(keyWords)
+GetPlayerList(userName, sessionID, pageNo)
+SearchPlayerByUrl(url)
+SearchPlayerBySteamID64(steamID64)
+GetOnlineCount()
}
class ProfileController {
+GetData(steamId)
+GetDailyScoreHistory(steamId)
+GetAchievements(steamId)
}
class SteamBLL {
+SearchPlayer()
+GetPlayerSummary()
+GetOwnedGames()
+GetRecentlyPlayedGames()
+GetNumberOfCurrentPlayersForGame()
+GetPlayerAchievements()
-GetSessionID()
-GetSteamID64ByUrl()
-ToChinese()
}
class ProfileBLL {
+GetProfileData()
+GetDailyScoreHistory()
+GetAchievements()
}
class AchievementSchemaService {
+GetAchievementSchemaAsync()
+RefreshCacheAsync()
-FetchAchievementSchemaFromSteam()
}
class MSearchPlayerResult {
+bool IsGameInfo
+JToken GameInfo
+int Total
+int PageNo
+string SessionID
+PlayerInfo[] PlayerList
}
class MSteamAchievement {
+string ApiName
+int Achieved
+DateTime UnlockTime
}
class MAchievementSchema {
+string ApiName
+string DisplayName
+string Description
+string Icon
+string IconGray
+bool Hidden
}
class MAchievement {
+int Id
+string ApiName
+string Name
+string Description
+string IconUrl
+string IconGrayUrl
+bool Unlocked
+DateTime UnlockedAt
+bool Hidden
}
class BaseBLL {
+CurrentUser
+HttpContext
+Localizer
}
class HttpHelper
class AppSettings
class IMemoryCache
SteamController --> SteamBLL : "调用"
ProfileController --> ProfileBLL : "调用"
ProfileBLL --> AchievementSchemaService : "调用"
ProfileBLL --> SteamBLL : "调用"
AchievementSchemaService --> IMemoryCache : "使用"
AchievementSchemaService --> SteamBLL : "调用"
SteamBLL --> HttpHelper : "HTTP 请求"
SteamBLL --> AppSettings : "读取配置"
SteamBLL --> MSearchPlayerResult : "返回"
SteamBLL --> MSteamAchievement : "返回"
ProfileBLL --> MAchievement : "返回"
AchievementSchemaService --> MAchievementSchema : "返回"
SteamBLL --|> BaseBLL : "继承"
```

**图表来源**
- [SteamController.cs:1-28](file://SpeedRunners.API/SpeedRunners/Controllers/SteamController.cs#L1-L28)
- [ProfileController.cs:1-41](file://SpeedRunners.API/SpeedRunners/Controllers/ProfileController.cs#L1-L41)
- [SteamBLL.cs:1-505](file://SpeedRunners.API/SpeedRunners.BLL/SteamBLL.cs#L1-L505)
- [ProfileBLL.cs:108-168](file://SpeedRunners.API/SpeedRunners.BLL/ProfileBLL.cs#L108-L168)
- [AchievementSchemaService.cs:1-110](file://SpeedRunners.API/SpeedRunners.BLL/AchievementSchemaService.cs#L1-L110)
- [MSearchPlayerResult.cs:1-38](file://SpeedRunners.API/SpeedRunners.Model/Steam/MSearchPlayerResult.cs#L1-L38)
- [MSteamAchievement.cs:1-15](file://SpeedRunners.API/SpeedRunners.Model/Steam/MSteamAchievement.cs#L1-L15)
- [MAchievementSchema.cs:1-41](file://SpeedRunners.API/SpeedRunners.Model/Steam/MAchievementSchema.cs#L1-L41)
- [MAchievement.cs:1-51](file://SpeedRunners.API/SpeedRunners.Model/Profile/MAchievement.cs#L1-L51)
- [BaseBLL.cs:1-17](file://SpeedRunners.API/SpeedRunners.Utils/BaseBLL.cs#L1-L17)
- [HttpHelper.cs:37-76](file://SpeedRunners.API/SpeedRunners.Utils/HttpHelper.cs#L37-L76)
- [appsettings.json:1-21](file://SpeedRunners.API/SpeedRunners/appsettings.json#L1-L21)

**章节来源**
- [SteamController.cs:1-28](file://SpeedRunners.API/SpeedRunners/Controllers/SteamController.cs#L1-L28)
- [ProfileController.cs:1-41](file://SpeedRunners.API/SpeedRunners/Controllers/ProfileController.cs#L1-L41)
- [SteamBLL.cs:1-505](file://SpeedRunners.API/SpeedRunners.BLL/SteamBLL.cs#L1-L505)
- [ProfileBLL.cs:108-168](file://SpeedRunners.API/SpeedRunners.BLL/ProfileBLL.cs#L108-L168)
- [AchievementSchemaService.cs:1-110](file://SpeedRunners.API/SpeedRunners.BLL/AchievementSchemaService.cs#L1-L110)
- [MSearchPlayerResult.cs:1-38](file://SpeedRunners.API/SpeedRunners.Model/Steam/MSearchPlayerResult.cs#L1-L38)
- [MSteamAchievement.cs:1-15](file://SpeedRunners.API/SpeedRunners.Model/Steam/MSteamAchievement.cs#L1-L15)
- [MAchievementSchema.cs:1-41](file://SpeedRunners.API/SpeedRunners.Model/Steam/MAchievementSchema.cs#L1-L41)
- [MAchievement.cs:1-51](file://SpeedRunners.API/SpeedRunners.Model/Profile/MAchievement.cs#L1-L51)
- [BaseBLL.cs:1-17](file://SpeedRunners.API/SpeedRunners.Utils/BaseBLL.cs#L1-L17)
- [HttpHelper.cs:37-76](file://SpeedRunners.API/SpeedRunners.Utils/HttpHelper.cs#L37-L76)
- [appsettings.json:1-21](file://SpeedRunners.API/SpeedRunners/appsettings.json#L1-L21)

## 性能与限流
- 批量请求：天梯分与后台任务均采用分组批量请求，降低 API 调用次数
- 并行搜索：玩家搜索同时尝试多种路径，缩短等待时间
- **新增**：AchievementSchemaService缓存服务显著减少Steam API调用频率
- **新增**：内存缓存策略：成就定义缓存24小时，避免频繁调用Steam API
- **增强**：成就数据获取的错误处理机制，确保API调用失败不影响整体功能
- **增强**：Steam API调用失败时返回null，让上层逻辑进行优雅降级
- 超时与代理：后台任务客户端设置超时与代理，提升稳定性
- 建议
  - 在高并发场景下，可引入令牌桶/漏桶限流与指数退避重试
  - 对社区搜索增加缓存与去重，避免重复解析 HTML
  - 对 Steam API 限流阈值进行监控与告警
  - **新增**：考虑为AchievementSchemaService添加缓存刷新策略
  - **新增**：监控内存缓存命中率，优化缓存策略

**章节来源**
- [SteamBLL.cs:55-56](file://SpeedRunners.API/SpeedRunners.BLL/SteamBLL.cs#L55-L56)
- [AchievementSchemaService.cs:20-22](file://SpeedRunners.API/SpeedRunners.BLL/AchievementSchemaService.cs#L20-L22)
- [Task.cs:299-306](file://SpeedRunners.Scheduler/Task.cs#L299-L306)
- [HttpRequestBase.cs:11-18](file://SpeedRunners.Scheduler/HttpRequestBase.cs#L11-L18)
- [ProfileBLL.cs:139-160](file://SpeedRunners.API/SpeedRunners.BLL/ProfileBLL.cs#L139-L160)

## 故障排查指南
- API 密钥问题
  - 确认 appsettings.json 中 ApiKey 配置正确且可访问
  - 若密钥无效，Steam API 返回失败，业务层会返回空结果
- 社区搜索失败
  - 检查 sessionid 与 steamCountry Cookie 是否注入
  - 确认 g_sessionID 正则匹配成功
- **新增**：成就数据获取失败
  - 检查 AchievementSchemaService 缓存是否正常工作
  - 确认 Steam Web API 的 GetSchemaForGame 接口是否可用
  - 检查 Steam Web API 的 GetPlayerAchievements 接口是否可用
  - 确认 steamID 参数格式正确（必须为64位数字）
  - 验证游戏ID（AppId）配置是否正确
- **新增**：缓存相关问题
  - 检查内存缓存服务是否正常注册
  - 确认缓存键 SpeedRunners_AchievementSchema 是否正确
  - 验证缓存时长设置（24小时）
- HTTP 超时/代理
  - 后台任务客户端默认启用代理与短超时，必要时调整
- 认证与权限
  - 前端需携带有效 Token，后端中间件会校验平台 ID
  - 登录成功后返回 Token，后续请求需附带

**章节来源**
- [appsettings.json:14-14](file://SpeedRunners.API/SpeedRunners/appsettings.json#L14-L14)
- [SteamBLL.cs:146-158](file://SpeedRunners.API/SpeedRunners.BLL/SteamBLL.cs#L146-L158)
- [SteamBLL.cs:289-306](file://SpeedRunners.API/SpeedRunners.BLL/SteamBLL.cs#L289-L306)
- [AchievementSchemaService.cs:36-48](file://SpeedRunners.API/SpeedRunners.BLL/AchievementSchemaService.cs#L36-L48)
- [ProfileBLL.cs:139-160](file://SpeedRunners.API/SpeedRunners.BLL/ProfileBLL.cs#L139-L160)
- [HttpRequestBase.cs:11-18](file://SpeedRunners.Scheduler/HttpRequestBase.cs#L11-L18)
- [SRLabTokenAuthMidd.cs:68-101](file://SpeedRunners.API/SpeedRunners/Middleware/SRLabTokenAuthMidd.cs#L68-L101)
- [UserBLL.cs:95-120](file://SpeedRunners.API/SpeedRunners.BLL/UserBLL.cs#L95-L120)
- [MUser.cs:1-35](file://SpeedRunners.API/SpeedRunners.Model/MUser.cs#L1-L35)

## 结论
该模块通过清晰的分层设计与稳健的错误处理，实现了对 Steam Web API 的稳定集成。**新增的AchievementSchemaService缓存服务显著提升了成就数据获取的性能和稳定性，通过内存缓存避免了频繁调用Steam API**。Profile模块实现了Steam成就与本地成就定义的关联处理，增强了错误处理和回退机制。业务层封装了多种查询路径与数据转换逻辑，前端与后台任务分别满足实时查询与批量同步需求。建议在生产环境中补充更完善的限流、缓存与可观测性策略，以进一步提升可靠性与性能。

## 附录：API 接口规范

### 基础信息
- 基础路径：/api/steam/{action} 和 /api/profile/{action}
- 认证：需要 Token（后端中间件校验）
- 语言：通过请求头 locale 切换游戏统计字段中文名映射

### 接口一览
- **SteamController 接口**
  - 搜索玩家
    - 方法：GET
    - 路径：/api/steam/SearchPlayer/{keyWords}
    - 参数：keyWords（字符串）
    - 返回：MSearchPlayerResult
  - 获取玩家列表（社区）
    - 方法：GET
    - 路径：/api/steam/GetPlayerList/{userName}/{sessionID}/{pageNo}
    - 参数：userName（字符串）、sessionID（字符串）、pageNo（整数）
    - 返回：MSearchPlayerResult（包含玩家列表）
  - 通过 CustomURL 查询
    - 方法：GET
    - 路径：/api/steam/SearchPlayerByUrl/{url}
    - 参数：url（字符串）
    - 返回：MSearchPlayerResult（若命中游戏统计则 IsGameInfo=true）
  - 通过 SteamID64 查询
    - 方法：GET
    - 路径：/api/steam/SearchPlayerBySteamID64/{steamID64}
    - 参数：steamID64（字符串）
    - 返回：MSearchPlayerResult（IsGameInfo=true）
  - 获取在线人数
    - 方法：GET
    - 路径：/api/steam/GetOnlineCount
    - 返回：uint（当前在线人数）

- **ProfileController 接口**
  - 获取个人主页数据
    - 方法：GET
    - 路径：/api/profile/GetData/{steamId}
    - 参数：steamId（字符串）
    - 返回：MProfileData
  - 获取每日天梯分历史记录
    - 方法：GET
    - 路径：/api/profile/GetDailyScoreHistory/{steamId}
    - 参数：steamId（字符串）
    - 返回：List<MDailyScore>
  - **新增**：获取玩家成就
    - 方法：GET
    - 路径：/api/profile/GetAchievements/{steamId}
    - 参数：steamId（字符串）
    - 返回：List<MAchievement>

### 响应数据结构
- MSearchPlayerResult
  - 字段：IsGameInfo（布尔）、GameInfo（JSON 游戏统计）、Total（整数）、PageNo（整数）、SessionID（字符串）、PlayerList（列表）
- PlayerInfo
  - 字段：Avatar（字符串）、UserName（字符串）、ProfilesOrID（字符串，值为 profiles 或 ID）、ContentOfID（字符串，CustomURL 或 SteamID64）
- MSteamAchievement
  - 字段：ApiName（字符串）、Achieved（整数，0或1）、UnlockTime（DateTime，可为空）
- **新增**：MAchievementSchema
  - 字段：ApiName（字符串）、DisplayName（字符串）、Description（字符串）、Icon（字符串）、IconGray（字符串）、Hidden（布尔）
- MAchievement
  - 字段：ApiName（字符串）、Name（字符串）、Description（字符串）、IconUrl（字符串）、IconGrayUrl（字符串）、Unlocked（布尔）、UnlockedAt（DateTime，可为空）、Hidden（布尔）

### 异常与错误处理
- HTTP 请求异常：业务层捕获并返回空，避免中断
- 社区搜索 success 校验：非 1 时返回空
- 天梯分查询：仅当返回包含 score 字段时解析
- **新增**：Steam API调用失败时返回null，让上层逻辑进行优雅降级
- **新增**：AchievementSchemaService缓存获取失败时返回空列表
- **新增**：ProfileBLL成就获取异常时捕获，但不中断整个流程
- 认证失败：中间件返回未授权

### 调用示例（前端）
- 搜索玩家：调用 steam.js 的 searchPlayer(keyWords)
- 获取玩家列表：调用 getPlayerList(userName, sessionID, pageNo)
- 通过 URL 查询：调用 searchPlayerByUrl(url)
- 通过 SteamID64 查询：调用 searchPlayerBySteamID64(steamID64)
- 获取在线人数：调用 getOnlineCount()
- **新增**：获取个人主页数据：调用 profile.js 的 getProfileData(steamId)
- **新增**：获取每日天梯分历史：调用 profile.js 的 getDailyScoreHistory(steamId)
- **新增**：获取玩家成就：调用 profile.js 的 getAchievements(steamId)

**章节来源**
- [SteamController.cs:12-26](file://SpeedRunners.API/SpeedRunners/Controllers/SteamController.cs#L12-L26)
- [ProfileController.cs:19-39](file://SpeedRunners.API/SpeedRunners/Controllers/ProfileController.cs#L19-L39)
- [MSearchPlayerResult.cs:6-36](file://SpeedRunners.API/SpeedRunners.Model/Steam/MSearchPlayerResult.cs#L6-L36)
- [MSteamAchievement.cs:8-13](file://SpeedRunners.API/SpeedRunners.Model/Steam/MSteamAchievement.cs#L8-L13)
- [MAchievementSchema.cs:8-41](file://SpeedRunners.API/SpeedRunners.Model/Steam/MAchievementSchema.cs#L8-L41)
- [MAchievement.cs:8-51](file://SpeedRunners.API/SpeedRunners.Model/Profile/MAchievement.cs#L8-L51)
- [steam.js:3-36](file://SpeedRunners.UI/src/api/steam.js#L3-L36)
- [profile.js:4-25](file://SpeedRunners.UI/src/api/profile.js#L4-L25)
- [index.vue:104-155](file://SpeedRunners.UI/src/views/searchPlayer/index.vue#L104-L155)
- [index.vue:334-364](file://SpeedRunners.UI/src/views/profile/index.vue#L334-L364)