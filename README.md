# Microservices Learning On [eShopOnContainers]

## Technical targets

- Net Core 3.1 LTS
- Docker 
- Docker-compose
- Kubernetes [AKS]
- Azure Key Vault
---
## [Services]

### **Catalog**
---
#### Catalog.API
* CatalogController
    - ItemsAsync
    - ItemByIdAsync
    - ItemsWithNameAsync
    - ItemsByTypeIdAndBrandIdAsync
    - ItemsByBrandIdAsync
    - CatalogTypesAsync
    - CatalogBrandsAsync
    - UpdateProductAsync
    - CreateProductAsync
    - DeleteProductAsync
* PicController
    - GetImageAsync 

## Dependencies
---

- [Polly - express policies such as Retry, Circuit Breaker, Timeout, Bulkhead Isolation, and Fallback in a fluent and thread-safe manner.](https://github.com/App-vNext/Polly)
- [Serilog](https://github.com/serilog/serilog)
- [Azure.Identity](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme)

# References

- [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers)
- [eShopOnContainers realease 3.1](https://github.com/dotnet-architecture/eShopOnContainers/releases)
