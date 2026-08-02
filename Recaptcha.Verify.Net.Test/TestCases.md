# Test Cases

## Attribute Tests (`Attribute/AttributeTest.cs`)

Tests for `RecaptchaAttribute` behavior in various scenarios.

| # | Test | Type | Description |
|---|---|---|---|
| 1 | `Execute_NoTokenExtractionService_ThrowsRecaptchaUnknownException` | Fact | When no `IRecaptchaTokenExtractionService` is registered, throws `RecaptchaUnknownException` with inner `InvalidOperationException`. Action pipeline is not invoked. |
| 2 | `Execute_TokenExtractionServiceThrowsTokenExtractorNotFound_PropagatesException` | Fact | When token extraction service throws `TokenExtractorNotFound`, the exception propagates. Action pipeline is not invoked. |
| 3 | `Execute_TokenExtractionServiceThrowsEmptyCaptchaAnswer_PropagatesException` | Fact | When token extraction service throws `EmptyCaptchaAnswerException`, the exception propagates. Action pipeline is not invoked. |
| 4 | `Execute_v2_Successful` | Theory | Successful v2 flow: token extracted, verification succeeds, validation passes. Action pipeline invoked once, no result set. Parameters: `useCancellationToken` ∈ {false, true}. |
| 5 | `Execute_v3_Successful` | Theory | Successful v3 flow with all combinations of `useCancellationToken` × `action` × `score`. Action pipeline invoked once, no result set. 8 parameter combinations. |
| 6 | `Execute_Unsuccessful_ReturnsBadRequest` | Theory | When validation returns unsuccessful, sets `BadRequestObjectResult` (400). Action pipeline not invoked. 8 parameter combinations. |
| 7 | `Execute_VerificationThrows_PropagatesException` | Theory | When verification service throws, it is wrapped in `RecaptchaUnknownException` with original as `InnerException`. Token extraction was called, action pipeline not invoked. 8 parameter combinations. |
| 8 | `Execute_ValidationThrows_PropagatesException` | Theory | When validation service throws, it is wrapped in `RecaptchaUnknownException` with original as `InnerException`. Token extraction was called, action pipeline not invoked. 8 parameter combinations. |

## Configuration Extensions Tests (`Configuration/ConfigurationExtensionsTest.cs`)

Tests for `AddRecaptcha` service registration: nested options wiring, `RecaptchaClient` base URL configuration, token-extractor auto-registration, and backward compatibility.

