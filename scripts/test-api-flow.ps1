param(
    [string]$BaseUrl = 'http://localhost:8080',
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

function Get-ErrorBody($errorRecord) {
    $response = $errorRecord.Exception.Response
    if ($null -eq $response) {
        return $errorRecord.Exception.Message
    }

    try {
        $stream = $response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        return $reader.ReadToEnd()
    }
    catch {
        return $errorRecord.Exception.Message
    }
}

function Show-ApiError($errorRecord) {
    Write-Host '[실패 응답]' -ForegroundColor Red
    $body = Get-ErrorBody $errorRecord
    if ($body) {
        Write-Host $body -ForegroundColor Yellow
    }
}

function Assert-Equals([string]$label, $actual, $expected) {
    if ($actual -ne $expected) {
        throw "$label 값이 예상과 다릅니다. expected=$expected actual=$actual"
    }

    Write-Host ("[OK] {0}: {1}" -f $label, $actual) -ForegroundColor Green
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
    Assert-Equals -label 'health.status' -actual $health.status -expected 'Healthy'

    Write-Step '2. Swagger 확인'
    $swagger = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/swagger/index.html"
    Write-Host ("Swagger HTTP Status: {0}" -f $swagger.StatusCode) -ForegroundColor Green
    Assert-Equals -label 'swagger.statusCode' -actual $swagger.StatusCode -expected 200

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
    Assert-Equals -label 'create.success' -actual $createResponse.success -expected $true
    Assert-Equals -label 'create.code' -actual $createResponse.code -expected 'SUCCESS'

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
    Assert-Equals -label 'cancel.success' -actual $cancelResponse.success -expected $true
    Assert-Equals -label 'cancel.code' -actual $cancelResponse.code -expected 'SUCCESS'

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
        throw '재고 부족 테스트가 예상대로 실패하지 않았습니다.'
    }
    catch {
        $body = Get-ErrorBody $_
        Show-ApiError $_

        if ($body) {
            $json = $body | ConvertFrom-Json
            Assert-Equals -label 'shortage.code' -actual $json.Code -expected 'ERR_STOCK_SHORTAGE'
            Assert-Equals -label 'shortage.message' -actual $json.Message -expected '재고가 부족합니다.'
        }
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
