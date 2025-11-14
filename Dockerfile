FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

COPY . .

RUN dotnet restore

RUN chmod +x bash.sh 

EXPOSE 5001

ENTRYPOINT ["./bash.sh"]