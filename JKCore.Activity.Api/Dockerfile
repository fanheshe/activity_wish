FROM expcat/aspcore-with-gdiplus:3.1.1
WORKDIR /app
COPY ./site .
RUN cp /usr/share/zoneinfo/Asia/Shanghai /etc/localtime

ENTRYPOINT ["dotnet", "JKCore.Activity.Api.dll"]