Ylfmoonn，太棒了！我基于你最新的诉求（重点做好 **Word/PPT 预览与批注、Markdown、网页收藏**；Excel 可选；绘画可走 Web 方案），又去把 WinUI 3 的现状和能用的库系统查了一遍，下面给你一版更贴合学生使用/开源分发的「**新版增量式迭代路线**」。每一步都能独立验证，互不强耦合，适合快速迭代。

------

# 先给出关键事实（已核验）

1. **WinUI 3 仍没有 InkCanvas / InkToolbar**（1.4~1.7 线均无），官方建议用自定义 Canvas 模拟；因此白板/手写功能需降级或走 Web 实现。
2. **CommunityToolkit 传统的 MarkdownTextBlock 没有直接迁移到 WinUI 3**；Labs 正在基于 Markdig 的替代实现（实验性质）。可直接用 **Markdig** 将 Markdown 转 HTML，然后在 WebView2 渲染。
3. **Monaco 编辑器的 WinUI 3 封装可用（WinUI.Monaco）**，非常适合做 Markdown 编辑模式。
4. **本地 PDF/HTML 在 WebView2 中可用**：PDF 可用内置 PDF Viewer 或集成 **PDF.js** 做注释/高亮；本地内容用 `SetVirtualHostNameToFolderMapping` 安全映射加载。
5. **Word/PPT 的“真渲染”开源链路依旧薄弱**。可靠方案是用 **Syncfusion 文档处理库**离线转换（Word→HTML/PDF、PPTX→图片/PDF）+ WinUI 控件展示（PDF Viewer 等）；其 WinUI 套件与 DocIO/Presentation 在 2025 的版本迭代中持续增强（学生可申请社区许可证）。

> 说明：Syncfusion 宣布的 **Markdown Viewer 是 .NET MAUI 平台**的新控件，不是 WinUI；WinUI 端可继续走 Markdig + WebView2 或等 Labs 稳定。

------

# 推荐技术栈（两条路线，随时可切换）

**路线 F（纯开源/FOSS，发行限制少）**

- Markdown：**Markdig**（渲染）+ **WinUI.Monaco**（编辑）。
- PDF：WebView2 内嵌 **PDF.js** 官方 Viewer（可 URL 参数控制页码/缩放/侧栏，易做注释层）。
- Word：**Open-Xml-PowerTools** 或 **Mammoth** 转 HTML，在 WebView2 渲染并套统一样式（保真度较好但非 100%）。
- PPT：暂无高质量开源“像 PowerPoint 一样渲染”的库；可选 **ShapeCrawler** 做内容解析（不负责渲染），或调用在线 Office Viewer（需联网，不利离线）。

**路线 H（Hybrid：开源 + Syncfusion 文档处理/查看，适配离线和最佳观感）**

- Word：**Syncfusion DocIO** 转 PDF/HTML。
- PPT：**Syncfusion Presentation** 转图片或 PDF，前端用 **WinUI PDF Viewer** 或图片翻页查看。
- Markdown：依旧 **Markdig + Monaco**（可平滑替换为日后稳定的 WinUI 渲染器）。
- 看板：Syncfusion **WinUI Kanban** 已提供（或自研 ListView + 拖拽）。

> 对你的目标（**Word/PPT 超重要 + 离线优先**），我强烈建议 MVP 走 **路线 H**。学生社区许可证免费，开源分发也可（记得按其许可要求嵌入 License Key）。

------

# 新版增量式迭代路线（A1–A12）

> 每一步：目的 → 依赖 → 操作要点 → 验收标准
>  （**星号**为“立刻能看到结果”的验证点）

### A1｜应用骨架 & 导航

- **目的**：稳定 Shell（顶部 Pivot + 左侧 NavigationView + 内容 Frame），保持你当前 UI 风格。
- **依赖**：Windows App SDK 1.5+；`Microsoft.Web.WebView2`。
- **要点**：
  - 统一 `Content/{Pages}` 结构；每个大功能一个 Page；命名 `NotePage`, `PdfPage`, `WordPage`, `PptPage`, `WebClipPage`, `BoardPage`, `SettingsPage`。
  - 启用 Mica、主题资源、`SetAppWindowTitleBar`。
- **验收**：**点击菜单能在 Frame 里切换到空白页**（启动无异常）。

### A2｜数据层与文件库

