# Identity services

###  Https

#### Create https
``` 
# local certs (local)
$ dotnet dev-crets https --trust

# create certs *certs name have to the same with [dotnet run Identity.API.dll]
$ dotnet dev-certs https -ep ./Identity.API.pfx -p pa$$w0rd

# set certs to user-secrets
$ dotnet user-secrets set "Kestrel:Certificates:Development:Password" "pa$$w0rd"
```

#### Docker configurations
``` 
$ docker run \
    -p 8080:80 -p 8081:443 \
    -e ASPNETCORE_URLS="https://+;http://+" \
    -e ASPNETCORE_HTTPS_PORT=8081 \
    -e ASPNETCORE_ENVIRONMENT=Development \
    -v <usersecrets_path>:/root/.microsoft/usersecrets \
    -v <certs_path>:/root/.aspent/https/ \
    identity.api
```

#### [User-Secrets](https://docs.microsoft.com/zh-tw/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=linux)

