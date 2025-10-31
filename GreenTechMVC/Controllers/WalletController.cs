using BLL.Service.Payments.Interface;
using BLL.Service.Wallet.Interface;
using DAL.DTOs.Wallet;
using DAL.Models.Enum;
using Microsoft.AspNetCore.Mvc;

namespace GreenTechMVC.Controllers
{
    public class WalletController : Controller
    {
        private readonly IWalletService _walletService;
        private readonly IPaymentGatewayFactory _paymentFactory;

        public WalletController(IWalletService walletService, IPaymentGatewayFactory paymentFactory)
        {
            _walletService = walletService;
            _paymentFactory = paymentFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Return()
        {
            var query = HttpContext.Request.Query;
            var allParams = query.ToDictionary(q => q.Key, q => q.Value.ToString());

            // Detect payment gateway by callback parameters
            PaymentGateway? detectedGateway = null;
            if (query.Any(kv => kv.Key.StartsWith("vnp_")))
            {
                detectedGateway = PaymentGateway.VNPAY;
            }
            else if (query.ContainsKey("resultCode") || query.ContainsKey("orderId"))
            {
                detectedGateway = PaymentGateway.MOMO;
            }

            if (!detectedGateway.HasValue)
            {
                ViewBag.Gateway = "Unknown";
                ViewBag.Success = false;
                ViewBag.Message = "Không xác định được cổng thanh toán";
                return View();
            }

            // Get processor using Factory pattern
            var processor = _paymentFactory.Get(detectedGateway.Value);

            // Parse and verify callback
            var callbackParams = processor.ParseCallbackParams(allParams);

            if (!processor.VerifyCallback(callbackParams))
            {
                ViewBag.Gateway = detectedGateway.Value.ToString();
                ViewBag.Success = false;
                ViewBag.Message = "Xác thực callback không thành công";
                return View();
            }

            // Extract transaction result
            var (isSuccess, gatewayTransactionId, amount, message) =
                processor.ExtractCallbackResult(callbackParams);

            // Set view data
            ViewBag.Gateway = detectedGateway.Value.ToString();
            ViewBag.Success = isSuccess;
            ViewBag.Message = message;

            // Set gateway-specific view data
            if (detectedGateway.Value == PaymentGateway.VNPAY)
            {
                ViewBag.ResponseCode = callbackParams.GetValueOrDefault("vnp_ResponseCode");
                ViewBag.TransactionRef = gatewayTransactionId;
                ViewBag.TransactionNo = callbackParams.GetValueOrDefault("vnp_TransactionNo");
                ViewBag.TransactionStatus = callbackParams.GetValueOrDefault(
                    "vnp_TransactionStatus"
                );
            }
            else if (detectedGateway.Value == PaymentGateway.MOMO)
            {
                ViewBag.ResultCode = callbackParams.GetValueOrDefault("resultCode");
                ViewBag.OrderId = gatewayTransactionId;
                ViewBag.TransId = callbackParams.GetValueOrDefault("transId");
                ViewBag.Amount = amount?.ToString();
            }

            // Confirm transaction if successful
            if (isSuccess && !string.IsNullOrWhiteSpace(gatewayTransactionId))
            {
                await _walletService.ConfirmTopUpSuccessByGatewayIdAsync(
                    gatewayTransactionId,
                    amount
                );
            }

            // NOTE: Trạng thái cuối cùng nên xác nhận bằng IPN (ipnUrl) để tránh giả mạo redirect
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Return(
            [FromBody] System.Text.Json.JsonElement? payload = null
        )
        {
            var request = HttpContext.Request;
            var allParams = new Dictionary<string, string>();

            // Detect payment gateway and parse parameters
            PaymentGateway? detectedGateway = null;

            // Check for VNPay IPN (form data)
            if (request.HasFormContentType && request.Form.Any(kv => kv.Key.StartsWith("vnp_")))
            {
                detectedGateway = PaymentGateway.VNPAY;
                foreach (var kvp in request.Form)
                {
                    allParams[kvp.Key] = kvp.Value.ToString();
                }
            }
            // Check for MoMo IPN (JSON payload)
            else if (payload.HasValue)
            {
                detectedGateway = PaymentGateway.MOMO;
                var jsonPayload = payload.Value;

                // Convert JSON to dictionary
                foreach (var prop in jsonPayload.EnumerateObject())
                {
                    allParams[prop.Name] =
                        prop.Value.ValueKind == System.Text.Json.JsonValueKind.String
                            ? prop.Value.GetString() ?? string.Empty
                            : prop.Value.ToString();
                }
            }

            if (!detectedGateway.HasValue)
            {
                return Ok(new { resultCode = 2001, message = "Unknown payment gateway" });
            }

            // Get processor using Factory pattern
            var processor = _paymentFactory.Get(detectedGateway.Value);

            // Parse and verify callback
            var callbackParams = processor.ParseCallbackParams(allParams);

            if (!processor.VerifyCallback(callbackParams))
            {
                if (detectedGateway.Value == PaymentGateway.VNPAY)
                {
                    return Ok(new { RspCode = "97", Message = "Checksum failed" });
                }
                return Ok(new { resultCode = 2001, message = "Verification failed" });
            }

            // Extract transaction result
            var (isSuccess, gatewayTransactionId, amount, _) = processor.ExtractCallbackResult(
                callbackParams
            );

            // Confirm transaction if successful
            if (isSuccess && !string.IsNullOrWhiteSpace(gatewayTransactionId))
            {
                await _walletService.ConfirmTopUpSuccessByGatewayIdAsync(
                    gatewayTransactionId,
                    amount
                );
            }

            // Return appropriate response format for each gateway
            if (detectedGateway.Value == PaymentGateway.VNPAY)
            {
                return Ok(
                    new
                    {
                        RspCode = isSuccess ? "00" : "99",
                        Message = isSuccess ? "Confirm Success" : "Confirm Failed",
                    }
                );
            }
            else // MoMo
            {
                return Ok(
                    new { resultCode = isSuccess ? 0 : 2001, message = isSuccess ? "OK" : "FAILED" }
                );
            }
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");
            var balance = await _walletService.GetWalletBalanceAsync(userId);
            ViewBag.WalletBalance = balance;
            return View();
        }

        [HttpGet]
        public IActionResult TopUp(decimal? amount, string? gateway = null)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            // Validate and set default gateway
            var selectedGateway = gateway;
            if (
                string.IsNullOrEmpty(selectedGateway)
                || !Enum.TryParse<PaymentGateway>(selectedGateway, true, out _)
            )
            {
                // Default to VNPAY if invalid or not provided
                selectedGateway = PaymentGateway.VNPAY.ToString();
            }

            var model = new TopUpRequestDTO
            {
                UserId = userId,
                Amount =
                    amount.HasValue && amount.Value > 0 ? decimal.Round(amount.Value, 0) : 100000,
                Gateway = selectedGateway,
                Description = "Nạp tiền vào ví GreenTech",
            };

            // Pass available gateways to view
            ViewBag.AvailableGateways = Enum.GetNames(typeof(PaymentGateway))
                .Select(name => new
                {
                    Value = name,
                    DisplayName = name == "VNPAY" ? "VNPay" : "MoMo",
                })
                .ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopUp(TopUpRequestDTO request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");
            request.UserId = userId;
            if (request.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Số tiền phải lớn hơn 0");
                return View(request);
            }

            var resp = await _walletService.TopUpAsync(request);
            if (!string.IsNullOrWhiteSpace(resp.PayUrl))
            {
                return Redirect(resp.PayUrl);
            }
            TempData["WalletMessage"] = "Khởi tạo thanh toán thất bại";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetBalance()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Json(new { success = false, balance = 0 });
            var balance = await _walletService.GetWalletBalanceAsync(userId);
            return Json(new { success = true, balance = balance });
        }

        private int GetCurrentUserId()
        {
            var userIdFromSession = HttpContext.Session.GetInt32("UserId");
            if (userIdFromSession.HasValue)
                return userIdFromSession.Value;
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int uid))
                return uid;
            return 0;
        }
    }
}