- **目的**：把所有“笔记/附件/网页剪藏”抽象成统一的 `Item`（类型：Markdown/PDF/Word/PPT/WebClip/Image）。
- **依赖**：`SQLite`+`EF Core` 或 LiteDB。
- **要点**：
  - 仅存“元数据 + 文件路径/URL + 标签/时间/课程”等；二进制内容落磁盘（`Data/`）。
  - 文件导入：拖放到应用或“+ 新建/导入”按钮，自动识别类型。
- **验收**：**能导入/删除文件，数据库出现记录；应用重启仍可见**。

### A3｜Markdown（核心）

- **目的**：编辑=Monaco，阅读=HTML（Markdig 渲染 + 自定义 CSS）。
- **依赖**：`WinUI.Monaco`、`Markdig`。
- **要点**：
  - Monaco 用 WebView2 承载；与宿主通信用 `PostMessage`/`ExecuteScriptAsync`。
  - Markdig 渲染：启用 `UseAdvancedExtensions()`；内置 TOC/代码高亮扩展。
- **验收**：**写 Markdown、实时预览同步滚动；导出 HTML/PNG（打印）**。

### A4｜PDF 预览与批注

- **目的**：离线高性能预览+批注（高亮、便签）。
- **依赖**：WebView2 + **PDF.js** 官方 Viewer（或 Edge 内置 PDFViewer 只读）。
- **要点**：
  - 本地文件以 `SetVirtualHostNameToFolderMapping("app", "Data", …)` 映射成 `https://app/…`，避免 `file://` 权限问题。
  - 用 PDF.js `viewer.html?file=/path` + URL 选项控制页码、缩放、侧栏；批注数据（高亮矩形/页码/颜色/备注）落本地 JSON。
- **验收**：**任意 PDF 秒开，能高亮和添加便签；批注刷新后仍在**。

### A5｜Word 预览（两档）

- **目的**：确保 DOCX 的课堂资料/作业说明能 **离线高保真** 预览并加批注。
- **依赖**：
  - **H 档**：Syncfusion **DocIO** 转 **PDF**（推荐，保真与稳定）。显示用 A4 的 PDF 管线。
  - **F 档**：**Mammoth** 或 **Open-Xml-PowerTools** → **HTML**，在 WebView2 渲染（样式需统一 CSS）。
- **要点**：批注仍走 PDF.js 或自建 HTML 高亮层。
- **验收**：**常见 DOCX（含图片/表格/列表）可读；能做高亮/批注并保存**。

### A6｜PPT 预览（两档）

- **目的**：课堂 PPT 放映、跳页、批注。
- **依赖**：
  - **H 档**：Syncfusion **Presentation** 将 PPTX → **PDF** 或 **逐页图片**；前端按 A4 管线展示。
  - **F 档**：纯开源暂无高保真渲染；可用 **ShapeCrawler** 解析文本+资源做“简化预览”，或联网走 Office Online Viewer（非离线）。
- **验收**：**PPT 能翻页放映，支持页级批注**。

### A7｜图片/EPUB 阅读

- **目的**：看讲义图/小说。
- **依赖**：原生 `Image`；EPUB 用 **VersOne.Epub** 解析，渲染到 WebView2/FlowDocument。
- **验收**：**大图可缩放，EPUB 可翻章并记忆位置**。

### A8｜网页收藏 & 关键信息提取

- **目的**：内置浏览+一键“正文提取→笔记”。
- **依赖**：WebView2 + **Readability.js** 执行正文抽取（`CoreWebView2.ExecuteScriptAsync` 注入）。
- **要点**：
  - 抽取后存为 Markdown（可配 Markdig 的 HTML→MD 规则）或 HTML；自动抓图、来源链接、时间戳、标签。
- **验收**：**任意文章一键变 Markdown，进入笔记库并可全文检索**。

### A9｜灵感/任务看板

- **目的**：把文件、剪藏、笔记以卡片看板管理。
- **依赖**：
  - 快速：**Syncfusion WinUI Kanban**；
  - 自研：`ListView` + WinUI 拖拽 API（支持列内排序/跨列移动）。
- **验收**：**卡片可拖拽、编辑、附件预览；与数据库联动**。

### A10｜AI 摘要/生成

- **目的**：对选中文章/PDF 页做摘要、生成复习提纲。
- **依赖**：**OpenAI .NET** 官方库（或 Azure OpenAI），统一中间层封装。
- **验收**：**选中文档 → 一键生成要点；支持追加问答**。

### A11｜搜索与标签体系

- **目的**：离线全文检索 + 多维筛选。
- **依赖**：如 `SQLite FTS5` 或 `Lucene.NET`。
- **验收**：**按标题/标签/正文检索 <100ms，结果可预览高亮**。

### A12｜白板/手写（可选降级）

