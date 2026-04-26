# OrderInventory.Domain 구조 메모

Domain 계층은 과도하게 무겁게 만들지 않으면서도, 공통 도메인 계약과 엔티티를 둘 수 있는 확장 지점으로 남겨 둔다.

## 권장 폴더 구조

```text
Entities/
Enums/
ValueObjects/
```

## 핵심 책임

- `Entities/Product.cs`
- `Entities/Order.cs`
- `Entities/OrderItem.cs`
- `Enums/OrderStatus.cs`
- `Enums/StockHistoryType.cs`
