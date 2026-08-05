# Codex Pocket Console

在 iPhone Safari 中查看本机 Codex 历史项目与会话，并创建、启动和中断独立任务的私有 Web 控制台。

## 已实现

- Vue 3 + TypeScript + Vite 移动端 PWA
- ASP.NET Core 网关和 Cookie 登录
- SignalR 实时任务与 Codex 事件同步
- SQLite `Tasks` 表，任务与 Codex thread 分离
- 读取 Codex App Server 的真实项目、历史会话和会话详情
- 创建任务，调用 `thread/start`、`turn/start` 和 `turn/interrupt`
- 工作区白名单、登录限流、API 与 SignalR 鉴权
- 通过 Tailscale Serve 提供 tailnet 内私有 HTTPS 访问

## 架构

```text
iPhone Safari / PWA
        │ Tailscale HTTPS
        ▼
ASP.NET Core + SignalR
        │ stdio JSON-RPC
        ▼
Codex App Server
        │
        ├─ 本机项目目录
        ├─ Codex 历史会话
        └─ SQLite Tasks
```

## 本机启动

环境要求：Windows、.NET 9、Node.js、npm、已登录的 Codex CLI。

首次构建并启动：

```powershell
.\scripts\Start-PocketConsole.ps1 -Build
```

后续启动：

```powershell
.\scripts\Start-PocketConsole.ps1
```

默认地址为 `http://127.0.0.1:5086`。脚本首次启动会生成随机访问密码并保存在 `.runtime/access-password.txt`。

默认允许访问当前仓库的父目录。可显式限制工作区：

```powershell
.\scripts\Start-PocketConsole.ps1 -WorkspaceRoots @(
    "D:\Projects\codex-workspaces"
)
```

停止服务：

```powershell
.\scripts\Stop-PocketConsole.ps1
```

## iPhone 远程访问

1. 在 Windows 与 iPhone 安装 Tailscale。
2. 两端登录同一个 Tailscale 网络（tailnet）。
3. 保持 Pocket Console 在本机 `5086` 端口运行。
4. 在项目根目录执行：

```powershell
.\scripts\Enable-Tailscale.ps1
```

5. 若脚本提示需要一次性启用 Serve，请打开它输出的 Tailscale 管理地址完成授权，然后重新执行脚本。
6. 使用命令输出的 `https://...ts.net` 地址在 iPhone Safari 打开。
7. 输入 `.runtime/access-password.txt` 中的密码。
8. 在 Safari 分享菜单中选择“添加到主屏幕”。

若暂时无法启用 HTTPS，可先使用仅 tailnet 内可见的 HTTP 模式验证连接：

```powershell
.\scripts\Enable-Tailscale.ps1 -Http
```

HTTP 模式适合临时访问；安装 PWA 和长期使用仍建议启用 HTTPS。

关闭 Tailscale Serve：

```powershell
.\scripts\Disable-Tailscale.ps1
```

## 开发

后端：

```powershell
dotnet build PocketConsole.sln
dotnet run --project src\PocketConsole.Api --urls http://127.0.0.1:5086
```

前端：

```powershell
Set-Location src\PocketConsole.Web
npm install
npm run dev
```

Vite 开发服务器会把 `/api` 和 `/hubs` 代理到 `127.0.0.1:5086`。生产构建会写入 API 项目的 `wwwroot`。

## 安全说明

- 不要把 `5086` 端口直接映射到公网。
- 不要分享 `.runtime/access-password.txt`。
- REST API 和 SignalR Hub 均要求登录。
- 可执行任务只能使用 `Security:WorkspaceRoots` 白名单内的目录。
- 当前 Codex 使用 `workspace-write` 与 `never` 审批策略，因为手机审批界面尚未实现。
- 下一阶段应增加手机审批、设备撤销和完整审计界面，再切换到 `on-request`。

详细设计与后续路线见 `docs/PROJECT_PLAN.md`。
