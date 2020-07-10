# DiagnosticLogCenter
基于 Diagnostic 实现的日志中心，现只支持 .NetCore 项目。

###特点：
1，自动记录请求信息、SQL语句、请求外部接口的操作，使用简单。
2，数据存储于MongoDB 数据库中，而不是文本文件中，便于查询。
3，数据按天分表，便于清理。
4，系统侵入性非常小，只需引用包、一行代码加入服务即可。
5，对于需要手动记录更详细的项目，提供了扩展。

###使用方法：
1，安装 mongodb，https://www.mongodb.com/ 下载并安装；如果有 docker 环境 执行以下命令即可：
docker run -p 27017:27017 -v /data/mongo/data:/data/db -d --restart=always mongo:3.2
您可以选择适合的版本和数据存放路径。
2，运行 CollectServer 项目，CollectServer 是日志收集器。下载本项目源码后，把 xLiAd.DiagnosticLogCenter.CollectServer
项目下的 appsettings.Production.json 中的 ConfigDbUrl 配置项改为您的 mongodb 地址。
发布项目，并在您的服务器上跑起；CollectServer 是 .NetCore 3.1 项目，可运行于 IIS、Linux、Docker 环境中。
3，在您的 AspNetCore 项目中使用 Nuget 安装 xLiAd.DiagnosticLogCenter.Agent 包。
4，在您的 AspNetCore 项目的 Startup 中的 ConfigServices 节中加入以下语句：
services.AddDiagnosticLog(x => x.CollectServerAddress = "192.168.1.22:8812");
您需要把上边语句的 192.168.1.22:8812 改为您的 CollectServer 实例的 IP 和 端口。
此语句还可以配置 项目在 CollectServer 中的名称、环境等。
此时，运行您的项目，即可实现请求日志的记录。 可参考 SampleAspNetCore 项目。
5，UserInterface 项目是用户查看日志的地方。发布，并运行即可。

###更多
1，如果您需要使用高级功能，您可以通过 Nuget 引用 xLiAd.DiagnosticLogCenter 包。
此包实现了手动日志、自动记录方法调用、DapperEx 日志等。
2，手动日志，引用上述程序包后，调用 DiagnosticLogCenter.AdditionLog 即可。
3，自动记录方法调用功能，需要您的项目集成 AspectCore 动态代理功能。
https://github.com/dotnetcore/AspectCore-Framework
集成后，为您的类或方法加上 AspectLog 特性标记，即可自动记录方法的调用、参数、返回值。
4，当不使用 SqlClient 时，不能自动记录Sql语句；比如当您使用 Mysql 等数据库时。
如果您使用了 DapperEx，那恰好能解决这个问题。
https://github.com/zl33842901/DapperEx