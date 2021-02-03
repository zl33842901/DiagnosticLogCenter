FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim
WORKDIR /app
COPY . ./
ENV TZ=Asia/Shanghai
ENTRYPOINT ["dotnet", "xLiAd.DiagnosticLogCenter.SampleAspNet5.dll"]