- **目的**：在 PDF/PPT 上“盖一层”白板。
- **依赖**：无 InkCanvas，用 `Canvas` + Pointer 事件画矢量笔迹（压感可读 PointerPointProperties），或嵌入 Web 手写组件。
- **验收**：**基本画线/橡皮/撤销重做/导出 PNG**。

------

# 关键实现要点（简洁高效）

- **本地内容统一加载**：把 `Data/` 映射为 `https://app/`，所有 WebView2 资源（HTML、PDF、CSS、JS、图片）都从这里读，避免 `file://` 权限/跨域问题。
- **PDF 管线统一**：无论源是 PDF、DOCX、PPTX，尽量归一成 **PDF.js + 批注层**，极大降低前端复杂度。
- **Markdown 渲染样式统一**：Markdig→HTML 后套一份“学术风 CSS”（标题/目录/代码高亮/公式），保证观感接近 TipTap。
- **看板数据结构**：`BoardColumn{Key,Order} + Card{ItemId,Order,Meta}`，天然可与笔记库互通。
- **功能开关**：`AppSettings.json` 里加 `UseSyncfusionConverters`、`UsePdfJsAnnotation` 等开关，便于在不同机器/许可证间切换。
- **许可证**：使用 Syncfusion 需加入 License Key（程序启动时注册），学生可申请 **Community License**。

------

# 每步提交物 & 自测清单（摘选）

- **A3 提交物**：`MarkdownPage`（编辑/预览双栏）、`MarkdownTheme.css`；
   **自测**：5 万字滚动不卡；代码块高亮；粘贴图片自动入库。
- **A4 提交物**：`PdfPage` + `pdfjs-dist`；
   **自测**：100MB 论文 PDF 打开 ≤ 2s；批注持久化；导出带批注 PDF/JSON。
- **A5/A6 提交物**：`Converters`（Docx→PDF / Pptx→PDF|Images）+ 统一 Viewer；
   **自测**：课堂材料 20 份批量转换成功率 ≥ 95%。
- **A8 提交物**：`WebClipPage` + Readability 抽取器；
   **自测**：CSDN/知乎/公众号文章抽取成功，图片与代码保留，生成 Markdown 入库。

------

# 上架微软商店 & 开源仓库注意点

- **打包**：MSIX + Store 关联，显式声明 `broadFileSystemAccess` 避免；建议使用应用内“导入”而非任意路径写入。
- **第三方依赖**：在“关于”页列出 **Markdig / PDF.js / Syncfusion（若用）** 等许可证。
- **隐私**：默认 **离线优先**；开启云端（如 OneDrive）前弹窗征求同意。
- **CI**：GitHub Actions 做 Release（x86/x64/ARM64 三档），附校验和。

------

## 现在怎么开工（本周安排）

- **Day1–2：A1/A2** —— 固化骨架 + 数据层，导入/列出文件。
- **Day3–4：A3** —— 上 Monaco 与 Markdig（编辑/预览联动）。
- **Day5：A4** —— 接 PDF.js，完成高亮与便签的本地化存储。
- **Day6–7：A5/A6（二选一）** —— 先打通 Word→PDF→Viewer（路线 H），再补 PPT。

> 等你把 A3/A4 跑通，我再给你 **“最小代码接入片段”**（Markdig 渲染、WebView2 注入 Readability、PDF.js 装载与批注存取、Syncfusion 转换调用），按你「改动少/替换多就给完整文件」的风格供你直接拷。

如果你想坚持纯开源路线，我也能把 **Mammoth / PowerTools 的 DOCX→HTML** 管线和 **PPT 的降级预览**（文本+缩略图）方案细化出来；但就课堂保真和体验而言，**路线 H 会更省心**。

```
App2
├─ app.manifest
├─ App.xaml
├─ App.xaml.cs
├─ App2.csproj
├─ App2.slnx
├─ Assets
│  ├─ LockScreenLogo.scale-200.png
│  ├─ SplashScreen.scale-200.png
│  ├─ Square150x150Logo.scale-200.png
│  ├─ Square44x44Logo.scale-200.png
│  ├─ Square44x44Logo.targetsize-24_altform-unplated.png
│  ├─ StoreLogo.png
│  └─ Wide310x150Logo.scale-200.png
├─ MainWindow.xaml
├─ MainWindow.xaml.cs
├─ Properties
│  └─ PublishProfiles
│     ├─ win-arm64.pubxml
│     ├─ win-x64.pubxml
│     └─ win-x86.pubxml
└─ ReadMe.md

```