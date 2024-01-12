FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
ENV TZ=Asia/Shanghai
RUN sed -i "s/TLSv1.2/TLSv1/g" /etc/ssl/openssl.cnf
COPY . ./
ENTRYPOINT ["dotnet", "xLiAd.DiagnosticLogCenter.CollectServerProxy.dll"]