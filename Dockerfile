FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /App

COPY ./src ./
RUN dotnet restore
RUN dotnet test -v n
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /App
ENV ASPNETCORE_URLS=http://*:80
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "PaymentGateway.Web.dll"]