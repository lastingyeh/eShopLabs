# Heads to [**eShopOnContainers**](https://github.com/dotnet-architecture/eShopOnContainers) Microservices 

## Setup

### __Docker Environment__ [__dev__]
- Install [Docker desktop](https://www.docker.com/products/docker-desktop)
- Install [WSL2](https://www.youtube.com/watch?v=BEVcW4kz1Kg&list=PLfQqWeOCIH4ACS0037k1KLNIv5f646jbr&index=1)
- Install [Windows terminal](https://docs.microsoft.com/en-us/windows/terminal/get-started)
- open terminal
```
# /src
# pull & build image
$ docker-compose build

# run 
$ docker-compose up -d

# check
$ docker-compose ps

# remove
$ docker-compose down
```
### [portainer monitor](https://localhost:9443/#!/2/docker/containers)
* Container status & logs & resources usage
   
  ![alt tag](https://github.com/lastingyeh/eShopLabs/blob/master/imgs/portainer.jpg)

---
## Tasks (updating...)
### [Catalog services](https://github.com/lastingyeh/eShopLabs/tree/master/src/Services/Catalog)

- [x] Catalog.API [_dev_]
- [x] Catalog.UnitTests
- [x] Catalog.FunctionalTests

### [Identity services](https://github.com/lastingyeh/eShopLabs/tree/master/src/Services/Identity)

- [x] Identity.API [_dev_]

### [Basket services](https://github.com/lastingyeh/eShopLabs/tree/master/src/Services/Basket)

- [x] Basket.API [_dev_]

### [Building Blocks](https://github.com/lastingyeh/eShopLabs/tree/master/src/BuildingBlocks)

- [x] EventBus
  - [x] EventBus
  - [x] EventRabbitMQ
  - [x] EventBusServiceBus
  - [x] IntegrationEventLogEF
- [x] Utils
  - [x] Linq
- [x] WebHostCustomization
  - [x] WebHost.Customization

---
## References

- [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers)
- [eShopOnContainers realease 3.1](https://github.com/dotnet-architecture/eShopOnContainers/releases)
- [eShopOnContainers wiki](https://github.com/dotnet-architecture/eShopOnContainers/wiki)
- [Docker_dotnet](https://github.com/dotnet/dotnet-docker/issues/2375)
- [Polly](https://github.com/App-vNext/Polly)
- [Serilog](https://github.com/serilog/serilog)
- [Azure.Identity](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme)
- [Windows10开发环境搭建](https://www.youtube.com/playlist?list=PLfQqWeOCIH4ACS0037k1KLNIv5f646jbr)
- [使用 WSL 2 打造優質的多重 Linux 開發環境](https://blog.miniasp.com/post/2020/07/26/Multiple-Linux-Dev-Environment-build-on-WSL-2)
