# Basket services

## APIs

### [Swagger APIs](http://host.docker.internal:5103/swagger)

### BasketController [api/v1/basket]

- GetBasketByIdAsync
- UpdateBasketAsync
- CheckoutAsync
- DeleteBasketByIdAsync

## Database

- Redis

## Commons

- EventBus
- EventBusRabbitMQ   [dev]
- EventBusServiceBus [prod]

## Technics

- Net Core 3.1 LTS
- Docker
- Docker-compose [dev]
- Grpc [basket.proto]
  - GetBasketById
  - UpdateBasket
- RabbitMQ [dev]
- Health Check

## Testing
### Basket.FunctionalTests

### Basket.UnitTests