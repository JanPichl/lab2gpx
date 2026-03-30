
namespace Lab2Gpx
{
    using System.Text.Json.Serialization;

    internal class Models
    {
        internal record Settings(
            string? UserName,
            string? UserPassword,
            double Latitude,
            double Longitute = 14.4653933d,
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

        internal record AdventureSummary2(
            [property: JsonPropertyName("adventureGuid")] string? AdventureGuid,
            [property: JsonPropertyName("title")] string? Title
        );

        internal record AdventureSummary(
            [property: JsonPropertyName("adventureGuid")] string? AdventureGuid,
            [property: JsonPropertyName("title")] string? Title,
            [property: JsonPropertyName("keyImageUrl")] string? KeyImageUrl,
            [property: JsonPropertyName("smartLink")] string? SmartLink,
            [property: JsonPropertyName("deepLink")] string? DeepLink,
            [property: JsonPropertyName("firebaseDynamicLink")] string? FirebaseDynamicLink,
            [property: JsonPropertyName("medianTimeToComplete")] int MedianTimeToComplete,
            [property: JsonPropertyName("description")] string? Description,
            [property: JsonPropertyName("ownerPublicGuid")] string? OwnerPublicGuid,
            [property: JsonPropertyName("visibility")] string? Visibility,
            [property: JsonPropertyName("createdUtc")] DateTime? CreatedUtc,
            [property: JsonPropertyName("publishedUtc")] DateTime? PublishedUtc,
            [property: JsonPropertyName("isArchived")] bool IsArchived,
            [property: JsonPropertyName("ratingsAverage")] double RatingsAverage,
            [property: JsonPropertyName("ratingsTotalCount")] int RatingsTotalCount,
            [property: JsonPropertyName("isHighlyRecommended")] bool IsHighlyRecommended,
            [property: JsonPropertyName("location")] Location? Location,
            [property: JsonPropertyName("stagesTotalCount")] int StagesTotalCount,
            [property: JsonPropertyName("isTest")] bool IsTest,
            [property: JsonPropertyName("adventureType")] string? AdventureType,
            [property: JsonPropertyName("completionStatus")] string? CompletionStatus,
            [property: JsonPropertyName("adventureThemes")] List<string>? AdventureThemes,
            [property: JsonPropertyName("adventureCredit")] AdventureCredit? AdventureCredit
        );

        internal record AdventureDetail(

        //[property: JsonPropertyName("id")] string? Id,

        [property: JsonPropertyName("adventureGuid")]
        string? AdventureGuid,

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

        /*internal record Owner(
            [property: JsonPropertyName("name")] string? Name
        );*/

        /*internal record Stage(
            [property: JsonPropertyName("id")] string? Id,
            [property: JsonPropertyName("title")] string? Title,
            [property: JsonPropertyName("description")] string? Description,
            [property: JsonPropertyName("question")] string? Question,
            [property: JsonPropertyName("location")] Location? Location
        );*/

        internal record StageSummary(
            [property: JsonPropertyName("id")] string? Id,
            [property: JsonPropertyName("title")] string? Title,
            [property: JsonPropertyName("keyImageUrl")] string? KeyImageUrl,
            [property: JsonPropertyName("findCodeHashBase16v2")] string? FindCodeHashBase16v2,
            [property: JsonPropertyName("answerCodeHashesBase16v2")] List<string>? AnswerCodeHashesBase16v2,
            [property: JsonPropertyName("isComplete")] bool IsComplete,
            [property: JsonPropertyName("description")] string? Description,
            [property: JsonPropertyName("location")] Location? Location,
            [property: JsonPropertyName("geofencingRadius")] int GeofencingRadius,
            [property: JsonPropertyName("challengeType")] string? ChallengeType,
            [property: JsonPropertyName("question")] string? Question,
            [property: JsonPropertyName("isFinal")] bool IsFinal
);

        internal record AdventureCredit(
            [property: JsonPropertyName("isFeatured")] bool IsFeatured,
            [property: JsonPropertyName("isPartner")] bool IsPartner,
            [property: JsonPropertyName("maxStages")] int MaxStages,
            [property: JsonPropertyName("feeAllowed")] bool FeeAllowed,
            [property: JsonPropertyName("hyperlinksAllowed")] bool HyperlinksAllowed
        );
    }
}
