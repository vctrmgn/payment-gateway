# Payment Gateway

This is a Payment Gateway API developed in C# with [ASP.NET](http://ASP.NET) Core 6.


## Available Features

### Idempotent Payments Processing

With the Payment Gateway it's currently possible for Merchants to process Credit Card payments in order to collect money from their clients.

Based on an `Idempotency-Key` sent with each payment request, the system is able to uniquely process the payments, even though multiple requests arrive at the same time or in the following 24h.

Below, there is an activity diagram describing the processing of a payment:

--- Image Here

### Single Payment Details

The Payment Gateway is able provide details of a single payment by receiving a Payment Id, that were previously informed to a Merchant after a complete payment processing request.

Credit Card sensitive data are not stored as plain text in Payment Gateway's database. Before trying to process a payment, Credit Card data is exchanged by a `single usage token` and the source data cant then be masked and stored.

For that reason, all payment details requests return Credit Card sensitive data masked.


### Paginated Payments Listing

In order to provide the Merchant with an easy way to collect payment data from the Gateway, a paginated listing feature were made available.

Merchants can request a list of payments by informing a criteria object, where they should specify, `pagination parameters`, as well as the `period` to be searched. A `source reference` parameter can also be provided optionally.

As it works for Single Payments Details, sensitive Credit Card data is always masked.


### Data Segregation

A Merchant can have multiple `API Secret Keys` associated to them, each one having a set of `Roles` associated to it. 

By calling the API endpoints with valid and authorized keys, Merchants will be automatically identified and operations will be performed over their own data only.


## Solution Design

The solution is designed using a Clean Architecture approach, where all domain dependencies flow from the Core component.

--- Image Here


### Core

Contains the application's domain logic and provides the interfaces that should be implemented by its dependents when interacting with it.

- DTOs for domain layer input and output
- Validators (with Fluent Validation)
- Domain Entities
- Service interfaces and their implementation
- Repository interfaces (being implemented by Infrastructure)
- Adapter interfaces (being implemented by Infrastructure)

### Shared Kernel

Contain common resources that are not domain specific only. Simplifies sharing, avoiding the dependencies with Core when not necessary.

- Enums
- Exceptions
- Extensions Methods
- Data Structures

### Infrastructure

Implements the communication with the outside world and provides solutions for the application to run securely.

- Adapters implementation (Bank and Distributed Lock)
- Repositories implementation
- Banking Client Interfaces (being implemented by Bank Simulator)
- Data Access with EF Core
- Migrations
- Identity Management (custom implementation)

### Web

This is the application's entry point, providing API features and defining how the solution behaves by setting up the dependency injection container.

- Database initialization
- Services registration (IoC)
- Controllers
- Exception handling filters
- API models
- Auto Mapping
- API Key Authentication Handler
- Authorization

### Bank Simulator

Implements the interfaces provided by Infrastructure, simulating the basic behavior of an actual Bank API. 

### Tests

Includes Unit and Integration tests. Defines a Testing Service Provider to make use of dependency injection, easily setting tests up.


## How to Run the Payment Gateway

Run the application by using one of the methods below. In both cases, the API will be available at:

`http://localhost:8080`

The API documentation and testing interface can found at:

`http://localhost:8080/swagger`


### With Docker

Run the bash script `startup.sh` located in the root directory to `build`, `run unit/integration tests` and `start the application`:

```bash
sh startup.sh
```

### With dotnet cli

Navigate to the application's entry point directory and run the dotnet cli command:

```bash
cd src/PaymentGateway.Web
dotnet run
```

To run unit/integration tests, do:

```bash
cd src/
dotnet test
```

## How to Test the Payment Gateway

The Payment Gateway can be tested by using Swagger UI, where the API documentation is available together with an API client.

When the application starts, the existing migrations are applied and a set of testing data is seeded to the database (the database is always recreated and seeded at application restart). 

### Merchant Testing Data

Two Merchants will be available at start. Below, they are described along with their `API Secret Keys` and their `Roles`:

| Merchant Name | Secret Key | Roles |
| --- | --- | --- |
| NerdStore | nerdstore_secret_example_1234 | ReadPayments, ProcessPayments |
| NerdStore | nerdstore_secret_example_5678 | ReadPayments |
| MagnoStore | magnostore_secret_example_1234 | ReadPayments, ProcessPayments |

