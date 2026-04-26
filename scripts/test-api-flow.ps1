param(
    [string]$BaseUrl = 'http://localhost:5138',
    [Nullable[long]]$SuccessProductId = $null,
    [int]$SuccessOrderQty = 1,
    [Nullable[long]]$ShortageProductId = $null,
    [Nullable[int]]$ShortageOrderQty = $null,
    [string]$CustomerName = '테스트고객',
    [string]$CreatedBy = 'local-test',
    [string]$CancelReason = '테스트취소'
)

$ErrorActionPreference = 'Stop'

function Write-Step([string]$message) {
    Write-Host "`n==== $message ====" -ForegroundColor Cyan
}

function Show-ApiError($errorRecord) {
    Write-Host '[실패 응답]' -ForegroundColor Red

    $response = $errorRecord.Exception.Response
    if ($null -eq $response) {
        Write-Host $errorRecord.Exception.Message -ForegroundColor Red
        return
    }

    try {
        $stream = $response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        $body = $reader.ReadToEnd()
        if ($body) {
            Write-Host $body -ForegroundColor Yellow
        }
        else {
            Write-Host $errorRecord.Exception.Message -ForegroundColor Red
        }
    }
    catch {
        Write-Host $errorRecord.Exception.Message -ForegroundColor Red
    }
}

function Get-Products([string]$ApiBaseUrl) {
    $response = Invoke-RestMethod -Method Get -Uri "$ApiBaseUrl/api/products"
    if ($null -eq $response -or $null -eq $response.Data) {
        throw '상품 목록 응답이 비어 있습니다.'
    }

    return @($response.Data)
}

function Find-ProductById($products, [long]$productId) {
    return $products | Where-Object { [long]$_.ProductId -eq $productId } | Select-Object -First 1
}

function Find-AvailableProduct($products, [int]$minQty) {
    return $products |
        Where-Object { [int]$_.CurrentStockQty -ge $minQty } |
        Sort-Object CurrentStockQty -Descending |
        Select-Object -First 1
}

try {
    Write-Step '1. Health 확인'
    $health = Invoke-RestMethod -Method Get -Uri "$BaseUrl/health"
    $health | ConvertTo-Json -Depth 5

    Write-Step '2. Swagger 확인'
    $swagger = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/swagger/index.html"
    Write-Host ("Swagger HTTP Status: {0}" -f $swagger.StatusCode) -ForegroundColor Green

    Write-Step '3. 상품 목록 조회'
    $products = Get-Products -ApiBaseUrl $BaseUrl
    $products | ConvertTo-Json -Depth 5

    if ($SuccessProductId.HasValue) {
        $successProduct = Find-ProductById -products $products -productId $SuccessProductId.Value
        if ($null -eq $successProduct) {
            throw "지정한 성공 테스트 상품 ID($SuccessProductId)를 찾지 못했습니다."
        }
        if ([int]$successProduct.CurrentStockQty -lt $SuccessOrderQty) {
            throw "지정한 성공 테스트 상품 ID($SuccessProductId)의 재고가 부족합니다. 현재 재고=$($successProduct.CurrentStockQty), 요청 수량=$SuccessOrderQty"
        }
    }
    else {
        $successProduct = Find-AvailableProduct -products $products -minQty $SuccessOrderQty
        if ($null -eq $successProduct) {
            throw '주문 생성 성공 테스트에 사용할 재고 보유 상품이 없습니다.'
        }
    }

    Write-Host ("성공 테스트 상품 선택: productId={0}, productName={1}, stock={2}" -f $successProduct.ProductId, $successProduct.ProductName, $successProduct.CurrentStockQty) -ForegroundColor Green

    Write-Step '4. 주문 생성 성공 테스트'
    $createBody = @{
        customerName = $CustomerName
        createdBy = $CreatedBy
        items = @(
            @{
                productId = [long]$successProduct.ProductId
                orderQty = $SuccessOrderQty
            }
        )
    } | ConvertTo-Json -Depth 5

    $createResponse = Invoke-RestMethod `
        -Method Post `
        -Uri "$BaseUrl/api/orders" `
        -ContentType 'application/json' `
        -Body $createBody

    $createResponse | ConvertTo-Json -Depth 5

    $orderId = $createResponse.data.orderId
    if (-not $orderId) {
        throw '주문 생성 응답에서 orderId를 찾지 못했습니다.'
    }

    Write-Step '5. 주문 취소 성공 테스트'
    $cancelBody = @{
        cancelReason = $CancelReason
        updatedBy = $CreatedBy
    } | ConvertTo-Json

    $cancelResponse = Invoke-RestMethod `
        -Method Post `
        -Uri "$BaseUrl/api/orders/$orderId/cancel" `
        -ContentType 'application/json' `
        -Body $cancelBody

    $cancelResponse | ConvertTo-Json -Depth 5

    Write-Step '6. 재고 부족 실패 테스트'
    if ($ShortageProductId.HasValue) {
        $shortageProduct = Find-ProductById -products $products -productId $ShortageProductId.Value
        if ($null -eq $shortageProduct) {
            throw "지정한 재고 부족 테스트 상품 ID($ShortageProductId)를 찾지 못했습니다."
        }
    }
    else {
        $shortageProduct = $successProduct
    }

    $effectiveShortageQty = if ($ShortageOrderQty.HasValue) {
        $ShortageOrderQty.Value
    }
    else {
        [int]$shortageProduct.CurrentStockQty + 1
    }

    Write-Host ("재고 부족 테스트 상품 선택: productId={0}, productName={1}, stock={2}, requestQty={3}" -f $shortageProduct.ProductId, $shortageProduct.ProductName, $shortageProduct.CurrentStockQty, $effectiveShortageQty) -ForegroundColor Yellow

    $shortageBody = @{
        customerName = '재고부족테스트'
        createdBy = $CreatedBy
        items = @(
            @{
                productId = [long]$shortageProduct.ProductId
                orderQty = $effectiveShortageQty
            }
        )
    } | ConvertTo-Json -Depth 5

    try {
        $shortageResponse = Invoke-RestMethod `
            -Method Post `
            -Uri "$BaseUrl/api/orders" `
            -ContentType 'application/json' `
            -Body $shortageBody

        Write-Host '재고 부족 테스트가 실패하지 않았습니다. 응답을 확인하세요.' -ForegroundColor Yellow
        $shortageResponse | ConvertTo-Json -Depth 5
    }
    catch {
        Show-ApiError $_
    }

    Write-Step '테스트 완료'
    Write-Host '주문 생성 / 주문 취소 / 재고 부족 실패 시나리오를 모두 실행했습니다.' -ForegroundColor Green
}
catch {
    Write-Host "테스트 실행 중 오류가 발생했습니다: $($_.Exception.Message)" -ForegroundColor Red
    Show-ApiError $_
    exit 1
}

Write-Host ''
Read-Host 'Enter 키를 누르면 종료합니다'