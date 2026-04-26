# OrderInventory.Api 구조 메모

API 계층은 가능한 한 얇게 유지하고, 요청/응답 처리, 유효성 검증, 예외 표준화, 전송 계층 역할에 집중한다.

## 권장 폴더 구조

```text
Controllers/
Middlewares/
Extensions/
Contracts/
Program.cs
appsettings.json
```

## 핵심 책임

- `Controllers`: HTTP 엔드포인트 정의와 Application 서비스 호출
- `Middlewares/GlobalExceptionMiddleware.cs`: Oracle 예외와 비즈니스 예외를 공통 응답으로 변환
- `Contracts/Common/ApiResponse.cs`: 공통 응답 포맷
- `Extensions/ServiceCollectionExtensions.cs`: DI 등록