The API Secret Key should be send as an Bearer Authorization header, like:

```
Bearer nerdstore_secret_example_1234
```

Using Swagger UI, click on the `Authorization` button at the right upper corner and then set the header as:

--- Image Here


### Credit Card Testing Data

Below, you can find a set of Credit Card numbers that will always be processed as `accepted` or `verified` (if amount is 0). Any other valid Credit Card number will have their payments `declined`. The other information (holder namer, year, month, cvv) just need to be valid.

```
1111222233334444
5186001700008785
5186001700009726
5186001700009908
5186001700008876
5186001700001434
4012888888881881
371449635398431
38520000023237
```

### Examples

### Payment Processing

Request:

```bash
curl -X 'POST' \
  'http://localhost:8080/api/v1/payments' \
  -H 'accept: application/json' \
  -H 'Idempotency-Key: unique-key-1234' \
  -H 'Authorization: bearer nerdstore_secret_example_1234' \
  -H 'Content-Type: application/json' \
  -d '{
  "sourceReference": "order-1234",
  "amount": 100,
  "currency": "BRL",
  "cardInfo": {
    "holderName": "John Doe",
    "number": "1111222233334444",
    "expiryYear": 2029,
    "expiryMonth": 1,
    "cvv": "828"
  }
}'
```

Response:

```json
{
  "paymentId": "ac4ce2e0-a83d-4ea6-a713-789cde2879a5",
  "paymentStatus": "Authorized"
}
```

### Payment Details

Request:

```bash
curl -X 'GET' \
  'http://localhost:8080/api/v1/payments/ac4ce2e0-a83d-4ea6-a713-789cde2879a5' \
  -H 'accept: application/json' \
  -H 'Authorization: bearer nerdstore_secret_example_1234'
```

Response:

```json
{
  "paymentId": "ac4ce2e0-a83d-4ea6-a713-789cde2879a5",
  "sourceReference": "order-1234",
  "amount": 100,
  "currency": "BRL",
  "cardHolderName": "J******e",
  "cardNumber": "1111********4444",
  "cardExpiryYear": 2029,
  "cardExpiryMonth": 1,
  "merchantName": "NerdStore",
  "paymentStatus": "Authorized",
  "creationDate": "2022-12-05T19:58:15.9154421",
  "lastUpdate": "2022-12-05T19:58:15.9154541"
}
```

### Paginated Payments Listing

Request

```bash
curl -X 'GET' \
  'http://localhost:8080/api/v1/payments?Limit=100&Skip=10&StartDate=2022-11-01&EndDateInclusive=2022-12-01' \
  -H 'accept: application/json' \
  -H 'Authorization: bearer nerdstore_secret_example_1234'
```

Response

```json
{
  "limit": 100,
  "skip": 10,
  "totalItems": 251,
  "pageItems": [
    {
      "paymentId": "413025f2-330f-4398-ac0b-2b28b2e83166",
      "sourceReference": "d0b12e4b-c759-4d4d-8c15-17ee8d5dd7b1",
      "amount": 8922456,
      "currency": "BRL",
      "cardHolderName": "B******o",
      "cardNumber": "5186********1434",
      "cardExpiryYear": 2023,
      "cardExpiryMonth": 12,
      "merchantName": "NerdStore",
      "paymentStatus": "Authorized",
      "creationDate": "2022-11-29T19:55:25.3874262",
      "lastUpdate": "2022-12-05T19:55:25.3874263"
    },
    ...
  ]
}
```

### Considerations

This is a simple and time boxed implementation of a Payment Gateway.  Some aspects, such as encryption (including HTTPS), secrets usage, database choice, etc, were simplified.

### Next Steps

Payments that fail after data persistence confirmation are returned to API callees as `Unknown` with status `202 Accepted`. In future developments a sub system based on `events` (of failure), a `worker` (to reprocess failed payments) and a `notification` service (with web hooks to merchants) should be created to reprocess them without returning such status to Merchants (today they can retry the failed payments and complete the processing). In a future implementation, a `Pending` status should be returned and the reprocessing performed by the Payment Gateway itself.