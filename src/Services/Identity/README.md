# Identity services

## Https

- Create & trust cert file

  ```
  # create pfx file
  $ dotnet dev-certs https -ep ${HOME}/.aspnet/https/aspnetapp.pfx -p pa55w0rd!

  $ dotnet dev-crets https --trust
  ```

- Docker-compose configuration
  ```yml
  identity-api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=https://+:443;http://+:80
      - ASPNETCORE_HTTPS_PORT=8081
      - ASPNETCORE_Kestrel__Certificates__Default__Password=pa55w0rd!
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
    ports:
      - '8081:443'
      - '8080:80'
    volumes:
      - ~/.aspnet/https:/https:ro
  ```

## References
- [User-Secrets](https://docs.microsoft.com/zh-tw/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=linux)
- [Docker Https](https://docs.microsoft.com/en-us/aspnet/core/security/docker-compose-https?view=aspnetcore-3.1)
- [Cookie SSO Protected](https://codingnote.cc/zh-tw/p/252256/)
- [UserSecurityStamp](https://stackoverflow.com/questions/19487322/what-is-asp-net-identitys-iusersecuritystampstoretuser-interface?rq=1)
