FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS restore
WORKDIR /src
COPY ./Billing.sln ./
COPY Billing.API/Billing.API.csproj ./Billing.API/
COPY Billing.API.Test/Billing.API.Test.csproj ./Billing.API.Test/
COPY ./.config/dotnet-tools.json ./.config/
RUN dotnet tool restore
RUN dotnet restore

FROM restore AS build
COPY . .
RUN dotnet dotnet-format --check
RUN dotnet build -c Release

FROM build AS test
RUN dotnet test

FROM build AS publish
RUN dotnet publish "Billing.API/Billing.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
COPY ./libadonetHDB.dll .
ARG version=unknown
RUN echo $version > /app/wwwroot/version.txt
ENTRYPOINT ["dotnet", "Billing.API.dll"]
