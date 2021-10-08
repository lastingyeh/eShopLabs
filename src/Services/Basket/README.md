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
- Update_Basket_Return_Add_Basket
- Delete_Basket_Return_Null

### Basket.UnitTests
- Get_Customer_Basket_Success
- Post_Customer_Basket_Success
- Doing_Checkout_Without_Basket_Should_Return_Bad_Request
- Doing_Checkout_With_Basket_Should_Publish_UserCheckoutAccepted_Integration_Event