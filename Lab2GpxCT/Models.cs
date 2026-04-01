
namespace Lab2Gpx
{
    using System.Text.Json.Serialization;
    internal class Models
    {
        internal record Settings(
            string? UserName,
            string? UserPassword,
            double Latitude,
            double Longitude = 14.4653933d,
            double Radius = 500,
            double Limit = 30,
            string? OutputFileName = "labs.gpx"
            );

        internal record LoginResponse(
            [property: JsonPropertyName("accessToken")] string AccessToken,
            [property: JsonPropertyName("refreshToken")] string RefreshToken,
            [property: JsonPropertyName("expiresIn")] int ExpiresIn
            );

        internal record SearchResponse(
            [property: JsonPropertyName("items")] List<AdventureSummary> Items,
            [property: JsonPropertyName("totalCount")] int TotalCount
            );


        internal record AdventureSummary(
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("adventureGuid")] string? AdventureGuid,
            [property: JsonPropertyOrder(0)][property: JsonPropertyName("title")] string? Title,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("keyImageUrl")] string? KeyImageUrl,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("smartLink")] string? SmartLink,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("deepLink")] string? DeepLink,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("firebaseDynamicLink")] string? FirebaseDynamicLink,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("medianTimeToComplete")] int MedianTimeToComplete,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("description")] string? Description,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("ownerPublicGuid")] string? OwnerPublicGuid,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("visibility")] string? Visibility,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("createdUtc")] DateTime? CreatedUtc,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("publishedUtc")] DateTime? PublishedUtc,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("isArchived")] bool IsArchived,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("ratingsAverage")] double RatingsAverage,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("ratingsTotalCount")] int RatingsTotalCount,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("isHighlyRecommended")] bool IsHighlyRecommended,
            [property: JsonPropertyOrder(2)][property: JsonPropertyName("location")] Location? Location,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("stagesTotalCount")] int StagesTotalCount,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("isTest")] bool IsTest,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("adventureType")] string? AdventureType,
            [property: JsonPropertyOrder(1)][property: JsonPropertyName("completionStatus")] string? CompletionStatus,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("adventureThemes")] List<string>? AdventureThemes,
            [property: JsonPropertyOrder(100)][property: JsonPropertyName("adventureCredit")] AdventureCredit? AdventureCredit
            );

        internal record AdventureDetail(
            [property: JsonPropertyName("adventureGuid")] string? AdventureGuid,
            [property: JsonPropertyName("title")] string? Title,
            [property: JsonPropertyName("description")] string? Description,
            [property: JsonPropertyName("visibility")] string? Visibility,
            [property: JsonPropertyName("createdUtc")] DateTime? CreatedUtc,
            [property: JsonPropertyName("publishedUtc")] DateTime? PublishedUtc,
            [property: JsonPropertyName("isArchived")] bool IsArchived,
            [property: JsonPropertyName("completedStagesCount")] int CompletedStagesCount,
            [property: JsonPropertyName("keyImageUrl")] string? KeyImageUrl,
            [property: JsonPropertyName("location")] Location? Location,
            [property: JsonPropertyName("isTest")] bool IsTest,
            [property: JsonPropertyName("smartLink")] string? SmartLink,
            [property: JsonPropertyName("customAccessCode")] string? CustomAccessCode,
            [property: JsonPropertyName("deepLink")] string? DeepLink,
            [property: JsonPropertyName("firebaseDynamicLink")] string? FirebaseDynamicLink,
            [property: JsonPropertyName("stageSummaries")] List<StageSummary>? StageSummaries,
            [property: JsonPropertyName("journalsTotalCount")] int JournalsTotalCount,
            [property: JsonPropertyName("ownerPublicGuid")] string? OwnerPublicGuid,
            [property: JsonPropertyName("ownerUsername")] string? OwnerUsername,
            [property: JsonPropertyName("adventureType")] string? AdventureType,
            [property: JsonPropertyName("ratingsAverage")] double RatingsAverage,
            [property: JsonPropertyName("ratingsTotalCount")] int RatingsTotalCount,
            [property: JsonPropertyName("medianTimeToComplete")] int MedianTimeToComplete,
            [property: JsonPropertyName("reviewsTotalCount")] int ReviewsTotalCount,
            [property: JsonPropertyName("adventureThemes")] List<string>? AdventureThemes,
            [property: JsonPropertyName("isHighlyRecommended")] bool IsHighlyRecommended,
            [property: JsonPropertyName("completionStatus")] string? CompletionStatus,
            [property: JsonPropertyName("adventureCredit")] AdventureCredit? AdventureCredit,
            [property: JsonPropertyName("recommendedCount")] int RecommendedCount,
            [property: JsonPropertyName("completionCount")] int CompletionCount
            );

        internal record Location(
            [property: JsonPropertyName("latitude")] double Latitude,
            [property: JsonPropertyName("longitude")] double Longitude
            );

        internal record StageSummary(
            [property: JsonPropertyName("id")] string? Id,
            [property: JsonPropertyName("title")] string? Title,
            [property: JsonPropertyName("keyImageUrl")] string? KeyImageUrl,
            [property: JsonPropertyName("findCodeHashBase16v2")] string? FindCodeHashBase16v2,
            [property: JsonPropertyName("answerCodeHashesBase16v2")] List<string>? AnswerCodeHashesBase16v2,
            [property: JsonPropertyName("isComplete")] bool? IsComplete,
            [property: JsonPropertyName("description")] string? Description,
            [property: JsonPropertyName("location")] Location? Location,
            [property: JsonPropertyName("geofencingRadius")] int GeofencingRadius,
            [property: JsonPropertyName("challengeType")] string? ChallengeType,
            [property: JsonPropertyName("question")] string? Question,
            [property: JsonPropertyName("isFinal")] bool? IsFinal
            );

        internal record AdventureCredit(
            [property: JsonPropertyName("isFeatured")] bool IsFeatured,
            [property: JsonPropertyName("isPartner")] bool IsPartner,
            [property: JsonPropertyName("maxStages")] int MaxStages,
            [property: JsonPropertyName("feeAllowed")] bool FeeAllowed,
            [property: JsonPropertyName("hyperlinksAllowed")] bool HyperlinksAllowed
            );

        internal record AdventureDetailV2(
    [property: JsonPropertyName("Id")] string? Id,
    [property: JsonPropertyName("Title")] string? Title,
    [property: JsonPropertyName("Description")] string? Description,
    [property: JsonPropertyName("Visibility")] int? Visibility,
    [property: JsonPropertyName("IsPublic")] bool? IsPublic,
    [property: JsonPropertyName("PublishedUtc")] DateTime? PublishedUtc,
    [property: JsonPropertyName("CompletedGeocachesCount")] int? CompletedGeocachesCount,
    [property: JsonPropertyName("TotalGeocachesCount")] int? TotalGeocachesCount,
    [property: JsonPropertyName("StagesTotalCount")] int? StagesTotalCount,
    [property: JsonPropertyName("KeyImageUrl")] string? KeyImageUrl,
    [property: JsonPropertyName("Location")] LocationV2? Location,
    [property: JsonPropertyName("IsComplete")] bool? IsComplete,
    [property: JsonPropertyName("IsTest")] bool? IsTest,
    [property: JsonPropertyName("SmartLinkString")] string? SmartLinkString,
    [property: JsonPropertyName("DeepLink")] string? DeepLink,
    [property: JsonPropertyName("FirebaseDynamicLink")] string? FirebaseDynamicLink,
    [property: JsonPropertyName("ClonedSourceAdventureId")] string? ClonedSourceAdventureId,
    [property: JsonPropertyName("TestAdventureId")] string? TestAdventureId,
    [property: JsonPropertyName("LastUpdateDateTimeUtc")] DateTime? LastUpdateDateTimeUtc,
    [property: JsonPropertyName("AwardVideoYouTubeId")] string? AwardVideoYouTubeId,
    [property: JsonPropertyName("AwardMessage")] string? AwardMessage,
    [property: JsonPropertyName("GeocacheSummaries")] List<GeocacheSummaryV2>? GeocacheSummaries,
    [property: JsonPropertyName("OwnerUsername")] string? OwnerUsername,
    [property: JsonPropertyName("OwnerId")] string? OwnerId,
    [property: JsonPropertyName("AwardImageAsBase64String")] string? AwardImageAsBase64String,
    [property: JsonPropertyName("KeyImageAsBase64String")] string? KeyImageAsBase64String,
    [property: JsonPropertyName("IsLinear")] bool? IsLinear,
    [property: JsonPropertyName("AdventureType")] int? AdventureType,
    [property: JsonPropertyName("RatingsAverage")] double? RatingsAverage,
    [property: JsonPropertyName("RatingsTotalCount")] int? RatingsTotalCount,
    [property: JsonPropertyName("MedianTimeToComplete")] int? MedianTimeToComplete,
    [property: JsonPropertyName("ReviewsTotalCount")] int? ReviewsTotalCount,
    [property: JsonPropertyName("AdventureThemes")] List<string>? AdventureThemes
);

        internal record LocationV2(
            [property: JsonPropertyName("Latitude")] double Latitude,
            [property: JsonPropertyName("Longitude")] double Longitude,
            [property: JsonPropertyName("Altitude")] double? Altitude
        );

        internal record GeocacheSummaryV2(
            [property: JsonPropertyName("Id")] string? Id,
            [property: JsonPropertyName("Title")] string? Title,
            [property: JsonPropertyName("KeyImageUrl")] string? KeyImageUrl,
            [property: JsonPropertyName("FindCodeHashBase16")] string? FindCodeHashBase16,
            [property: JsonPropertyName("FindCodeHashBase16v2")] string? FindCodeHashBase16v2,
            [property: JsonPropertyName("IsComplete")] bool? IsComplete,
            [property: JsonPropertyName("AwardImageUrl")] string? AwardImageUrl,
            [property: JsonPropertyName("AwardVideoYouTubeId")] string? AwardVideoYouTubeId,
            [property: JsonPropertyName("CompletionAwardMessage")] string? CompletionAwardMessage,
            [property: JsonPropertyName("Description")] string? Description,
            [property: JsonPropertyName("Location")] LocationV2? Location,
            [property: JsonPropertyName("GeofencingRadius")] double? GeofencingRadius,
            [property: JsonPropertyName("ChallengeType")] int? ChallengeType,
            [property: JsonPropertyName("Question")] string? Question,
            [property: JsonPropertyName("CompletionCode")] string? CompletionCode,
            [property: JsonPropertyName("MultiChoiceOptions")] object? MultiChoiceOptions,
            [property: JsonPropertyName("IsFinal")] bool? IsFinal,
            [property: JsonPropertyName("UnlockingStages")] object? UnlockingStages,
            [property: JsonPropertyName("LastUpdateDateTimeUtc")] DateTime? LastUpdateDateTimeUtc,
            [property: JsonPropertyName("DescriptionVideoYouTubeId")] string? DescriptionVideoYouTubeId,
            [property: JsonPropertyName("AwardImageAsBase64String")] string? AwardImageAsBase64String,
            [property: JsonPropertyName("KeyImageAsBase64String")] string? KeyImageAsBase64String,
            [property: JsonPropertyName("DeleteAwardImage")] bool? DeleteAwardImage
        );
    }
}
