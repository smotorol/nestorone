# OrderInventory.Client.WinForms 구조 메모

현재 WinForms 클라이언트는 복잡한 운영 화면보다, API 연동과 주문 흐름 검증에 초점을 둔 단순 테스트 클라이언트다.

## 현재 폴더 구조

```text
Forms/
Services/
Models/
Program.cs
```

## 현재 핵심 화면

- `Forms/MainForm.cs`
  - 상품 조회
  - 상품 선택 후 주문 생성
  - 결과 메시지 표시

## 핵심 책임

- `Services/ApiClient.cs`: API 호출 래퍼
- `Models/*`: 상품 조회/주문 요청/응답 모델
- `Forms/MainForm.cs`: 간단한 사용자 입력과 결과 표시