| # | Test | Type | Description |
|---|---|---|---|
| 1 | `AddRecaptcha_DefaultBaseUrl_WhenNotSpecified` | Fact | When `Verification.BaseUrl` is not set, `RecaptchaClient` uses default Google URL `https://www.google.com/recaptcha/api/`. |
| 2 | `AddRecaptcha_CustomBaseUrl_WhenSpecified` | Fact | When custom `Verification.BaseUrl` is specified, `RecaptchaClient` is configured with that URL (trailing slash appended). |
| 3 | `AddRecaptcha_DefaultBaseUrl_WhenNullOrEmpty` | Theory | When `Verification.BaseUrl` is null, empty, or whitespace, `RecaptchaClient` falls back to default Google URL. Parameters: `baseUrl` ∈ {null, "", "   "}. |
| 4 | `AddRecaptcha_AppendsTrailingSlash_WhenMissing` | Fact | If custom `Verification.BaseUrl` does not end with `/`, one is automatically appended. |
| 5 | `AddRecaptcha_PreservesTrailingSlash_WhenPresent` | Fact | If custom `Verification.BaseUrl` already ends with `/`, it is preserved as-is (no double slash). |
| 6 | `AddRecaptcha_RegistersNestedOptionsAsFocused` | Fact | Settings on the nested `Verification`/`Validation`/`Attribute` groups are registered as the corresponding focused `IOptions<>` (`SecretKey`, `Action`/`ScoreThreshold`, `UseCancellationToken`/`VerificationFailedMessage`). |
| 7 | `AddRecaptcha_LegacyFlatOptions_FlowThroughObsoleteProxy` | Fact | The obsolete flat root fields still flow through the proxies into the same focused options (backward compatibility). |
| 8 | `AddRecaptcha_TokenExtractors_Header_RegistersHeaderExtractor` | Fact | Setting `TokenExtractors.Header` registers a single `HeaderTokenExtractor`. |
| 9 | `AddRecaptcha_TokenExtractors_Form_RegistersFormExtractor` | Fact | Setting `TokenExtractors.Form` registers a single `FormTokenExtractor`. |
| 10 | `AddRecaptcha_TokenExtractors_Query_RegistersQueryExtractor` | Fact | Setting `TokenExtractors.Query` registers a single `QueryTokenExtractor` (not recommended for security; covered for backward compatibility). |
| 11 | `AddRecaptcha_TokenExtractors_ActionArgumentsDelegate_RegistersExtractor` | Fact | Setting `TokenExtractors.GetResponseTokenFromActionArguments` registers a single `ActionArgumentsTokenExtractor`. |
| 12 | `AddRecaptcha_TokenExtractors_ActionArgumentName_RegistersExtractor` | Fact | Setting `TokenExtractors.ActionArgument` registers a single `ActionArgumentsTokenExtractor` (extracts token by action argument name). |
| 13 | `AddRecaptcha_TokenExtractors_ExecutingContextDelegate_RegistersExtractor` | Fact | Setting `TokenExtractors.GetResponseTokenFromExecutingContext` registers a single `ExecutingContextTokenExtractor`. |
| 14 | `AddRecaptcha_TokenExtractors_TakesPrecedenceOverAttributeOptions` | Fact | When both `TokenExtractors.Header` and legacy `AttributeOptions.ResponseTokenNameInHeader` are set, only one extractor is registered (new value wins). |
| 15 | `AddRecaptcha_FallsBackToAttributeOptions_WhenTokenExtractorsEmpty` | Fact | When `TokenExtractors.Header` is empty, the legacy `AttributeOptions.ResponseTokenNameInHeader` is used (backward compatibility). |
| 16 | `AddRecaptcha_TokenExtractors_FromConfigurationSection` | Fact | `Verification:SecretKey` and `TokenExtractors:Header`/`Form`/`Query`/`ActionArgument` bind from an `IConfigurationSection` and register the corresponding extractors. |
| 17 | `AddRecaptcha_ActionsScoreThresholds_BindsFromNestedConfigurationSection` | Fact | `Validation:ActionsScoreThresholds` binds automatically from an `IConfigurationSection` into `RecaptchaValidationOptions.ActionsScoreThresholds` (no explicit bind line needed). |
| 18 | `AddRecaptcha_ActionsScoreThresholds_BindsFromLegacyFlatConfigurationSection` | Fact | Legacy flat `ActionsScoreThresholds` at root binds into `RecaptchaValidationOptions.ActionsScoreThresholds` via the obsolete root proxy (backward compatibility). |
| 19 | `AddRecaptcha_NoExtractors_WhenNothingConfigured` | Fact | With no token-extractor configuration, no `IRecaptchaTokenExtractor` is registered. |
| 20 | `AddRecaptcha_AppliesTimeoutToHttpClient_WhenConfigured` | Fact | A configured `Verification.Timeout` is applied to the `HttpClient.Timeout`. |
| 21 | `AddRecaptcha_DefaultTimeout_IsTenSeconds` | Fact | The default `Verification.Timeout` is 10 seconds. |
| 22 | `AddRecaptcha_KeepsHttpClientDefaultTimeout_WhenTimeoutIsZero` | Fact | `Verification.Timeout = TimeSpan.Zero` keeps the `HttpClient` default timeout (100s). |
| 23 | `AddRecaptcha_AppliesConfigureHttpClientAction` | Fact | The optional `configureHttpClient` action runs after the library defaults, so it can add default request headers and override `Timeout`. |

## Token Extraction Tests (`TokenExtraction/TokenExtractionTest.cs`)

Tests for individual token extractor implementations.

| # | Test | Type | Description |
|---|---|---|---|
| 1 | `Extract_FromActionArguments_WithAction` | Fact | `ActionArgumentsTokenExtractor` with lambda extracts token from action arguments. |
| 2 | `Extract_FromActionArguments_WithName` | Fact | `ActionArgumentsTokenExtractor` with parameter name extracts token from action arguments by key. |
| 3 | `Extract_FromExecutingContext` | Fact | `ExecutingContextTokenExtractor` extracts token from `ActionExecutingContext`. |
| 4 | `Extract_FromForm` | Fact | `FormTokenExtractor` extracts token from HTTP form data by field name. |
| 5 | `Extract_FromHeader` | Fact | `HeaderTokenExtractor` extracts token from HTTP request headers by header name. |
| 6 | `Extract_FromQuery` | Fact | `QueryTokenExtractor` extracts token from HTTP query string by parameter name. |

## Token Extraction Service Tests (`TokenExtraction/TokenExtractionServiceTest.cs`)

Tests for `RecaptchaTokenExtractionService` — first-wins token extraction strategy.

| # | Test | Type | Description |
|---|---|---|---|
| 1 | `GetToken_NoExtractors_ThrowsTokenExtractorNotFound` | Fact | Calling `GetToken` with no registered extractors throws `TokenExtractorNotFound`. |
| 2 | `GetToken_AllExtractorsReturnEmpty_ThrowsEmptyCaptchaAnswer` | Fact | When all extractors return null/empty/whitespace, throws `EmptyCaptchaAnswerException`. |
| 3 | `GetToken_SingleExtractorReturnsToken_ReturnsToken` | Fact | When a single extractor returns a valid token, the service returns it. |
| 4 | `GetToken_FirstEmptySecondReturnsToken_ReturnsToken` | Fact | When the first extractor returns null and the second returns a valid token, the service returns the second one (fallback). |
| 5 | `GetToken_FirstWins_SecondExtractorNotCalled` | Fact | When the first extractor returns a valid token, the second extractor is never called (short-circuit). |

