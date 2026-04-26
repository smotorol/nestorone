# OrderInventory.Infrastructure 구조 메모

Oracle 접근, Repository 구현, Dapper/ADO.NET 코드는 Infrastructure 계층에 격리한다. API와 Application이 Oracle 세부 구현을 몰라도 되게 만드는 것이 목적이다.

## 권장 폴더 구조

```text
Persistence/
Repositories/
Procedures/
Configurations/
```

## 핵심 책임

- `Persistence/OracleConnectionFactory.cs`
- `Repositories/ProductRepository.cs`
- `Repositories/OrderRepository.cs`
- `Procedures/OrderProcedureExecutor.cs`
- `Configurations/OracleOptions.cs`
