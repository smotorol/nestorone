# API 명세

이 프로젝트의 API 범위는 일부러 좁게 잡았다. 개인 프로젝트 규모를 유지하면서도 서버 구조와 Oracle 연동 방식을 설명할 수 있게 하기 위함이다.

## 공통 응답 형식

```json
{
  "success": true,
  "code": "SUCCESS",
  "message": "Request completed successfully.",
  "data": {},
  "traceId": "00-7fa3...-01"
}
```

실패 예시:

```json
{
  "success": false,
  "code": "ERR_STOCK_SHORTAGE",
  "message": "재고가 부족합니다.",
  "data": null,
  "traceId": "00-7fa3...-01"
}
```

## 1. GET /api/products

상품 목록 조회.

쿼리 파라미터:

- `keyword` 선택
- `categoryCode` 선택

## 2. GET /api/products/{productId}

상품 상세와 현재 재고 조회.

## 3. GET /api/stocks/{productId}

PL/SQL `pkg_order.get_stock` 호출.

## 4. POST /api/orders

주문 생성.

```json
{
  "customerName": "홍길동",
  "createdBy": "winforms-user",
  "items": [
    {
      "productId": 1001,
      "orderQty": 2
    },
    {
      "productId": 1002,
      "orderQty": 1
    }
  ]
}
```

## 5. GET /api/orders/{orderId}

주문 헤더와 상세 조회.

## 6. GET /api/orders?fromDate=2026-04-01&toDate=2026-04-30

주문일자 범위 조회.

## 7. POST /api/orders/{orderId}/cancel

```json
{
  "cancelReason": "고객 요청",
  "updatedBy": "admin-user"
}
```

실패 코드:

- `ERR_ORDER_NOT_FOUND`
- `ERR_ALREADY_CANCELLED`