## Verification Service Tests (`TokenVerification/VerificationServiceTest.cs`)

Tests for `RecaptchaVerificationService` — token verification via Google's API.

| # | Test | Type | Description |
|---|---|---|---|
| 1 | `Verify_MissingSecretKey_Throws` | Theory | Calling `VerifyAsync` with null/empty/whitespace secret key throws `SecretKeyNotSpecifiedException`. Parameters: `secretKey` ∈ {null, "", "   "}. |
| 2 | `Verify_EmptyResponse_Throws` | Theory | Calling `VerifyAsync` with null/empty/whitespace response token throws `EmptyCaptchaAnswerException`. Parameters: `response` ∈ {null, "", "   "}. |
| 3 | `Verify_ClientException_Throws` | Fact | When the HTTP client throws during verification, the service wraps it in `VerifyRequestException`. |
| 4 | `Verify_InvalidResponseToken_ReturnsVerificationResult` | Fact | Using an invalid response token returns `VerifyResponse` with `Success=false`. |
| 5 | `Verify_ValidResponseToken_ReturnsVerificationResult` | Theory | Using a valid response token returns `VerifyResponse` with `Success=true` and expected score. Parameters: valid tokens from fixture. |
| 6 | `Verify_HttpTimeout_ThrowsVerifyRequestException` | Fact | When the siteverify HTTP call exceeds the configured timeout, it surfaces as `VerifyRequestException` (inner `OperationCanceledException`). |

## Validation Service Tests (`VerificationResultValidation/ValidationServiceTest.cs`)

Tests for `RecaptchaVerificationResultValidationService` — verification result validation logic.

| # | Test | Type | Description |
|---|---|---|---|
| 1 | `Validate_v3_EmptyAction_Throws` | Theory | Calling `Validate` with empty/null/whitespace action on v3 result throws `EmptyActionException`. Tests both direct and constructor-based action passing. Parameters: `action` ∈ {null, "", "   "}. |
| 2 | `Validate_v3_ScoreNotSpecified_Throws` | Fact | When no score threshold is configured (globally or per-action), validating a v3 result throws `MinScoreNotSpecifiedException`. Tests both options-based and direct-call paths. |
| 3 | `Validate_v3_UnsuccessfulVerification` | Fact | Unsuccessful verification produces `ValidationResult` with all flags false: `IsV3=false`, `ResponseSuccessful=false`, `ActionMatches=false`, `ScoreSatisfies=false`, `Success=false`. |
| 4 | `Validate_v2_SuccessfulVerification` | Fact | Successful v2 verification (no score/action) produces `IsV3=false`, `ResponseSuccessful=true`, `ActionMatches=false`, `ScoreSatisfies=false`, `Success=true`. |
| 5 | `Validate_v3_SuccessfulVerification_WithScoreThreshold` | Theory | v3 validation with global score threshold via options. Checks `IsV3=true`, `ResponseSuccessful=true`, `ActionMatches=true`, and `ScoreSatisfies`/`Success` based on score vs threshold. Parameters: verification results with varying scores. |
| 6 | `Validate_v3_SuccessfulVerification_WithActionsScoreThresholds` | Theory | v3 validation with per-action score thresholds via `ActionsScoreThresholds`. Same assertions as above but using action-to-score mappings. Parameters: verification results with varying scores. |
| 7 | `Validate_v3_SuccessfulVerification_WithScoreThresholdDirectly` | Theory | v3 validation when action and score are passed directly to `Validate`. Confirms score-satisfies logic with direct parameters. Parameters: verification results with varying scores. |
| 8 | `Validate_v3_SuccessfulVerification_WithScoreThresholdDirectly_OverridesFromOptions` | Theory | Directly passed action/score override values from options. Service is initialized with different options but direct parameters take precedence. Parameters: verification results with varying scores. |

## Logger Extensions Tests (`Logging/LoggerExtensionsTest.cs`)

Tests for `LoggerExtensions.SendingRequest` — verify the secret key is never leaked into logs, in either the formatted message text or the structured `Data` log property (which holds a redacted string projection rather than the `VerifyRequest` object).

| # | Test | Type | Description |
|---|---|---|---|
| 1 | `SendingRequest_NeverEmitsSensitiveData` | Fact | The secret, response token, and remote IP never appear in the formatted message or the structured `Data` property; `Data` shows `Secret=***`, `Response=<length=…>`, and `RemoteIp=***`. |

## HTTP Error Tests (`TokenVerification/HttpErrorTest.cs`)

Tests for HTTP transport-failure handling in `RecaptchaClient` / `RecaptchaVerificationService` — non-success status codes preserve the status code and a response-body snippet in the surfaced exception.

| # | Test | Type | Description |
|---|---|---|---|
| 1 | `Verify_HttpFailure_PreservesStatusAndBodyInException` | Fact | An HTTP failure (e.g. `500`) through `RecaptchaVerificationService` is wrapped in a `VerifyRequestException` whose message carries the status code and a (capped) body snippet, with the `HttpRequestException` as `InnerException`; a long body is truncated. |
