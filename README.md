# ECommerceMS — E-Commerce Microservices Platform
A distributed e-commerce platform built with microservices architecture.

## Tech Stack
ASP.NET Core 8 · RabbitMQ · Redis · YARP API Gateway · SignalR · React · TypeScript · Docker Compose

## Services
| Service | Port | Description |
|---|---|---|
| API Gateway | 5000 | Single entry point, routing |
| Product Service | 5001 | Product catalog, search, inventory |
| Order Service | 5002 | Cart, checkout, order management |
| Payment Service | 5003 | Payment processing |
| Notification Service | 5004 | Real-time notifications via SignalR |

## Run Locally
```bash
docker compose up
```

## Status
🚧 In Progress
