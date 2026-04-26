# OrderInventory.Application 구조 메모

Application 계층은 유스케이스 흐름을 조율하는 자리다. Controller와 Infrastructure가 서로 단순하게 유지되도록 서비스 인터페이스, DTO, 저장소 계약을 여기에 둔다.

## 권장 폴더 구조

```text
Interfaces/
Services/
Dtos/
Exceptions/
```

## 핵심 책임

- `Interfaces/IProductQueryService.cs`
- `Interfaces/IOrderCommandService.cs`
- `Services/OrderCommandService.cs`
- `Dtos/*`: 요청/응답 DTO
- `Exceptions/BusinessException.cs`: 재고 부족, 주문 없음 등 비즈니스 예외 표현
